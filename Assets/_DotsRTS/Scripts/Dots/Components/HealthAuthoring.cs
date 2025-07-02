using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Health : IComponentData
    {
        public int health;
        public int healthMax;
        public bool onHealthChanged;
        public bool onDead;
    }

    class HealthAuthoring : MonoBehaviour
    {
        public int health;

        class HealthAuthoringBaker : Baker<HealthAuthoring>
        {
            public override void Bake(HealthAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Health
                {
                    health = authoring.health,
                    healthMax = authoring.health,
                    onHealthChanged = true
                });
            }
        }
    }
}