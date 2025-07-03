using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct BuildingHarvester: IComponentData
    {
        public float harvestTimer;
        public float harvestTimerMax;
        public ResourceType resourceType;
    }

    class BuildingHarvesterAuthoring : MonoBehaviour
    {
        public float harvestTimerMax;
        public ResourceType resourceType;

        class BuildingHarvesterAuthoringBaker : Baker<BuildingHarvesterAuthoring>
        {
            public override void Bake(BuildingHarvesterAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingHarvester
                {
                    harvestTimerMax = authoring.harvestTimerMax,
                    resourceType = authoring.resourceType,
                });
            }
        }
    }
}