using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct FindTargetSystem : ISystem
    {
        private ComponentLookup<LocalTransform> transformLookup;
        private ComponentLookup<Faction> factionLookup;
        public EntityStorageInfoLookup entityStorage;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            transformLookup = state.GetComponentLookup<LocalTransform>(true);
            factionLookup = state.GetComponentLookup<Faction>(true);
            entityStorage = state.GetEntityStorageInfoLookup();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            transformLookup.Update(ref state);
            factionLookup.Update(ref state);
            entityStorage.Update(ref state);

            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            CollisionFilter filter = new CollisionFilter
            {
                BelongsTo = ~0u,        //Bitmasks, all bits 1
                CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                GroupIndex = 0
            };

            var job = new FindTargetJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                collision = collision,
                filter = filter,
                transformLookup = transformLookup,
                factionLookup = factionLookup,
                entityStorage = entityStorage
            };
            job.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct FindTargetJob: IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;
        [ReadOnly] public ComponentLookup<Faction> factionLookup;
        [ReadOnly] public EntityStorageInfoLookup entityStorage;

        [ReadOnly] public float deltaTime;
        [ReadOnly] public CollisionWorld collision;
        [ReadOnly] public CollisionFilter filter;

        public void Execute(in LocalTransform transf, ref FindTarget findTarget, ref Target target, in TargetOverride targetOverride)
        {
            //Run the logic only once in a while
            findTarget.timer -= deltaTime;
            if (findTarget.timer > 0.0f)
                return;
            findTarget.timer += findTarget.timerMax;


            if (targetOverride.target != Entity.Null)
            {
                target.target = targetOverride.target;
                return;
            }

            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.TempJob);

            Entity closestTarget = Entity.Null;
            float closestDistance = float.MaxValue;
            float closestDistanceOffset = 0f;
            if (target.target != Entity.Null)
            {
                closestTarget = target.target;
                var targetTransf = transformLookup[target.target];
                closestDistance = math.distance(transf.Position, targetTransf.Position);
                closestDistanceOffset = 2f;
            }

            if (collision.OverlapSphere(transf.Position, findTarget.range, ref distanceHits, filter))
            {
                foreach (DistanceHit hit in distanceHits)
                {
                    if (!entityStorage.Exists(hit.Entity) || !factionLookup.HasComponent(hit.Entity))
                        continue;

                    Faction unit = factionLookup[hit.Entity];
                    if (findTarget.targetFaction == unit.faction)
                    {
                        if (closestTarget == Entity.Null)
                        {
                            closestTarget = hit.Entity;
                            closestDistance = hit.Distance;
                        }
                        else if (hit.Distance + closestDistanceOffset < closestDistance)
                        {
                            closestDistance = hit.Distance;
                            closestTarget = hit.Entity;
                        }
                    }
                }
            }
            if (closestTarget != Entity.Null)
                target.target = closestTarget;
        }
    }
}