using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct UnitAnimations: IComponentData
    {
        public AnimationType idleAnim;
        public AnimationType walkAnim;
        public AnimationType aimAnim;
        public AnimationType shootAnim;
        public AnimationType meleeAnim;
    }

    class UnitAnimationsAuthoring : MonoBehaviour
    {
        public AnimationType idleAnim;
        public AnimationType walkAnim;
        public AnimationType aimAnim;
        public AnimationType shootAnim;
        public AnimationType meleeAnim;

        class UnitAnimationsAuthoringBaker : Baker<UnitAnimationsAuthoring>
        {
            public override void Bake(UnitAnimationsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new UnitAnimations
                {
                    idleAnim = authoring.idleAnim,
                    walkAnim = authoring.walkAnim,
                    aimAnim = authoring.aimAnim,
                    shootAnim = authoring.shootAnim,
                    meleeAnim = authoring.meleeAnim,
                });
            }
        }
    }
}