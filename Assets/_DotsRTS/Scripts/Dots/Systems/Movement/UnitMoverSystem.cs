using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor.PackageManager.Requests;

namespace DotsRTS
{
    partial struct UnitMoverSystem : ISystem
    {
        public const float REACH_DIST_SQ = 2f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridSystem.GridSystemData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gridData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.PATHFINDING_WALLS,
                GroupIndex = 0
            };

            foreach (var (transf, targetQueue, enableTargetQueue, request, enableRequest, mover, enableFollower, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<TargetPositionPathQueue>, EnabledRefRW<TargetPositionPathQueue>, RefRW<FlowFieldPathRequest>, EnabledRefRW<FlowFieldPathRequest>, RefRW<UnitMover>, EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldPathRequest, FlowFieldFollower>().WithEntityAccess())
            {
                RaycastInput ray = new RaycastInput
                {
                    Start = transf.ValueRO.Position,
                    End = targetQueue.ValueRO.targetPos,
                    Filter = filter
                };
                if(!collision.CastRay(ray))
                {
                    mover.ValueRW.targetPosition = targetQueue.ValueRO.targetPos;
                    enableRequest.ValueRW = false;
                    enableFollower.ValueRW = false;
                }
                else
                {
                    if(SystemAPI.HasComponent<MoveOverride>(entity))
                    {
                        SystemAPI.SetComponentEnabled<MoveOverride>(entity, false);
                    }

                    if (GridSystem.IsValidWalkablePosition(targetQueue.ValueRO.targetPos, gridData))
                    {
                        request.ValueRW.targetPos = targetQueue.ValueRO.targetPos;
                        enableRequest.ValueRW = true;
                    }
                    else
                    {
                        mover.ValueRW.targetPosition = transf.ValueRO.Position;
                        enableRequest.ValueRW = false;
                        enableFollower.ValueRW = false;
                    }
                }
                enableTargetQueue.ValueRW = false;
            }

            foreach (var (transf, follower, enableFollower, mover) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>, RefRW<UnitMover>>())
            {
                var pos = GridSystem.GetGridPosition(transf.ValueRO.Position, gridData.gridNodeSize);
                int index = GridSystem.CalculateIndex(pos, gridData.width);
                Entity entity = gridData.gridMap[follower.ValueRO.gridIndex].gridEntities[index];
                var node = SystemAPI.GetComponent<GridSystem.GridNode>(entity);
                var moveVect = GridSystem.GetWorldMovementVector(node.vector);

                if(GridSystem.IsWall(node))
                {
                    moveVect = follower.ValueRO.lastMoveVect;
                }
                else
                {
                    follower.ValueRW.lastMoveVect = moveVect;
                }
                mover.ValueRW.targetPosition = GridSystem.GetWorldCenterPosition(pos.x, pos.y, gridData.gridNodeSize) + moveVect * gridData.gridNodeSize * 2f;

                if(math.distance(transf.ValueRO.Position, follower.ValueRO.targetPos) < gridData.gridNodeSize)
                {
                    mover.ValueRW.targetPosition = transf.ValueRO.Position;
                    enableFollower.ValueRW = false;
                }

                RaycastInput ray = new RaycastInput
                {
                    Start = transf.ValueRO.Position,
                    End = follower.ValueRO.targetPos,
                    Filter = filter
                };
                if (!collision.CastRay(ray))
                {
                    mover.ValueRW.targetPosition = follower.ValueRO.targetPos;
                    enableFollower.ValueRW = false;
                }
            }

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
}