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

            new ActiveAnimationJob
            {
                animHolder = animHolder,
                deltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ActiveAnimationJob: IJobEntity
    {
        public AnimationDataHolder animHolder;
        public float deltaTime;

        public void Execute(ref ActiveAnimation anim, ref MaterialMeshInfo materialMesh)
        {
            ref AnimationData animData = ref animHolder.animations.Value[(int)anim.activeAnim];

            anim.frameTimer += deltaTime;
            if (anim.frameTimer > animData.frameTimerMax)
            {
                anim.frameTimer -= animData.frameTimerMax;
                anim.frame = (anim.frame + 1) % animData.frameMax;

                materialMesh.Mesh = animData.meshes[anim.frame];

                if (anim.frame == 0 && (anim.activeAnim == AnimationType.SoldierShoot || anim.activeAnim == AnimationType.ZombieAttack))
                {
                    anim.activeAnim = AnimationType.None;
                }
            }
        }
    }
}