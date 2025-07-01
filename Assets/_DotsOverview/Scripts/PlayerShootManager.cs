using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DotsOverview
{
    public class PlayerShootManager : MonoBehaviour
    {
        [SerializeField] private GameObject shootPopupPrefab;

        private void Start()
        {
            var shooting = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerShootingSystem>();
            shooting.OnShoot += OnPlayerShoot;
        }

        private void OnPlayerShoot(Entity entity)
        {
            LocalTransform transf = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalTransform>(entity);
            Instantiate(shootPopupPrefab, transf.Position, Quaternion.identity);
        }
    }
}