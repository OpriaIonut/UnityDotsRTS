using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct BuildingTypeSOHolder : IComponentData
    {
        public BuildingType buildingType;
    }

    class BuildingTypeSOHolderAuthoring : MonoBehaviour
    {
        public BuildingType buildingType;

        class BuildingTypeSOHolderAuthoringBaker : Baker<BuildingTypeSOHolderAuthoring>
        {
            public override void Bake(BuildingTypeSOHolderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingTypeSOHolder
                {
                    buildingType = authoring.buildingType
                });
            }
        }
    }
}