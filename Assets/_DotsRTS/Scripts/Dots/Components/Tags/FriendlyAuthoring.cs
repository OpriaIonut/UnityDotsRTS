using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Friendly : IComponentData
    {

    }

    class FriendlyAuthoring : MonoBehaviour
    {
        class FriendlyAuthoringBaker : Baker<FriendlyAuthoring>
        {
            public override void Bake(FriendlyAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Friendly());
            }
        }
    }
}