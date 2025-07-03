using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public struct FlowFieldPathRequest: IComponentData, IEnableableComponent
    {
        public float3 targetPos;
    }

    class FlowFieldPathRequestAuthoring : MonoBehaviour
    {
        class FlowFieldPathRequestAuthoringBaker : Baker<FlowFieldPathRequestAuthoring>
        {
            public override void Bake(FlowFieldPathRequestAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FlowFieldPathRequest());
                SetComponentEnabled<FlowFieldPathRequest>(entity, false);
            }
        }
    }
}