using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DotsRTS
{
    public class BuildingPlacementManager : MonoBehaviour
    {
        [SerializeField] private BuildingTypeSO buildingType;
        [SerializeField] private UnityEngine.Material ghostMaterial;

        public UnityAction OnActiveBuildingTypeChanged;

        private Transform ghostTransform;

        #region Singleton
        public static BuildingPlacementManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type BuildingPlacementManager in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;
        }
        #endregion

        private void Start()
        {
            BuildingTypeListSO buildingList = GameAssets.Instance.buildingTypeList;
            SetActiveBuildingTypeSO(buildingList.GetBuildingDataSO(BuildingType.None));
        }

        private void Update()
        {
            if(ghostTransform != null)
            {
                ghostTransform.position = MouseWorldPosition.Instance.GetPosition();
            }

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (buildingType.buildingType == BuildingType.None)
                return;

            if(Input.GetMouseButtonDown(1))
            {
                BuildingTypeListSO buildingList = GameAssets.Instance.buildingTypeList;
                SetActiveBuildingTypeSO(buildingList.GetBuildingDataSO(BuildingType.None));
            }

            if(Input.GetMouseButtonDown(0))
            {
                if (ResourceManager.Instance.CanSpendResourceAmount(buildingType.buildCosts))
                {
                    Vector3 mouseWorldPos = MouseWorldPosition.Instance.GetPosition();
                    if (CanPlaceBuilding(mouseWorldPos))
                    {
                        ResourceManager.Instance.SpendResourceAmount(buildingType.buildCosts);

                        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                        var query = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
                        EntitiesReferences references = query.GetSingleton<EntitiesReferences>();

                        //Entity prefab = buildingType.GetPrefabEntity(references);
                        //Entity clone = entityManager.Instantiate(prefab);
                        //entityManager.SetComponentData(clone, LocalTransform.FromPosition(mouseWorldPos));

                        Entity visualPrefab = buildingType.GetVisualPrefabEntity(references);
                        Entity visualClone = entityManager.Instantiate(visualPrefab);
                        entityManager.SetComponentData(visualClone, LocalTransform.FromPosition(mouseWorldPos + new Vector3(0, buildingType.constructionYOffset, 0)));

                        Entity prefab = references.buildingConstructionPrefab;
                        Entity clone = entityManager.Instantiate(prefab);
                        entityManager.SetComponentData(clone, LocalTransform.FromPosition(mouseWorldPos));
                        entityManager.SetComponentData(clone, new BuildingConstruction
                        {
                            buildingType = buildingType.buildingType,
                            constructionTimer = 0f,
                            constructionTimerMax = buildingType.buildingDuration,
                            finalPrefab = buildingType.GetPrefabEntity(references),
                            visual = visualClone,
                            endPos = mouseWorldPos,
                            startPos = mouseWorldPos + new Vector3(0, buildingType.constructionYOffset, 0)
                        });
                    }
                }
            }
        }

        private bool CanPlaceBuilding(Vector3 pos)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery query = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            var physics = query.GetSingleton<PhysicsWorldSingleton>();
            var collision = physics.CollisionWorld;

            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,        //Bitmasks, all bits 1
                CollidesWith = 1u << GameAssets.BUILDINGS_LAYER | 1u << GameAssets.RESOURCE_LAYER,
                GroupIndex = 0
            };

            var col = buildingType.prefab.GetComponent<UnityEngine.BoxCollider>();
            float bonusExtends = 1.1f;
            NativeList<DistanceHit> distanceList = new NativeList<DistanceHit>(Allocator.Temp);
            if(collision.OverlapBox(pos, Quaternion.identity, col.size * 0.5f * bonusExtends, ref distanceList, filter))
            {
                return false;
            }

            distanceList.Clear();
            if (collision.OverlapSphere(pos, buildingType.buildingDistanceMin, ref distanceList, filter))
            {
                foreach(var hit in distanceList)
                {
                    if(entityManager.HasComponent<BuildingTypeSOHolder>(hit.Entity))
                    {
                        var holder = entityManager.GetComponentData<BuildingTypeSOHolder>(hit.Entity);
                        if (holder.buildingType == buildingType.buildingType)
                            return false;
                    }
                    if (entityManager.HasComponent<BuildingConstruction>(hit.Entity))
                    {
                        var holder = entityManager.GetComponentData<BuildingConstruction>(hit.Entity);
                        if (holder.buildingType == buildingType.buildingType)
                            return false;
                    }
                }
            }

            if (buildingType is BuildingResourceHarvesterTypeSO harvesterSO)
            {
                bool hasNearbyResource = false;
                distanceList.Clear();
                if (collision.OverlapSphere(pos, harvesterSO.harvestDistance, ref distanceList, filter))
                {
                    foreach (var hit in distanceList)
                    {
                        if (entityManager.HasComponent<ResourceTypeSOHolder>(hit.Entity))
                        {
                            var holder = entityManager.GetComponentData<ResourceTypeSOHolder>(hit.Entity);
                            if (holder.resourceType == harvesterSO.resourceType)
                            {
                                hasNearbyResource = true;
                                break;
                            }
                        }
                    }
                }
                if (!hasNearbyResource)
                    return false;
            }

            return true;
        }

        public BuildingTypeSO GetActiveBuildingTypeSO()
        {
            return buildingType;
        }

        public void SetActiveBuildingTypeSO(BuildingTypeSO item)
        {
            buildingType = item;

            if(ghostTransform != null)
            {
                Destroy(ghostTransform.gameObject);
            }
            if (buildingType.buildingType != BuildingType.None)
            {
                ghostTransform = Instantiate(buildingType.visualPrefab);
                foreach(MeshRenderer rend in ghostTransform.GetComponentsInChildren<MeshRenderer>())
                {
                    rend.material = ghostMaterial;
                }
            }

            OnActiveBuildingTypeChanged?.Invoke();
        }
    }
}