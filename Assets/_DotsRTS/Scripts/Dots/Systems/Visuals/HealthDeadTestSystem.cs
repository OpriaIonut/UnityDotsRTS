using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

namespace DotsRTS
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct HealthDeadTestSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (health, entity) in SystemAPI.Query<RefRO<Health>>().WithEntityAccess())
            {
                if(health.ValueRO.health <= 0)
                {
                    buffer.DestroyEntity(entity);
                }
            }
        }
    }
}