using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Bullet: IComponentData
    {
        public float speed;
        public int damage;
    }

    class BulletAuthoring : MonoBehaviour
    {
        public float speed;
        public int damage;

        class BulletAuthoringBaker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Bullet
                {
                    speed = authoring.speed,
                    damage = authoring.damage,
                });
            }
        }
    }
}