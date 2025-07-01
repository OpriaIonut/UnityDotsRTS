using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct EntitiesReferences: IComponentData
    {
        public Entity bulletEntity;
        public Entity zombiePrefab;
        public Entity shootLightPrefab;
    }

    class EntitiesReferencesAuthoring : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public GameObject zombiePrefab;
        public GameObject shootLightPrefab;

        class EntitiesReferencesAuthoringBaker : Baker<EntitiesReferencesAuthoring>
        {
            public override void Bake(EntitiesReferencesAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EntitiesReferences
                {
                    bulletEntity = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                    zombiePrefab = GetEntity(authoring.zombiePrefab, TransformUsageFlags.Dynamic),
                    shootLightPrefab = GetEntity(authoring.shootLightPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}