using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct BuildingConstructionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (transf, construction, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<BuildingConstruction>>().WithEntityAccess())
            {
                var visualTransf = SystemAPI.GetComponentRW<LocalTransform>(construction.ValueRO.visual);
                float lerpVal = construction.ValueRO.constructionTimer / construction.ValueRO.constructionTimerMax;
                visualTransf.ValueRW.Position = math.lerp(construction.ValueRO.startPos, construction.ValueRO.endPos, lerpVal);

                construction.ValueRW.constructionTimer += SystemAPI.Time.DeltaTime;
                if (construction.ValueRO.constructionTimer < construction.ValueRO.constructionTimerMax)
                    continue;

                Entity spawned = buffer.Instantiate(construction.ValueRO.finalPrefab);
                buffer.SetComponent(spawned, LocalTransform.FromPosition(transf.ValueRO.Position));

                buffer.DestroyEntity(construction.ValueRO.visual);
                buffer.DestroyEntity(entity);
            }
        }
    }
}