using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsRTS
{
    public struct ShootAttack : IComponentData
    {
        public float timer;
        public float timerMax;
        public int damage;
        public float attackDistance;

        public bool onShoot;
        public float3 firePointLocal;
    }

    class ShootAttackAuthoring : MonoBehaviour
    {
        public float timerMax;
        public int damage;
        public float attackDistance;
        public Transform firePoint;

        class ShootAttackAuthoringBaker : Baker<ShootAttackAuthoring>
        {
            public override void Bake(ShootAttackAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new ShootAttack
                {
                    timerMax = authoring.timerMax,
                    damage = authoring.damage,
                    attackDistance = authoring.attackDistance,
                    firePointLocal = authoring.firePoint.localPosition
                });
            }
        }
    }
}