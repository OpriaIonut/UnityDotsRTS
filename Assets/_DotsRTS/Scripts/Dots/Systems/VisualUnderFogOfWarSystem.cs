using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct VisualUnderFogOfWarSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            CollisionFilter filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.FOG_OF_WAR,
                GroupIndex = 0
            };

            var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (fow, entity) in SystemAPI.Query<RefRW<VisualUnderFogOfWar>>().WithEntityAccess())
            {
                LocalTransform parentTransf = SystemAPI.GetComponent<LocalTransform>(fow.ValueRO.parentEntity);
                if(!collision.SphereCast(parentTransf.Position, fow.ValueRO.sphereCastSize, new float3(0, 1, 0), 100, filter))
                {
                    if(fow.ValueRO.isVisible)
                    {
                        fow.ValueRW.isVisible = false;
                        buffer.AddComponent<DisableRendering>(entity);
                    }
                }
                else
                {
                    if(!fow.ValueRO.isVisible)
                    {
                        fow.ValueRW.isVisible = true;
                        buffer.RemoveComponent<DisableRendering>(entity);
                    }
                }
            }
        }
    }
}