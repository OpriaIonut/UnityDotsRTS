using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsRTS
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    partial struct ResetEventsSystem : ISystem
    {
        private NativeArray<JobHandle> spawnedJobs;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            spawnedJobs = new NativeArray<JobHandle>(4, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            spawnedJobs[0] = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
            spawnedJobs[1] = new ResetHealthEventsJob().ScheduleParallel(state.Dependency);
            spawnedJobs[2] = new ResetShootAttackEventsJob().ScheduleParallel(state.Dependency);
            spawnedJobs[3] = new ResetMeleeAttackEventsJob().ScheduleParallel(state.Dependency);

            state.Dependency = JobHandle.CombineDependencies(spawnedJobs);
        }
    }

    [BurstCompile]
    public partial struct ResetHealthEventsJob : IJobEntity
    {
        public void Execute(ref Health health)
        {
            health.onHealthChanged = false;
        }
    }

    [BurstCompile]
    public partial struct ResetShootAttackEventsJob : IJobEntity
    {
        public void Execute(ref ShootAttack shootAttack)
        {
            shootAttack.onShoot = false;
        }
    }

    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct ResetSelectedEventsJob : IJobEntity
    {
        public void Execute(ref Selected selected)
        {
            selected.onSelected = false;
            selected.onDeselected = false;
        }
    }

    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct ResetMeleeAttackEventsJob : IJobEntity
    {
        public void Execute(ref MeleeAttack attack)
        {
            attack.onAttack = false;
        }
    }
}