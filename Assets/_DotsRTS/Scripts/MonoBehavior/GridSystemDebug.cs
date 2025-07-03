using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public class GridSystemDebug : MonoBehaviour
    {
        [SerializeField] private Transform debugPrefab;
        [SerializeField] private Sprite circleSprite;
        [SerializeField] private Sprite arrowSprite;

        private bool isInitialized = false;
        private GridDebugCell[,] debugGrid;

        #region Singleton
        public static GridSystemDebug Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type GridSystemDebug in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;
        }
        #endregion

        public void InitializeGrid(GridSystem.GridSystemData gridSystemData)
        {
            if (isInitialized)
                return;
            isInitialized = true;

            debugGrid = new GridDebugCell[gridSystemData.width, gridSystemData.height];
            for (int x = 0; x < gridSystemData.width; ++x)
            {
                for (int y = 0; y < gridSystemData.height; ++y)
                {
                    Transform clone = Instantiate(debugPrefab);
                    var cell = clone.GetComponent<GridDebugCell>();
                    cell.Setup(x, y, gridSystemData.gridNodeSize);
                    debugGrid[x, y] = cell;
                }
            }
        }

        public void UpdateGrid(GridSystem.GridSystemData gridSystemData)
        {
            for (int x = 0; x < gridSystemData.width; ++x)
            {
                for (int y = 0; y < gridSystemData.height; ++y)
                {
                    GridDebugCell cell = debugGrid[x, y];
                    EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    int index = GridSystem.CalculateIndex(x, y, gridSystemData.width);

                    int gridIndex = gridSystemData.nextGridIndex - 1;
                    if (gridIndex < 0)
                        gridIndex = GridSystem.FLOW_FIELD_MAP_COUNT - 1;

                    Entity node = gridSystemData.gridMap[gridIndex].gridEntities[index];
                    var nodeData = entityManager.GetComponentData<GridSystem.GridNode>(node);
                    //cell.SetColor(nodeData.data == 0 ? Color.white : Color.blue);

                    if (nodeData.cost == 0)
                    {
                        cell.SetSprite(circleSprite);
                        cell.SetColor(Color.green);
                    }
                    else if (nodeData.cost == GridSystem.WALL_COST)
                    {
                        cell.SetSprite(circleSprite);
                        cell.SetColor(Color.black);
                    }
                    else
                    {
                        cell.SetSprite(arrowSprite);
                        cell.SetColor(Color.white);
                        cell.SetSpriteRotation(Quaternion.LookRotation(new Vector3(nodeData.vector.x, 0, nodeData.vector.y), Vector3.up));
                    }
                }
            }
        }
    }
}