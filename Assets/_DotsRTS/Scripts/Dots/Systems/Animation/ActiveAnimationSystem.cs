using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace DotsRTS
{
    partial struct ActiveAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimationDataHolder>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            AnimationDataHolder animHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

            foreach(var (anim, materialMesh) in SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>())
            {
                ref AnimationData animData = ref animHolder.animations.Value[(int)anim.ValueRO.activeAnim];

                anim.ValueRW.frameTimer += SystemAPI.Time.DeltaTime;
                if(anim.ValueRW.frameTimer > animData.frameTimerMax)
                {
                    anim.ValueRW.frameTimer -= animData.frameTimerMax;
                    anim.ValueRW.frame = (anim.ValueRW.frame + 1) % animData.frameMax;

                    materialMesh.ValueRW.MeshID = animData.meshes[anim.ValueRW.frame];
                }
            }
        }
    }
}