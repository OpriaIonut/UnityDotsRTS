using Unity.Entities;
using UnityEngine;

namespace DotsOverview
{
    public struct Player : IComponentData
    {

    }

    public class PlayerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Player());
            }
        }
    }
}