using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public struct ShootVictim: IComponentData
    {
        public float3 hitLocalPos;
    }

    class ShootVictimAuthoring : MonoBehaviour
    {
        public Transform hitTransform;

        class ShootVictimAuthoringBaker : Baker<ShootVictimAuthoring>
        {
            public override void Bake(ShootVictimAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ShootVictim
                {
                    hitLocalPos = authoring.hitTransform.localPosition,
                });
            }
        }
    }
}