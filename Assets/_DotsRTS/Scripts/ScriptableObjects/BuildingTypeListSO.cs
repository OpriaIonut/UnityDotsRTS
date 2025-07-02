using System.Collections.Generic;
using UnityEngine;

namespace DotsRTS
{
    [CreateAssetMenu(fileName = "BuildingTypeListSO", menuName = "Scriptable Objects/BuildingTypeListSO")]
    public class BuildingTypeListSO : ScriptableObject
    {
        public List<BuildingTypeSO> buildings;

        public BuildingTypeSO GetBuildingDataSO(BuildingType type)
        {
            foreach(var item in buildings)
            {
                if (item.buildingType == type)
                    return item;
            }
            return null;
        }
    }
}