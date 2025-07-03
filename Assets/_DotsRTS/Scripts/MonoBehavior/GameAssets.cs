using UnityEngine;

namespace DotsRTS
{
    public class GameAssets : MonoBehaviour
    {
        public const int UNITS_LAYER = 6;
        public const int BUILDINGS_LAYER = 7;
        public const int PATHFINDING_WALLS = 8;
        public const int PATHFINDING_HEAVY = 9;
        public const int FOG_OF_WAR = 11;

        public UnitTypeListSO unitTypeList;
        public BuildingTypeListSO buildingTypeList;

        #region Singleton
        public static GameAssets Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type GameAssets in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;
        }
        #endregion


    }
}