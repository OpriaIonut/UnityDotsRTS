using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public class ZombieTopBarUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI zombieAmountText;
        [SerializeField] private TextMeshProUGUI zombieBuildingAmountText;

        private float updateCooldown = 0.5f;
        private float lastUpdateTime;

        private void Start()
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }

        private void Update()
        {
            if (Time.time - lastUpdateTime < updateCooldown)
                return;
            lastUpdateTime = Time.time;

            UpdateUI();
        }

        private void UpdateUI()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = new EntityQueryBuilder(Allocator.Temp).WithAll<Unit, Zombie>().Build(entityManager);

            zombieAmountText.text = query.CalculateEntityCount().ToString();
            query.Dispose();

            query = new EntityQueryBuilder(Allocator.Temp).WithAll<ZombieSpawner>().Build(entityManager);
            zombieBuildingAmountText.text = query.CalculateEntityCount().ToString();
            query.Dispose();
        }
    }
}