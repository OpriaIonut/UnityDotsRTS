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
        private NativeList<Entity> barrackQueueChangedList;
        private NativeList<Entity> onHealthDeadEntityList;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            spawnedJobs = new NativeArray<JobHandle>(3, Allocator.Persistent);
            barrackQueueChangedList = new NativeList<Entity>(Allocator.Persistent);
            onHealthDeadEntityList = new NativeList<Entity>(Allocator.Persistent);
        }

        public void OnUpdate(ref SystemState state)
        {
            if(SystemAPI.HasSingleton<BuildingHQ>())
            {
                Health health = SystemAPI.GetComponent<Health>(SystemAPI.GetSingletonEntity<BuildingHQ>());
                if(health.onDead)
                {
                    DotsEventsManager.Instance.TriggerOnHQDead();
                }
            }

            spawnedJobs[0] = new ResetSelectedEventsJob().ScheduleParallel(state.Dependency);
            spawnedJobs[1] = new ResetShootAttackEventsJob().ScheduleParallel(state.Dependency);
            spawnedJobs[2] = new ResetMeleeAttackEventsJob().ScheduleParallel(state.Dependency);

            onHealthDeadEntityList.Clear();
            new ResetHealthEventsJob()
            {
                onHealthDeadEntityList = onHealthDeadEntityList.AsParallelWriter()
            }.ScheduleParallel(state.Dependency).Complete();
            DotsEventsManager.Instance?.TriggerOnHealthDead(onHealthDeadEntityList);

            barrackQueueChangedList.Clear();
            new ResetBuildingBarracksEventsJob()
            {
                onUnitQueueChangedEntityList = barrackQueueChangedList.AsParallelWriter()
            }.ScheduleParallel(state.Dependency).Complete();
            DotsEventsManager.Instance?.TriggerOnBarracksUnitQueueChanged(barrackQueueChangedList);

            state.Dependency = JobHandle.CombineDependencies(spawnedJobs);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            spawnedJobs.Dispose();
            barrackQueueChangedList.Dispose();
            onHealthDeadEntityList.Dispose();
        }
    }

    [BurstCompile]
    public partial struct ResetHealthEventsJob : IJobEntity
    {
        public NativeList<Entity>.ParallelWriter onHealthDeadEntityList;

        public void Execute(ref Health health, Entity entity)
        {
            if(health.onDead)
            {
                onHealthDeadEntityList.AddNoResize(entity);
            }

            health.onHealthChanged = false;
            health.onDead = false;
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

    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct ResetBuildingBarracksEventsJob : IJobEntity
    {
        public NativeList<Entity>.ParallelWriter onUnitQueueChangedEntityList;

        public void Execute(ref BuildingBarracks barrack, Entity entity)
        {
            if (barrack.onUnitQueueChanged)
            {
                onUnitQueueChangedEntityList.AddNoResize(entity);
            }
            barrack.onUnitQueueChanged = false;
        }
    }
}