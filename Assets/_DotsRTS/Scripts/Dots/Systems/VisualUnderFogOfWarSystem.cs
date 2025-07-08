using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Collections;

namespace DotsRTS
{
    partial struct VisualUnderFogOfWarSystem : ISystem
    {
        private ComponentLookup<LocalTransform> transfLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameSceneTag>();

            transfLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            transfLookup.Update(ref state);

            PhysicsWorldSingleton physics = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collision = physics.CollisionWorld;

            CollisionFilter filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.FOG_OF_WAR,
                GroupIndex = 0
            };

            var buffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var job = new VisualUnderFogOfWarJob
            {
                buffer = buffer.AsParallelWriter(),
                filter = filter,
                collision = collision,
                deltaTime = SystemAPI.Time.DeltaTime,
                transfLookup = transfLookup
            };
            job.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct VisualUnderFogOfWarJob: IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter buffer;

        [ReadOnly] public ComponentLookup<LocalTransform> transfLookup;
        [ReadOnly] public CollisionWorld collision;
        [ReadOnly] public CollisionFilter filter;
        [ReadOnly] public float deltaTime;

        public void Execute(ref VisualUnderFogOfWar fow, [ChunkIndexInQuery] int chunkIndexInQuery, Entity entity)
        {
            fow.timer -= deltaTime;
            if (fow.timer > 0)
                return;
            fow.timer = fow.timerMax;

            LocalTransform parentTransf = transfLookup[fow.parentEntity];
            if (!collision.SphereCast(parentTransf.Position, fow.sphereCastSize, new float3(0, 1, 0), 100, filter))
            {
                if (fow.isVisible)
                {
                    fow.isVisible = false;
                    buffer.AddComponent<DisableRendering>(chunkIndexInQuery, entity);
                }
            }
            else
            {
                if (!fow.isVisible)
                {
                    fow.isVisible = true;
                    buffer.RemoveComponent<DisableRendering>(chunkIndexInQuery, entity);
                }
            }
        }
    }
}