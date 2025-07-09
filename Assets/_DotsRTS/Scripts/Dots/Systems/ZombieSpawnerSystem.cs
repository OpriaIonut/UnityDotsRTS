using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
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

            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;
            NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

            foreach(var (transf, spawner) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<ZombieSpawner>>())
            {
                //Run the logic only once in a while
                spawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (spawner.ValueRO.timer > 0.0f)
                    continue;
                spawner.ValueRW.timer = spawner.ValueRO.timerMax;

                distanceHitList.Clear();
                var filter = new CollisionFilter
                {
                    BelongsTo = ~0u,        //Bitmasks, all bits 1
                    CollidesWith = 1u << GameAssets.UNITS_LAYER,
                    GroupIndex = 0
                };
                int zombieCounter = 0;
                if(collision.OverlapSphere(transf.ValueRO.Position, spawner.ValueRO.zombieSpawnCheckDist, ref distanceHitList, filter))
                {
                    foreach(var hit in distanceHitList)
                    {
                        if (!SystemAPI.Exists(hit.Entity))
                            continue;
                        if (SystemAPI.HasComponent<Unit>(hit.Entity) && SystemAPI.HasComponent<Zombie>(hit.Entity))
                        {
                            zombieCounter++;
                        }
                    }
                }
                if (zombieCounter >= spawner.ValueRO.maxZombies)
                    continue;

                Entity zombie = state.EntityManager.Instantiate(references.zombiePrefab);
                LocalTransform spawnTransf = LocalTransform.FromPosition(transf.ValueRO.Position);
                SystemAPI.SetComponent(zombie, spawnTransf);

                var randWalking = SystemAPI.GetComponentRW<RandomWalking>(zombie);
                randWalking.ValueRW.originPos = spawnTransf.Position;
                randWalking.ValueRW.targetPos = spawnTransf.Position;
                randWalking.ValueRW.distMin = spawner.ValueRO.walkDistMin;
                randWalking.ValueRW.distMax = spawner.ValueRO.walkDistMax;
            }
        }
    }
}