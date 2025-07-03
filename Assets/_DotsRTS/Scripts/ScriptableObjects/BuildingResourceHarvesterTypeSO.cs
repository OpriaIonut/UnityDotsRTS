using UnityEngine;

namespace DotsRTS
{
    [CreateAssetMenu(fileName = "BuildingResourceHarvesterTypeSO", menuName = "Scriptable Objects/BuildingResourceHarvesterTypeSO")]
    public class BuildingResourceHarvesterTypeSO : BuildingTypeSO
    {
        public ResourceType resourceType;
        public float harvestDistance;
    }
}