using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DotsRTS
{
    public class RagdollManager : MonoBehaviour
    {
        [SerializeField] private UnitTypeListSO unitList;

        private void Start()
        {
            DotsEventsManager.Instance.OnHealthDead += OnHealthDead;
        }

        private void OnHealthDead(object sender, System.EventArgs e)
        {
            Entity entity = (Entity)sender;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (entityManager.HasComponent<UnitTypeHolder>(entity))
            {
                var transf = entityManager.GetComponentData<LocalTransform>(entity);
                var unitHolder = entityManager.GetComponentData<UnitTypeHolder>(entity);
                var unitSO = unitList.GetUnitDataSO(unitHolder.unitType);

                Transform ragdoll = Instantiate(unitSO.ragdoll, transf.Position, Quaternion.identity);
                Destroy(ragdoll.gameObject, 10f);
            }
        }
    }
}