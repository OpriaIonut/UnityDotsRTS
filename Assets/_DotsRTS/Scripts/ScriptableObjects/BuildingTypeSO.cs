using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public enum BuildingType
    {
        None,
        ZombieSpawner,
        Tower,
        Barracks,
        HQ,
        GoldHarvester,
        IronHarvester,
        OilHarvester
    }

    [CreateAssetMenu(fileName = "BuildingTypeSO", menuName = "Scriptable Objects/BuildingTypeSO")]
    public class BuildingTypeSO : ScriptableObject
    {
        public BuildingType buildingType;
        public GameObject prefab;
        public float buildingDistanceMin;
        public float buildingDuration;
        public float constructionYOffset;
        public bool showInPlacementUI;
        public Sprite sprite;
        public Transform visualPrefab;
        public ResourceAmount[] buildCosts;

        public Entity GetPrefabEntity(EntitiesReferences entities)
        {
            switch (buildingType)
            {
                default:
                case BuildingType.None:
                case BuildingType.Tower: return entities.buildingTowerPrefab;
                case BuildingType.Barracks: return entities.buildingBarracksPrefab;
                case BuildingType.GoldHarvester: return entities.buildingHarvesterGold;
                case BuildingType.IronHarvester: return entities.buildingHarvesterIron;
                case BuildingType.OilHarvester: return entities.buildingHarvesterOil;
            }
        }

        public Entity GetVisualPrefabEntity(EntitiesReferences entities)
        {
            switch (buildingType)
            {
                default:
                case BuildingType.None:
                case BuildingType.Tower: return entities.buildingTowerPrefabVisual;
                case BuildingType.Barracks: return entities.buildingBarracksPrefabVisual;
                case BuildingType.GoldHarvester: return entities.buildingHarvesterGoldVisual;
                case BuildingType.IronHarvester: return entities.buildingHarvesterIronVisual;
                case BuildingType.OilHarvester: return entities.buildingHarvesterOilVisual;
            }
        }
    }
}