using System.Collections.Generic;
using UnityEngine;

namespace DotsRTS
{
    [CreateAssetMenu(fileName = "UnitTypeListSO", menuName = "Scriptable Objects/UnitTypeListSO")]
    public class UnitTypeListSO : ScriptableObject
    {
        public List<UnitTypeSO> units;

        public UnitTypeSO GetUnitDataSO(UnitType type)
        {
            foreach (var item in units)
            {
                if (item.unitType == type)
                    return item;
            }
            return null;
        }
    }
}