using Unity.Burst;
using Unity.Entities;
using UnityEngine.Experimental.GlobalIllumination;

namespace DotsRTS
{
    partial struct ShootLightDestroySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (shootLight, entity) in SystemAPI.Query<RefRW<ShootLight>>().WithEntityAccess())
            {
                shootLight.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (shootLight.ValueRO.timer <= 0f)
                    buffer.DestroyEntity(entity);
            }
        }
    }
}