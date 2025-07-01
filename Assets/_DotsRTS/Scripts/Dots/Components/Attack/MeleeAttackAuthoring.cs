using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct MeleeAttack: IComponentData
    {
        public float timer;
        public float timerMax;
        public int damage;
        public float colliderSize;
    }

    class MeleeAttackAuthoring : MonoBehaviour
    {
        public float timerMax;
        public int damage;
        public float colliderSize;

        class MeleeAttackAuthoringBaker : Baker<MeleeAttackAuthoring>
        {
            public override void Bake(MeleeAttackAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MeleeAttack
                {
                    timerMax = authoring.timerMax,
                    damage = authoring.damage,
                    colliderSize = authoring.colliderSize,
                });
            }
        }
    }
}