using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct HordeSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var references = SystemAPI.GetSingleton<EntitiesReferences>();
            var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach(var (transf, horde) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Horde>>())
            {
                horde.ValueRW.startTimer -= SystemAPI.Time.DeltaTime;
                if (horde.ValueRO.startTimer > 0)
                    continue;

                if (horde.ValueRO.zombiesToSpawn <= 0)
                    continue;

                horde.ValueRW.spawnTimer -= SystemAPI.Time.DeltaTime;
                if (horde.ValueRO.spawnTimer > 0)
                    continue;
                horde.ValueRW.spawnTimer = horde.ValueRO.spawnTimerMax;

                var rand = horde.ValueRO.rand;
                float3 spawnPos = transf.ValueRO.Position;
                spawnPos.x += rand.NextFloat(-horde.ValueRO.spawnAreaWidth, horde.ValueRO.spawnAreaWidth);
                spawnPos.z += rand.NextFloat(-horde.ValueRO.spawnAreaHeight, horde.ValueRO.spawnAreaHeight);
                horde.ValueRW.rand = rand;

                Entity zombie = buffer.Instantiate(references.zombiePrefab);
                buffer.SetComponent(zombie, LocalTransform.FromPosition(spawnPos));
                buffer.AddComponent<EnemyAttackHQ>(zombie);
                buffer.RemoveComponent<RandomWalking>(zombie);

                horde.ValueRW.zombiesToSpawn--;
            }
        }
    }
}