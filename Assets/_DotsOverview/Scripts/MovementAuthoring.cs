using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsOverview
{
    public struct Movement: IComponentData
    {
        public float3 moveDir;
    }

    public class MovementAuthoring : MonoBehaviour
    {
        public class Baker : Baker<MovementAuthoring>
        {
            public override void Bake(MovementAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Movement
                {
                    moveDir = new float3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))
                });
            }
        }
    }
}