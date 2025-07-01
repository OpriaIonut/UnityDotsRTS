using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace DotsRTS
{
    [UpdateBefore(typeof(ActiveAnimationSystem))]
    partial struct ChangingAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            AnimationDataHolder animHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

            foreach(var (anim, mesh) in SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>())
            {
                if(anim.ValueRO.activeAnim != anim.ValueRO.nextAnim)
                {
                    anim.ValueRW.frame = 0;
                    anim.ValueRW.frameTimer = 0f;
                    anim.ValueRW.activeAnim = anim.ValueRO.nextAnim;

                    ref AnimationData animData = ref animHolder.animations.Value[(int)anim.ValueRW.activeAnim];
                    mesh.ValueRW.MeshID = animData.meshes[0];
                }
            }
        }
    }
}