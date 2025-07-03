using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public struct TargetPositionPathQueue: IComponentData, IEnableableComponent
    {
        public float3 targetPos;
    }

    class TargetPositionPathQueueAuthoring : MonoBehaviour
    {
        class TargetPositionPathQueueAuthoringBaker : Baker<TargetPositionPathQueueAuthoring>
        {
            public override void Bake(TargetPositionPathQueueAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TargetPositionPathQueue());
                SetComponentEnabled<TargetPositionPathQueue>(entity, false);
            }
        }
    }
}
