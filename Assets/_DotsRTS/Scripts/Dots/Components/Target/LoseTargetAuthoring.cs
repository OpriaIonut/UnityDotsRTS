using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct LoseTarget : IComponentData
    {
        public float lostDist;
    }

    class LoseTargetAuthoring : MonoBehaviour
    {
        public float lostDist;

        class LoseTargetAuthoringBaker : Baker<LoseTargetAuthoring>
        {
            public override void Bake(LoseTargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new LoseTarget
                {
                    lostDist = authoring.lostDist,
                });
            }
        }
    }
}