using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Selected : IComponentData, IEnableableComponent
    {
        public Entity visualEntity;
        public float showScale;

        public bool onSelected;
        public bool onDeselected;
    }

    class SelectedAuthoring : MonoBehaviour
    {
        public GameObject visualGameObject;
        public float showScale;

        class SelectedAuthoringBaker : Baker<SelectedAuthoring>
        {
            public override void Bake(SelectedAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Selected
                {
                    visualEntity = GetEntity(authoring.visualGameObject, TransformUsageFlags.Dynamic),
                    showScale = authoring.showScale
                });
                SetComponentEnabled<Selected>(entity, false);
            }
        }
    }
}