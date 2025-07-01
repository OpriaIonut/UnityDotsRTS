using Unity.Entities;
using UnityEngine;

namespace DotsOverview
{
    public class SpawnCubesConfigAuthoring : MonoBehaviour
    {
        public GameObject cube;
        public int count;

        public class Baker : Baker<SpawnCubesConfigAuthoring>
        {
            public override void Bake(SpawnCubesConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnCubesConfig
                {
                    cube = GetEntity(authoring.cube, TransformUsageFlags.Dynamic),
                    count = authoring.count
                });
            }
        }
    }

    public struct SpawnCubesConfig: IComponentData
    {
        public Entity cube;
        public int count;
    }
}