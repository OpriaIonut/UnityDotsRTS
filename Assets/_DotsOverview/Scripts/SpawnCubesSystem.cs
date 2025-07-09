using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsOverview
{
    public partial class SpawnCubesSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<SpawnCubesConfig>();
        }

        protected override void OnUpdate()
        {
            Enabled = false;

            SpawnCubesConfig cubesConfig = SystemAPI.GetSingleton<SpawnCubesConfig>();
            for(int index = 0; index < cubesConfig.count; ++index)
            {
                Entity entity = EntityManager.Instantiate(cubesConfig.cube);
                SystemAPI.SetComponent(entity, new LocalTransform
                {
                    Position = new float3(UnityEngine.Random.Range(-10f, 10f), 0.6f, UnityEngine.Random.Range(-10f, 10f)),
                    Rotation = quaternion.identity,
                    Scale = 1.0f
                });
            }
        }
    }
}