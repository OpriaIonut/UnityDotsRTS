using Unity.Entities;
using UnityEngine;

namespace DotsOverview
{
    public struct RotatingCube : IComponentData
    {

    }


    public class RotateCubeAuthoring : MonoBehaviour
    {
        public class Baker : Baker<RotateCubeAuthoring>
        {
            public override void Bake(RotateCubeAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RotatingCube());
            }
        }
    }
}