using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct LoseTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (transf, target, loseTarget, targetOverride) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<Target>, RefRO<LoseTarget>, RefRO<TargetOverride>>())
            {
                if (target.ValueRO.target == Entity.Null)
                    continue;

                if (targetOverride.ValueRO.target != Entity.Null)
                    continue;

                LocalTransform targetTransf = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.target);
                float targetDist = math.distance(transf.ValueRO.Position, targetTransf.Position);
                if (targetDist > loseTarget.ValueRO.lostDist)
                    target.ValueRW.target = Entity.Null;
            }
        }
    }
}