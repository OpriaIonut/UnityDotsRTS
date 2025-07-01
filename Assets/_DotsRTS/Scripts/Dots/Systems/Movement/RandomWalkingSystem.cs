using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct RandomWalkingSystem : ISystem
    {
        private Random rand;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            rand = new Random(1);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (randWalking, mover, transf) in SystemAPI.Query<RefRW<RandomWalking>, RefRW<UnitMover>, RefRO<LocalTransform>>())
            {
                float dist = math.distancesq(transf.ValueRO.Position, randWalking.ValueRO.targetPos);
                if(dist <= UnitMoverSystem.REACH_DIST_SQ)
                {
                    float3 randDir = new float3(rand.NextFloat(-1f, 1f), 0f, rand.NextFloat(-1f, 1f));
                    randDir = math.normalize(randDir);

                    randWalking.ValueRW.targetPos = randWalking.ValueRO.originPos + randDir * rand.NextFloat(randWalking.ValueRO.distMin, randWalking.ValueRO.distMax);
                }
                else
                {
                    mover.ValueRW.targetPosition = randWalking.ValueRO.targetPos;
                }
            }
        }
    }
}