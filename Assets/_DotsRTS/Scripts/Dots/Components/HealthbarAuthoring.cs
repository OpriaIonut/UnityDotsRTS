using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Healthbar: IComponentData
    {
        public Entity barVisualEntity;
        public Entity healthEntity;
    }

    class HealthbarAuthoring : MonoBehaviour
    {
        public GameObject barVisualEntity;
        public GameObject healthEntity;

        class HealthbarAuthoringBaker : Baker<HealthbarAuthoring>
        {
            public override void Bake(HealthbarAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Healthbar
                {
                    barVisualEntity = GetEntity(authoring.barVisualEntity, TransformUsageFlags.NonUniformScale),
                    healthEntity = GetEntity(authoring.healthEntity, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}