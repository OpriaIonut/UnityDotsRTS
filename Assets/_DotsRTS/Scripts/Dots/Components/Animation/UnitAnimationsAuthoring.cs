using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct UnitAnimations: IComponentData
    {
        public AnimationType idleAnim;
        public AnimationType walkAnim;
    }

    class UnitAnimationsAuthoring : MonoBehaviour
    {
        public AnimationType idleAnim;
        public AnimationType walkAnim;

        class UnitAnimationsAuthoringBaker : Baker<UnitAnimationsAuthoring>
        {
            public override void Bake(UnitAnimationsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new UnitAnimations
                {
                    idleAnim = authoring.idleAnim,
                    walkAnim = authoring.walkAnim,
                });
            }
        }
    }
}