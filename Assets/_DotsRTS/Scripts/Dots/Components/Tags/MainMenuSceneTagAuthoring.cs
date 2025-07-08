using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct MainMenuSceneTag : IComponentData
    {

    }

    class MainMenuSceneTagAuthoring : MonoBehaviour
    {
        class MainMenuSceneTagAuthoringBaker : Baker<MainMenuSceneTagAuthoring>
        {
            public override void Bake(MainMenuSceneTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MainMenuSceneTag());
            }
        }
    }
}