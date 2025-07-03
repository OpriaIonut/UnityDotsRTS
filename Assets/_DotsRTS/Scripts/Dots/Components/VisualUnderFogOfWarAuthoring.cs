using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct VisualUnderFogOfWar: IComponentData
    {
        public bool isVisible;
        public Entity parentEntity;
        public float sphereCastSize;
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
                    isVisible = true,
                    parentEntity = GetEntity(authoring.parent, TransformUsageFlags.Dynamic),
                    sphereCastSize = authoring.sphereCastSize
                });
            }
        }
    }
}