using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public enum UnitType
    {
        None,
        Soldier,
        Scout,
        Zombie
    }

    [CreateAssetMenu(fileName = "UnitTypeSO", menuName = "Scriptable Objects/UnitTypeSO")]
    public class UnitTypeSO : ScriptableObject
    {
        public UnitType unitType;
        public float spawnDuration;
        public Sprite sprite;

        public Entity GetPrefabEntity(EntitiesReferences entities)
        {
            switch (unitType)
            {
                default:
                case UnitType.None:
                case UnitType.Soldier: return entities.soldierPrefab;
                case UnitType.Scout: return entities.scoutPrefab;
                case UnitType.Zombie: return entities.zombiePrefab;
            }
        }
    }
}