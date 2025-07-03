using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public struct FlowFieldFollower: IComponentData, IEnableableComponent
    {
        public float3 lastMoveVect;
        public float3 targetPos;
        public int gridIndex;
    }

    class FlowFieldFollowerAuthoring : MonoBehaviour
    {
        class FlowFieldFollowerAuthoringBaker : Baker<FlowFieldFollowerAuthoring>
        {
            public override void Bake(FlowFieldFollowerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FlowFieldFollower());
                SetComponentEnabled<FlowFieldFollower>(entity, false);
            }
        }
    }
}