using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct EnemyAttackHQ: IComponentData
    {

    }

    class EnemyAttackHQAuthoring : MonoBehaviour
    {
        class EnemyAttackHQAuthoringBaker : Baker<EnemyAttackHQAuthoring>
        {
            public override void Bake(EnemyAttackHQAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyAttackHQ());
            }
        }
    }
}