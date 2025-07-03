using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct MeleeAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            NativeList<RaycastHit> hitList = new NativeList<RaycastHit>(Allocator.Temp);

            foreach(var (transf, melee, target, pathQueue, enabledPathQueue) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<MeleeAttack>, RefRO<Target>, RefRW<TargetPositionPathQueue>, EnabledRefRW<TargetPositionPathQueue>>().WithDisabled<MoveOverride>().WithPresent<TargetPositionPathQueue>())
            {
                if (target.ValueRO.target == Entity.Null)
                    continue;

                LocalTransform targetTransf = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.target);
                float meleeAttackDistanceSq = 2f;
                bool isCloseEnough = math.distancesq(transf.ValueRO.Position, targetTransf.Position) < meleeAttackDistanceSq;

                bool isTouchingTarget = false;
                if (!isCloseEnough)
                {
                    float3 dirToTarget = targetTransf.Position - transf.ValueRO.Position;
                    dirToTarget = math.normalize(dirToTarget);
                    float distExtraForRaycast = 0.4f;

                    RaycastInput raycast = new RaycastInput
                    {
                        Start = transf.ValueRO.Position,
                        End = transf.ValueRO.Position + dirToTarget * (melee.ValueRO.colliderSize + distExtraForRaycast),
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                            GroupIndex = 0
                        },
                    };

                    hitList.Clear();
                    if (collision.CastRay(raycast, ref hitList))
                    {
                        foreach(RaycastHit hit in hitList)
                        {
                            if(hit.Entity == target.ValueRO.target)
                            {
                                isTouchingTarget = true;
                                break;
                            }
                        }
                    }
                }

                if (!isCloseEnough && !isTouchingTarget)
                {
                    pathQueue.ValueRW.targetPos = targetTransf.Position;
                    enabledPathQueue.ValueRW = true;
                }
                else
                {
                    pathQueue.ValueRW.targetPos = transf.ValueRO.Position;
                    enabledPathQueue.ValueRW = true;

                    melee.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                    if (melee.ValueRO.timer > 0)
                        continue;
                    melee.ValueRW.timer = melee.ValueRO.timerMax;

                    RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.target);
                    targetHealth.ValueRW.health -= melee.ValueRO.damage;
                    targetHealth.ValueRW.onHealthChanged = true;

                    melee.ValueRW.onAttack = true;
                }
            }
        }
    }
}