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
                });
            }
        }
    }
}