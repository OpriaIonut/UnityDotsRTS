using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace DotsRTS
{
    public struct VisualUnderFogOfWar: IComponentData
    {
        public bool isVisible;
        public Entity parentEntity;
        public float sphereCastSize;
        public float timer;
        public float timerMax;
    }

    class VisualUnderFogOfWarAuthoring : MonoBehaviour
    {
        public GameObject parent;
        public float sphereCastSize;

        class VisualUnderFogOfWarAuthoringBaker : Baker<VisualUnderFogOfWarAuthoring>
        {
            public override void Bake(VisualUnderFogOfWarAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VisualUnderFogOfWar
                {
                    isVisible = false,
                    parentEntity = GetEntity(authoring.parent, TransformUsageFlags.Dynamic),
                    sphereCastSize = authoring.sphereCastSize,
                    timer = 0f,
                    timerMax = 0.2f
                });
                AddComponent(entity, new DisableRendering());
            }
        }
    }
}