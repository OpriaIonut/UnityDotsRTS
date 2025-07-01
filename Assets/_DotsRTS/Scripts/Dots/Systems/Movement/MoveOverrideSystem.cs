using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct MoveOverrideSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (transf, moveOverride, mover, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveOverride>, RefRW<UnitMover>>().WithEntityAccess())
            {
                if(math.distancesq(transf.ValueRO.Position, moveOverride.ValueRO.targetPos) > UnitMoverSystem.REACH_DIST_SQ)
                {
                    mover.ValueRW.targetPosition = moveOverride.ValueRO.targetPos;
                }
                else
                {
                    SystemAPI.SetComponentEnabled<MoveOverride>(entity, false);
                }
            }
        }
    }
}