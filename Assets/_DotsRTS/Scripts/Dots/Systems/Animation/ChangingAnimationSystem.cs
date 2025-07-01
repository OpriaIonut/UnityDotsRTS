using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace DotsRTS
{
    [UpdateBefore(typeof(ActiveAnimationSystem))]
    partial struct ChangingAnimationSystem : ISystem
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

            new ChangeAnimationJob
            {
                animHolder = animHolder,
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct ChangeAnimationJob: IJobEntity
        {
            public AnimationDataHolder animHolder;

            public void Execute(ref ActiveAnimation anim, ref MaterialMeshInfo mesh)
            {
                if (anim.activeAnim != anim.nextAnim)
                {
                    anim.frame = 0;
                    anim.frameTimer = 0f;
                    anim.activeAnim = anim.nextAnim;

                    ref AnimationData animData = ref animHolder.animations.Value[(int)anim.activeAnim];
                    mesh.Mesh = animData.meshes[0];
                }
            }
        }
    }
}