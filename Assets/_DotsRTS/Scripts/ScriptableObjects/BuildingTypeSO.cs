using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public enum BuildingType
    {
        None,
        ZombieSpawner,
        Tower,
        Barracks
    }

    [CreateAssetMenu(fileName = "BuildingTypeSO", menuName = "Scriptable Objects/BuildingTypeSO")]
    public class BuildingTypeSO : ScriptableObject
    {
        public BuildingType buildingType;
        public GameObject prefab;
        public float buildingDistanceMin;
        public bool showInPlacementUI;
        public Sprite sprite;
        public Transform visualPrefab;


        public Entity GetPrefabEntity(EntitiesReferences entities)
        {
            switch (buildingType)
            {
                default:
                case BuildingType.None:
                case BuildingType.Tower: return entities.buildingTowerPrefab;
                case BuildingType.Barracks: return entities.buildingBarracksPrefab;
            }
        }
    }
}