using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsRTS
{
    public struct ActiveAnimation: IComponentData
    {
        public int frame;
        public float frameTimer;
        public AnimationType activeAnim;
        public AnimationType nextAnim;
    }

    class ActiveAnimationAuthoring : MonoBehaviour
    {
        public AnimationType nextAnim;

        class ActiveAnimationAuthoringBaker : Baker<ActiveAnimationAuthoring>
        {
            public override void Bake(ActiveAnimationAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ActiveAnimation
                {
                    nextAnim = authoring.nextAnim
                });
            }
        }
    }
}