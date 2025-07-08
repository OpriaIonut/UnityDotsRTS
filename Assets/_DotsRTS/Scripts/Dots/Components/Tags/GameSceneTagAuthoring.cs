using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct GameSceneTag : IComponentData
    {

    }

    class GameSceneTagAuthoring : MonoBehaviour
    {
        class GameSceneTagAuthoringBaker : Baker<GameSceneTagAuthoring>
        {
            public override void Bake(GameSceneTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GameSceneTag());
            }
        }
    }
}