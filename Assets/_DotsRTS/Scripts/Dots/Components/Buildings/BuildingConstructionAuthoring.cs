using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public struct BuildingConstruction: IComponentData
    {
        public float constructionTimer;
        public float constructionTimerMax;
        public float3 startPos;
        public float3 endPos;
        public BuildingType buildingType;
        public Entity finalPrefab;
        public Entity visual;
    }

    class BuildingConstructionAuthoring : MonoBehaviour
    {
        class BuildingConstructionAuthoringBaker : Baker<BuildingConstructionAuthoring>
        {
            public override void Bake(BuildingConstructionAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingConstruction());
            }
        }
    }
}