using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct EntitiesReferences: IComponentData
    {
        public Entity bulletEntity;
        public Entity zombiePrefab;
        public Entity shootLightPrefab;
        public Entity scoutPrefab;
        public Entity soldierPrefab;

        public Entity buildingTowerPrefab;
        public Entity buildingBarracksPrefab;
        public Entity buildingHarvesterGold;
        public Entity buildingHarvesterIron;
        public Entity buildingHarvesterOil;

        public Entity buildingTowerPrefabVisual;
        public Entity buildingBarracksPrefabVisual;
        public Entity buildingHarvesterGoldVisual;
        public Entity buildingHarvesterIronVisual;
        public Entity buildingHarvesterOilVisual;

        public Entity buildingConstructionPrefab;
    }

    class EntitiesReferencesAuthoring : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public GameObject zombiePrefab;
        public GameObject shootLightPrefab;
        public GameObject scoutPrefab;
        public GameObject soldierPrefab;

        public GameObject buildingTowerPrefab;
        public GameObject buildingBarracksPrefab;
        public GameObject buildingHarvesterGold;
        public GameObject buildingHarvesterIron;
        public GameObject buildingHarvesterOil;

        public GameObject buildingTowerPrefabVisual;
        public GameObject buildingBarracksPrefabVisual;
        public GameObject buildingHarvesterGoldVisual;
        public GameObject buildingHarvesterIronVisual;
        public GameObject buildingHarvesterOilVisual;

        public GameObject buildingConstructionPrefab;

        class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
        {
            public override void Bake(EntitiesReferencesAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EntitiesReferences
                {
                    bulletEntity = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                    zombiePrefab = GetEntity(authoring.zombiePrefab, TransformUsageFlags.Dynamic),
                    shootLightPrefab = GetEntity(authoring.shootLightPrefab, TransformUsageFlags.Dynamic),
                    scoutPrefab = GetEntity(authoring.scoutPrefab, TransformUsageFlags.Dynamic),
                    soldierPrefab = GetEntity(authoring.soldierPrefab, TransformUsageFlags.Dynamic),
                    buildingTowerPrefab = GetEntity(authoring.buildingTowerPrefab, TransformUsageFlags.Dynamic),
                    buildingBarracksPrefab = GetEntity(authoring.buildingBarracksPrefab, TransformUsageFlags.Dynamic),
                    buildingHarvesterGold = GetEntity(authoring.buildingHarvesterGold, TransformUsageFlags.Dynamic),
                    buildingHarvesterIron = GetEntity(authoring.buildingHarvesterIron, TransformUsageFlags.Dynamic),
                    buildingHarvesterOil = GetEntity(authoring.buildingHarvesterOil, TransformUsageFlags.Dynamic),
                    buildingTowerPrefabVisual = GetEntity(authoring.buildingTowerPrefabVisual, TransformUsageFlags.Dynamic),
                    buildingBarracksPrefabVisual = GetEntity(authoring.buildingBarracksPrefabVisual, TransformUsageFlags.Dynamic),
                    buildingHarvesterGoldVisual = GetEntity(authoring.buildingHarvesterGoldVisual, TransformUsageFlags.Dynamic),
                    buildingHarvesterIronVisual = GetEntity(authoring.buildingHarvesterIronVisual, TransformUsageFlags.Dynamic),
                    buildingHarvesterOilVisual = GetEntity(authoring.buildingHarvesterOilVisual, TransformUsageFlags.Dynamic),
                    buildingConstructionPrefab = GetEntity(authoring.buildingConstructionPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}