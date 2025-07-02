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
    }
}