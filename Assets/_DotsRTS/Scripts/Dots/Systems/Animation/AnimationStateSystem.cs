using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static UnityEngine.GraphicsBuffer;

namespace DotsRTS
{
    [UpdateAfter(typeof(ShootAttackSystem))]
    partial struct AnimationStateSystem : ISystem
    {
        private ComponentLookup<ActiveAnimation> activeAnimLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            activeAnimLookup = state.GetComponentLookup<ActiveAnimation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            activeAnimLookup.Update(ref state);
            new IdleMoveStateJob { activeAnimLookup = activeAnimLookup }.ScheduleParallel();

            activeAnimLookup.Update(ref state);
            new ShootAimStateJob { activeAnimLookup = activeAnimLookup }.ScheduleParallel();

            activeAnimLookup.Update(ref state);
            new MeleeStateJob { activeAnimLookup = activeAnimLookup }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct IdleMoveStateJob: IJobEntity
        {
            [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimLookup;

            public void Execute(ref AnimatedMesh animMesh, in UnitMover mover, in UnitAnimations unitAnims)
            {
                var activeAnim = activeAnimLookup.GetRefRW(animMesh.meshEntity);

                if (activeAnim.ValueRW.activeAnim == AnimationType.SoldierShoot || activeAnim.ValueRW.activeAnim == AnimationType.ZombieAttack)
                    return;

                if (mover.isMoving)
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.walkAnim;
                }
                else
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.idleAnim;
                }
            }
        }

        [BurstCompile]
        public partial struct ShootAimStateJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimLookup;

            public void Execute(ref AnimatedMesh animMesh, in UnitMover mover, in UnitAnimations unitAnims, in ShootAttack shootAttack, in Target target)
            {
                var activeAnim = activeAnimLookup.GetRefRW(animMesh.meshEntity);

                if (!mover.isMoving && target.target != Entity.Null)
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.aimAnim;
                }

                if (shootAttack.onShoot)
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.shootAnim;
                }
            }
        }

        [BurstCompile]
        public partial struct MeleeStateJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimLookup;

            public void Execute(ref AnimatedMesh animMesh, in UnitAnimations unitAnims, in MeleeAttack meleeAttack)
            {
                var activeAnim = activeAnimLookup.GetRefRW(animMesh.meshEntity);

                if (meleeAttack.onAttack)
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.meleeAnim;
                }
            }
        }
    }
}