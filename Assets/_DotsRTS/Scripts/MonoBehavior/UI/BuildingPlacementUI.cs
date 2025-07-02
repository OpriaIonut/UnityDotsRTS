using System.Collections.Generic;
using UnityEngine;

namespace DotsRTS
{
    public class BuildingPlacementUI : MonoBehaviour
    {
        [SerializeField] private RectTransform buildingContainer;
        [SerializeField] private RectTransform buildingTemplate;
        [SerializeField] private BuildingTypeListSO buildingList;

        private Dictionary<BuildingTypeSO, BuildingPlacementBtn> spawnedBtns = new Dictionary<BuildingTypeSO, BuildingPlacementBtn>();

        private void Awake()
        {
            buildingTemplate.gameObject.SetActive(false);

            foreach(var item in buildingList.buildings)
            {
                if (item.showInPlacementUI == false)
                    continue;

                RectTransform transf = Instantiate(buildingTemplate, buildingContainer);
                transf.gameObject.SetActive(true);

                BuildingPlacementBtn btn = transf.GetComponent<BuildingPlacementBtn>();
                btn.Setup(item);
                spawnedBtns.Add(item, btn);
            }
        }

        private void Start()
        {
            BuildingPlacementManager.Instance.OnActiveBuildingTypeChanged += OnActiveBuildingTypeChanged;
            UpdateSelectedVisual();
        }


        private void UpdateSelectedVisual()
        {
            foreach(var item in spawnedBtns.Keys)
            {
                spawnedBtns[item].SetSelected(false);
            }

            var activeBuilding = BuildingPlacementManager.Instance.GetActiveBuildingTypeSO();
            spawnedBtns[activeBuilding].SetSelected(true);
        }

        private void OnActiveBuildingTypeChanged()
        {
            UpdateSelectedVisual();
        }
    }
}