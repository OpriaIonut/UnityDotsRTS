using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct FindTarget : IComponentData
    {
        public float range;
        public Faction targetFaction;

        public float timer;
        public float timerMax;
    }

    class FindTargetAuthoring : MonoBehaviour
    {
        public float range;
        public float timerMax;
        public Faction targetFaction;

        class FindTargetAuthoringBaker : Baker<FindTargetAuthoring>
        {
            public override void Bake(FindTargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FindTarget
                {
                    range = authoring.range,
                    timerMax = authoring.timerMax,
                    targetFaction = authoring.targetFaction,
                });
            }
        }
    }
}