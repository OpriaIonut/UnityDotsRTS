using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;

namespace DotsOverview
{
    public partial class PlayerShootingSystem : SystemBase
    {
        public UnityAction<Entity> OnShoot;

        protected override void OnCreate()
        {
            RequireForUpdate<Player>();
        }

        protected override void OnUpdate()
        {
            if(Input.GetKeyDown(KeyCode.P))
            {
                Entity player = SystemAPI.GetSingletonEntity<Player>();
                EntityManager.SetComponentEnabled<Stunned>(player, true);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Entity player = SystemAPI.GetSingletonEntity<Player>();
                EntityManager.SetComponentEnabled<Stunned>(player, false);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnCubesConfig config = SystemAPI.GetSingleton<SpawnCubesConfig>();

                EntityCommandBuffer buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
                foreach (var (transf, player, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Player>>().WithDisabled<Stunned>().WithEntityAccess())
                {
                    Entity spawnedEntity = buffer.Instantiate(config.cube);
                    buffer.SetComponent(spawnedEntity, new LocalTransform
                    {
                        Position = transf.ValueRO.Position,
                        Rotation = quaternion.identity,
                        Scale = 1.0f
                    });
                    buffer.SetComponent(spawnedEntity, new Movement
                    {
                        moveDir = transf.ValueRO.Forward()
                    });

                    OnShoot?.Invoke(entity);
                }
                buffer.Playback(EntityManager);
            }

            float horizontalInput = Input.GetAxis("Horizontal");
            if (horizontalInput != 0f)
            {
                foreach(var (transf, player) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Player>>())
                {
                    transf.ValueRW = transf.ValueRO.RotateY(horizontalInput * 10.0f * SystemAPI.Time.DeltaTime);
                }
            }
            float verticalInput = Input.GetAxis("Vertical");
            if(verticalInput != 0f)
            {
                foreach (var (transf, player) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Player>>())
                {
                    transf.ValueRW = transf.ValueRO.Translate(transf.ValueRO.Forward() * verticalInput * SystemAPI.Time.DeltaTime);
                }
            }
        }
    }
}