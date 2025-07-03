using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct EnemyAttackHQSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BuildingHQ>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity entity = SystemAPI.GetSingletonEntity<BuildingHQ>();
            float3 position = SystemAPI.GetComponent<LocalTransform>(entity).Position;

            foreach(var (attack, pathQueue, enabledPathQueue, target) in SystemAPI.Query<RefRO<EnemyAttackHQ>, RefRW<TargetPositionPathQueue>, EnabledRefRW<TargetPositionPathQueue>, RefRO<Target>>().WithDisabled<MoveOverride>().WithPresent<TargetPositionPathQueue>())
            {
                if (target.ValueRO.target != Entity.Null)
                    continue;
                pathQueue.ValueRW.targetPos = position;
                enabledPathQueue.ValueRW = true;
            }
        }
    }
}