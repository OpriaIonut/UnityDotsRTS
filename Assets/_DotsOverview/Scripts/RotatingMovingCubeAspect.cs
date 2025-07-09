using Unity.Entities;
using Unity.Transforms;

namespace DotsOverview
{
    public readonly partial struct RotatingMovingCubeAspect : IAspect
    {
        public readonly RefRO<RotatingCube> cube;

        public readonly RefRW<LocalTransform> transf;
        public readonly RefRO<RotateSpeed> speed;
        public readonly RefRO<Movement> movement;

        public void MoveAndRotate(float deltaTime)
        {
            transf.ValueRW = transf.ValueRO.RotateY(speed.ValueRO.value * deltaTime);
            transf.ValueRW = transf.ValueRO.Translate(movement.ValueRO.moveDir * deltaTime);
        }
    }
}