using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct UnitMoverSystem : ISystem
    {
        public const float REACH_DIST_SQ = 2f;

        public ComponentLookup<TargetPositionPathQueue> pathQueueLookup;
        public ComponentLookup<FlowFieldPathRequest> requestLookup;
        public ComponentLookup<FlowFieldFollower> followerLookup;
        public ComponentLookup<MoveOverride> moveOverrideLookup;
        public ComponentLookup<GridSystem.GridNode> gridNodeLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridSystem.GridSystemData>();

            pathQueueLookup = SystemAPI.GetComponentLookup<TargetPositionPathQueue>(false);
            requestLookup = SystemAPI.GetComponentLookup<FlowFieldPathRequest>(false);
            followerLookup = SystemAPI.GetComponentLookup<FlowFieldFollower>(false);
            moveOverrideLookup = SystemAPI.GetComponentLookup<MoveOverride>(false);
            gridNodeLookup = SystemAPI.GetComponentLookup<GridSystem.GridNode>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gridData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

            pathQueueLookup.Update(ref state);
            requestLookup.Update(ref state);
            followerLookup.Update(ref state);
            moveOverrideLookup.Update(ref state);
            gridNodeLookup.Update(ref state);

            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                GroupIndex = 0
            };

            TargetPositionPathQueueJob pathQueueJob = new TargetPositionPathQueueJob
            {
                width = gridData.width,
                height = gridData.height,
                gridNodeSize = gridData.gridNodeSize,
                collision = collision,
                filter = filter,
                costMap = gridData.costMap,

                pathQueueLookup = pathQueueLookup,
                followerLookup = followerLookup,
                moveOverrideLookup = moveOverrideLookup,
                requestLookup = requestLookup,
            };
            pathQueueJob.ScheduleParallel();

            FlowFieldFollowerJob flowFieldFollowerJob = new FlowFieldFollowerJob
            {
                width = gridData.width,
                height = gridData.height,
                gridNodeSize = gridData.gridNodeSize,
                totalGridMapEntities = gridData.totalGridMapEntityArray,

                followerLookup = followerLookup,
                gridNodeLookup = gridNodeLookup,
            };
            flowFieldFollowerJob.ScheduleParallel();

            TestCanMoveStraightJob canMoveStraightJob = new TestCanMoveStraightJob
            {
                filter = filter,
                collision = collision,
                followerLookup = followerLookup,
            };
            canMoveStraightJob.ScheduleParallel();

            UnitMoverJob job = new UnitMoverJob
            {
                deltaTime = SystemAPI.Time.DeltaTime
            };
            job.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct UnitMoverJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(ref LocalTransform transf, ref PhysicsVelocity physics, ref UnitMover movement)
        {
            float3 targetPosition = movement.targetPosition;
            float3 moveDir = targetPosition - transf.Position;
            if (math.lengthsq(moveDir) <= UnitMoverSystem.REACH_DIST_SQ)
            {
                physics.Linear = float3.zero;
                physics.Angular = float3.zero;
                movement.isMoving = false;
                return;
            }
            moveDir = math.normalize(moveDir);

            quaternion targetRot = quaternion.LookRotation(moveDir, math.up());
            transf.Rotation = math.slerp(transf.Rotation, targetRot, movement.rotationSpeed * deltaTime);

            physics.Linear = moveDir * movement.movementSpeed;
            physics.Angular = float3.zero;
            movement.isMoving = true;
        }
    }

    [BurstCompile]
    [WithAll(typeof(TargetPositionPathQueue))]
    public partial struct TargetPositionPathQueueJob: IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<TargetPositionPathQueue> pathQueueLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldPathRequest> requestLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> followerLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<MoveOverride> moveOverrideLookup;

        [ReadOnly] public CollisionWorld collision;
        [ReadOnly] public CollisionFilter filter;
        [ReadOnly] public float gridNodeSize;
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public NativeArray<byte> costMap;

        public void Execute(in LocalTransform transf, ref UnitMover mover, Entity entity)
        {
            RaycastInput ray = new RaycastInput
            {
                Start = transf.Position,
                End = pathQueueLookup[entity].targetPos,
                Filter = filter
            };
            if (!collision.CastRay(ray))
            {
                mover.targetPosition = pathQueueLookup[entity].targetPos;
                requestLookup.SetComponentEnabled(entity, false);
                followerLookup.SetComponentEnabled(entity, false);
            }
            else
            {
                if (moveOverrideLookup.HasComponent(entity))
                {
                    moveOverrideLookup.SetComponentEnabled(entity, false);
                }

                if (GridSystem.IsValidWalkablePosition(pathQueueLookup[entity].targetPos, gridNodeSize, width, height, costMap))
                {
                    var request = requestLookup[entity];
                    request.targetPos = pathQueueLookup[entity].targetPos;
                    requestLookup[entity] = request;
                    requestLookup.SetComponentEnabled(entity, true);
                }
                else
                {
                    mover.targetPosition = transf.Position;
                    requestLookup.SetComponentEnabled(entity, false);
                    followerLookup.SetComponentEnabled(entity, false);
                }
            }
            pathQueueLookup.SetComponentEnabled(entity, false);
        }
    }

    [BurstCompile]
    [WithAll(typeof(FlowFieldFollower))]
    public partial struct FlowFieldFollowerJob: IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> followerLookup;
        [ReadOnly] public ComponentLookup<GridSystem.GridNode> gridNodeLookup;

        [ReadOnly] public float gridNodeSize;
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public NativeArray<Entity> totalGridMapEntities;

        public void Execute(in LocalTransform transf, ref UnitMover mover, Entity entity)
        {
            var pos = GridSystem.GetGridPosition(transf.Position, gridNodeSize);
            int index = GridSystem.CalculateIndex(pos, width);

            int totalCount = width * height;
            Entity gridNodeEntity = totalGridMapEntities[totalCount * followerLookup[entity].gridIndex + index];
            var node = gridNodeLookup[gridNodeEntity];
            var moveVect = GridSystem.GetWorldMovementVector(node.vector);

            if (GridSystem.IsWall(node))
            {
                moveVect = followerLookup[entity].lastMoveVect;
            }
            else
            {
                FlowFieldFollower follower = followerLookup[entity];
                follower.lastMoveVect = moveVect;
                followerLookup[entity] = follower;
            }
            mover.targetPosition = GridSystem.GetWorldCenterPosition(pos.x, pos.y, gridNodeSize) + moveVect * gridNodeSize * 2f;

            if (math.distance(transf.Position, followerLookup[entity].targetPos) < gridNodeSize)
            {
                mover.targetPosition = transf.Position;
                followerLookup.SetComponentEnabled(entity, false);
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(FlowFieldFollower))]
    public partial struct TestCanMoveStraightJob: IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> followerLookup;

        [ReadOnly] public CollisionWorld collision;
        [ReadOnly] public CollisionFilter filter;

        public void Execute(in LocalTransform transf, ref UnitMover mover, Entity entity)
        {
            RaycastInput ray = new RaycastInput
            {
                Start = transf.Position,
                End = followerLookup[entity].targetPos,
                Filter = filter
            };
            if (!collision.CastRay(ray))
            {
                mover.targetPosition = followerLookup[entity].targetPos;
                followerLookup.SetComponentEnabled(entity, false);
            }
        }
    }
}