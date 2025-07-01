using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public struct RandomWalking: IComponentData
    {
        public float3 targetPos;
        public float3 originPos;
        public float distMin;
        public float distMax;
    }

    class RandomWalkingAuthoring : MonoBehaviour
    {
        public float3 targetPos;
        public float3 originPos;
        public float distMin;
        public float distMax;

        class RandomWalkingAuthoringBaker : Baker<RandomWalkingAuthoring>
        {
            public override void Bake(RandomWalkingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RandomWalking
                {
                    targetPos = authoring.targetPos,
                    originPos = authoring.originPos,
                    distMin = authoring.distMin,
                    distMax = authoring.distMax,
                });
            }
        }
    }
}