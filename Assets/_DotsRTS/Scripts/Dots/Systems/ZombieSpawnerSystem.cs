using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct ZombieSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences references = SystemAPI.GetSingleton<EntitiesReferences>();

            foreach(var (transf, spawner) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
            {
                //Run the logic only once in a while
                spawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (spawner.ValueRO.timer > 0.0f)
                    continue;
                spawner.ValueRW.timer = spawner.ValueRO.timerMax;

                Entity zombie = state.EntityManager.Instantiate(references.zombiePrefab);
                LocalTransform spawnTransf = LocalTransform.FromPosition(transf.ValueRO.Position);
                SystemAPI.SetComponent(zombie, spawnTransf);

                var randWalking = SystemAPI.GetComponentRW<RandomWalking>(zombie);
                randWalking.ValueRW.originPos = spawnTransf.Position;
                randWalking.ValueRW.targetPos = spawnTransf.Position;
            }
        }
    }
}