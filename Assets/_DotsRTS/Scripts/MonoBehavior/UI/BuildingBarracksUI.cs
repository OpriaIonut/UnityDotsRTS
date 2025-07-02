using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace DotsRTS
{
    public class BuildingBarracksUI : MonoBehaviour
    {
        [SerializeField] private Button soldierBtn;
        [SerializeField] private Button scoutBtn;
        [SerializeField] private Image progressBar;
        [SerializeField] private RectTransform queueContainer;
        [SerializeField] private RectTransform queueItemTemplate;

        private EntityManager entityManager;
        private Entity buildingBarracks;

        private void Awake()
        {
            soldierBtn.onClick.AddListener(() => SpawnUnit(UnitType.Soldier));
            scoutBtn.onClick.AddListener(() => SpawnUnit(UnitType.Scout));
            queueItemTemplate.gameObject.SetActive(false);
        }

        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            UnitSelectionManager.Instance.OnSelectedEntitiesChanged += OnSelectedEntitiesChanged;
            DotsEventsManager.Instance.OnBarracksQueueChanged += OnBarracksQueueChanged;
            Hide();
        }

        private void Update()
        {
            UpdateProgressBar();
        }

        private void OnSelectedEntitiesChanged()
        {
            var query = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, BuildingBarracks>().Build(entityManager);

            var entities = query.ToEntityArray(Allocator.Temp);
            if(entities.Length > 0)
            {
                buildingBarracks = entities[0];
                Show();
                UpdateProgressBar();
                UpdateUnitQueueVisual();
            }
            else
            {
                buildingBarracks = Entity.Null;
                Hide();
            }
        }

        private void OnBarracksQueueChanged(object sender, System.EventArgs e)
        {
            Entity entity = (Entity)sender;
            if(entity == buildingBarracks)
                UpdateUnitQueueVisual();
        }

        private void UpdateUnitQueueVisual()
        {
            foreach(Transform child in queueContainer)
            {
                if (child == queueItemTemplate)
                    continue;
                Destroy(child.gameObject);
            }

            var spawnBuffer = entityManager.GetBuffer<SpawnUnitTypeBuffer>(buildingBarracks, true);
            foreach(var item in spawnBuffer)
            {
                RectTransform transf = Instantiate(queueItemTemplate, queueContainer);
                transf.gameObject.SetActive(true);

                Image img = transf.GetComponent<Image>();
                UnitTypeSO unitData = GameAssets.Instance.unitTypeList.GetUnitDataSO(item.unitType);
                img.sprite = unitData.sprite;
            }
        }

        private void UpdateProgressBar()
        {
            if(buildingBarracks == Entity.Null)
            {
                progressBar.fillAmount = 0f;
                return;
            }

            var barrack = entityManager.GetComponentData<BuildingBarracks>(buildingBarracks);
            if(barrack.activeUnitType == UnitType.None)
            {
                progressBar.fillAmount = 0f;
            }
            else
            {
                progressBar.fillAmount = barrack.progress / barrack.progressMax;
            }
        }

        private void SpawnUnit(UnitType unitType)
        {
            entityManager.SetComponentData(buildingBarracks, new BuildingBarracksUnitEnqueue
            {
                unitType = unitType
            });
            entityManager.SetComponentEnabled<BuildingBarracksUnitEnqueue>(buildingBarracks, true);
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}