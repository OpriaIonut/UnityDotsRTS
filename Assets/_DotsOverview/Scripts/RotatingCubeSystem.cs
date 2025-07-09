using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DotsOverview
{
    public partial struct RotatingCubeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RotateSpeed>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            return;

            foreach(var (transf, speed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotateSpeed>>())
            {
                transf.ValueRW = transf.ValueRO.RotateY(speed.ValueRO.value * SystemAPI.Time.DeltaTime);
            }
            RotatingCubeJob rotatingCubeJob = new RotatingCubeJob
            {
                deltaTime = SystemAPI.Time.DeltaTime
            };
            rotatingCubeJob.Schedule();
        }
    }

    [BurstCompile]
    [WithAll(typeof(RotatingCube))]
    public partial struct RotatingCubeJob : IJobEntity
    {
        public float deltaTime;

        [BurstCompile]
        public void Execute(ref LocalTransform transf, in RotateSpeed speed)
        {
            transf = transf.RotateY(speed.value * deltaTime);
        }
    }
}