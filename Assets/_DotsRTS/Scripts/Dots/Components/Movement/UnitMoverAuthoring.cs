using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsRTS
{
    public struct UnitMover : IComponentData
    {
        public float movementSpeed;
        public float rotationSpeed;
        public float3 targetPosition;
        public bool isMoving;
    }

    class UnitMoverAuthoring : MonoBehaviour
    {
        public float movementSpeed;
        public float rotationSpeed;

        class UnitMoverAuthoringBaker : Baker<UnitMoverAuthoring>
        {
            public override void Bake(UnitMoverAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new UnitMover
                {
                    movementSpeed = authoring.movementSpeed,
                    rotationSpeed = authoring.rotationSpeed,
                    targetPosition = authoring.transform.position
                });
            }
        }
    }
}