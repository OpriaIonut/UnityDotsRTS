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
        [SerializeField] private UnityEngine.Material ghostRedMaterial;

        public UnityAction OnActiveBuildingTypeChanged;

        private Transform ghostTransform;
        private UnityEngine.Material activeGhostMaterial;

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
            {
                TooltipScreenSpaceUI.HideTooltip_Static();
                return;
            }

            if(Input.GetMouseButtonDown(1))
            {
                BuildingTypeListSO buildingList = GameAssets.Instance.buildingTypeList;
                SetActiveBuildingTypeSO(buildingList.GetBuildingDataSO(BuildingType.None));
                TooltipScreenSpaceUI.HideTooltip_Static();
                return;
            }

            TooltipScreenSpaceUI.ShowTooltip_Static(buildingType.name + "\n" + ResourceAmount.GetString(buildingType.buildCosts), .5f);

            if (!ResourceManager.Instance.CanSpendResourceAmount(buildingType.buildCosts))
            {
                // Cannot afford this building
                SetGhostMaterial(ghostRedMaterial);
                TooltipScreenSpaceUI.ShowTooltip_Static(buildingType.name + "\n" + ResourceAmount.GetString(buildingType.buildCosts) + "\n" +
                    "<color=#ff0000>Cannot afford resource cost!</color>", .05f);
                return;
            }
            else
            {
                SetGhostMaterial(ghostMaterial);
            }

            Vector3 mouseWorldPos = MouseWorldPosition.Instance.GetPosition();

            if (!CanPlaceBuilding(mouseWorldPos, out string errorMessage))
            {
                // Cannot place building here
                SetGhostMaterial(ghostRedMaterial);
                TooltipScreenSpaceUI.ShowTooltip_Static(buildingType.name + "\n" + ResourceAmount.GetString(buildingType.buildCosts) + "\n" +
                    "<color=#ff0000>" + errorMessage + "</color>", .05f);
                return;
            }
            else
            {
                SetGhostMaterial(ghostMaterial);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (ResourceManager.Instance.CanSpendResourceAmount(buildingType.buildCosts))
                {
                    if (CanPlaceBuilding(mouseWorldPos, out string errorMsg))
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

        private bool CanPlaceBuilding(Vector3 pos, out string errorMessage)
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
                errorMessage = "Build area must be clear!";
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
                        {
                            errorMessage = "Same Building Type too close!";
                            return false;
                        }
                    }
                    if (entityManager.HasComponent<BuildingConstruction>(hit.Entity))
                    {
                        var holder = entityManager.GetComponentData<BuildingConstruction>(hit.Entity);
                        if (holder.buildingType == buildingType.buildingType)
                        {
                            errorMessage = "Same Building Type too close!";
                            return false;
                        }
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
                {
                    errorMessage = "No valid Resource Nodes nearby!";
                    return false;
                }
            }

            errorMessage = null;
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
                SetGhostMaterial(ghostMaterial);
            }

            OnActiveBuildingTypeChanged?.Invoke();
        }

        private void SetGhostMaterial(UnityEngine.Material ghostMaterial)
        {
            if (activeGhostMaterial == ghostMaterial)
            {
                // Already set this material
                return;
            }

            activeGhostMaterial = ghostMaterial;

            foreach (MeshRenderer meshRenderer in ghostTransform.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = ghostMaterial;
            }
        }
    }
}