using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct BulletMoverSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (transf, bullet, target, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Bullet>, RefRO<Target>>().WithEntityAccess())
            {
                if (target.ValueRO.target == Entity.Null)
                {
                    buffer.DestroyEntity(entity);
                    continue;
                }

                LocalTransform targetTransf = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.target);
                ShootVictim shootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.target);
                float3 targetPos = targetTransf.TransformPoint(shootVictim.hitLocalPos);

                float3 moveDir = targetPos - transf.ValueRO.Position;
                moveDir = math.normalize(moveDir);

                float distBeforeSq = math.distancesq(transf.ValueRO.Position, targetPos);
                transf.ValueRW.Position += moveDir * bullet.ValueRO.speed * SystemAPI.Time.DeltaTime;

                float destroyDistSq = 0.2f;
                float distAfterSq = math.distancesq(transf.ValueRO.Position, targetPos);
                if (distAfterSq > distBeforeSq)
                {
                    transf.ValueRW.Position = targetPos;
                }

                if (math.distancesq(transf.ValueRO.Position, targetPos) < destroyDistSq)
                {
                    var health = SystemAPI.GetComponentRW<Health>(target.ValueRO.target);
                    health.ValueRW.health -= bullet.ValueRO.damage;
                    health.ValueRW.onHealthChanged = true;

                    buffer.DestroyEntity(entity);
                }
            }
        }
    }
}