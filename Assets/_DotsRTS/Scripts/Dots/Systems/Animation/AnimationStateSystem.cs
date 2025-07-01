using Unity.Burst;
using Unity.Entities;

namespace DotsRTS
{
    partial struct AnimationStateSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (animMesh, mover, unitAnims) in SystemAPI.Query<RefRW<AnimatedMesh>, RefRO<UnitMover>, RefRO<UnitAnimations>>())
            {
                var activeAnim = SystemAPI.GetComponentRW<ActiveAnimation>(animMesh.ValueRO.meshEntity);

                if(mover.ValueRO.isMoving)
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.ValueRO.walkAnim;
                }
                else
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.ValueRO.idleAnim;
                }
            }

            foreach (var (animMesh, shootAttack, unitAnims) in SystemAPI.Query<RefRW<AnimatedMesh>, RefRO<ShootAttack>, RefRO<UnitAnimations>>())
            {
                var activeAnim = SystemAPI.GetComponentRW<ActiveAnimation>(animMesh.ValueRO.meshEntity);

                if (shootAttack.ValueRO.onShoot)
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.ValueRO.walkAnim;
                }
                else
                {
                    activeAnim.ValueRW.nextAnim = unitAnims.ValueRO.idleAnim;
                }
            }
        }
    }
}