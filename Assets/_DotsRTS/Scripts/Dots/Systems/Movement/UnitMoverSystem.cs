using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct UnitMoverSystem : ISystem
    {
        public const float REACH_DIST_SQ = 2f;

        public void OnUpdate(ref SystemState state)
        {
            UnitMoverJob job = new UnitMoverJob
            {
                deltaTime = SystemAPI.Time.DeltaTime
            };
            job.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct UnitMoverJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(ref LocalTransform transf, ref PhysicsVelocity physics, ref UnitMover movement)
        {
            float3 targetPosition = movement.targetPosition;
            float3 moveDir = targetPosition - transf.Position;
            if (math.lengthsq(moveDir) <= UnitMoverSystem.REACH_DIST_SQ)
            {
                physics.Linear = float3.zero;
                physics.Angular = float3.zero;
                movement.isMoving = false;
                return;
            }
            moveDir = math.normalize(moveDir);

            quaternion targetRot = quaternion.LookRotation(moveDir, math.up());
            transf.Rotation = math.slerp(transf.Rotation, targetRot, movement.rotationSpeed * deltaTime);

            physics.Linear = moveDir * movement.movementSpeed;
            physics.Angular = float3.zero;
            movement.isMoving = true;
        }
    }
}