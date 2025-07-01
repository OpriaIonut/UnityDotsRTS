using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Target : IComponentData
    {
        public Entity target;
    }

    class TargetAuthoring : MonoBehaviour
    {
        public GameObject target;

        class TargetAuthoringBaker : Baker<TargetAuthoring>
        {
            public override void Bake(TargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Target
                {
                    target = GetEntity(authoring.target, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}