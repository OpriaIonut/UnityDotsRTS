using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct SelectedVisualSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>())
            {
                if (selected.ValueRO.onDeselected)
                {
                    var visualTransf = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
                    visualTransf.ValueRW.Scale = 0f;
                }
                if (selected.ValueRO.onSelected)
                {
                    var visualTransf = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
                    visualTransf.ValueRW.Scale = selected.ValueRO.showScale;
                }
            }
        }
    }
}