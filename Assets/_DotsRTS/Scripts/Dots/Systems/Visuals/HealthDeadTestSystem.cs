using Unity.Burst;
using Unity.Entities;

namespace DotsRTS
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct HealthDeadTestSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (health, entity) in SystemAPI.Query<RefRW<Health>>().WithEntityAccess())
            {
                if(health.ValueRO.health <= 0)
                {
                    health.ValueRW.onDead = true;
                    buffer.DestroyEntity(entity);

                    if(SystemAPI.HasComponent<BuildingConstruction>(entity))
                    {
                        var construction = SystemAPI.GetComponent<BuildingConstruction>(entity);
                        buffer.DestroyEntity(construction.visual);
                    }
                }
            }
        }
    }
}