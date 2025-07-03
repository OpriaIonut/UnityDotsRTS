using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct ResourceTypeSOHolder: IComponentData
    {
        public ResourceType resourceType;
    }

    class ResourceTypeSOHolderAuthoring : MonoBehaviour
    {
        public ResourceType resourceType;

        class ResourceTypeSOHolderAuthoringBaker : Baker<ResourceTypeSOHolderAuthoring>
        {
            public override void Bake(ResourceTypeSOHolderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ResourceTypeSOHolder
                {
                    resourceType = authoring.resourceType,
                });
            }
        }
    }
}