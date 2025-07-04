using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct ShootLight: IComponentData
    {
        public float timer;
    }

    class ShootLightAuthoring : MonoBehaviour
    {
        public float timer;

        class ShootLightAuthoringBaker : Baker<ShootLightAuthoring>
        {
            public override void Bake(ShootLightAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootLight
                {
                    timer = authoring.timer,
                });
            }
        }
    }
}