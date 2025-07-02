using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace DotsRTS
{
    partial struct FindTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;
            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);

            CollisionFilter filter = new CollisionFilter
            {
                BelongsTo = ~0u,        //Bitmasks, all bits 1
                CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                GroupIndex = 0
            };

            foreach (var (transf, findTarget, target, targetOverride) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FindTarget>, RefRW<Target>, RefRO<TargetOverride>>())
            {
                //Run the logic only once in a while
                findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (findTarget.ValueRO.timer > 0.0f)
                    continue;
                findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;


                if (targetOverride.ValueRO.target != Entity.Null)
                {
                    target.ValueRW.target = targetOverride.ValueRO.target;
                    continue;
                }

                if (distanceHits.Length > 0)
                    distanceHits.Clear();

                Entity closestTarget = Entity.Null;
                float closestDistance = float.MaxValue;
                float closestDistanceOffset = 0f;
                if(target.ValueRO.target != Entity.Null)
                {
                    closestTarget = target.ValueRO.target;
                    var targetTransf = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.target);
                    closestDistance = math.distance(transf.ValueRO.Position, targetTransf.Position);
                    closestDistanceOffset = 2f;
                }

                if (collision.OverlapSphere(transf.ValueRO.Position, findTarget.ValueRO.range, ref distanceHits, filter))
                {
                    foreach (DistanceHit hit in distanceHits)
                    {
                        Faction unit = SystemAPI.GetComponent<Faction>(hit.Entity);
                        if (findTarget.ValueRO.targetFaction == unit.faction)
                        {
                            if (closestTarget == Entity.Null)
                            {
                                closestTarget = hit.Entity;
                                closestDistance = hit.Distance;
                            }
                            else if(hit.Distance + closestDistanceOffset < closestDistance)
                            {
                                closestDistance = hit.Distance;
                                closestTarget = hit.Entity;
                            }
                        }
                    }
                }
                if(closestTarget != Entity.Null)
                    target.ValueRW.target = closestTarget;
            }
        }
    }
}