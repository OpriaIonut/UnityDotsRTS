
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DotsRTS
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    partial struct ResetTargetSystem : ISystem
    {
        private ComponentLookup<LocalTransform> transfLookup;
        private EntityStorageInfoLookup infoLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            transfLookup = state.GetComponentLookup<LocalTransform>(true);
            infoLookup = state.GetEntityStorageInfoLookup();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            transfLookup.Update(ref state);
            infoLookup.Update(ref state);

            ResetTargetJob resetTarget = new ResetTargetJob
            {
                transfLookup = transfLookup,
                infoLookup = infoLookup,
            };
            resetTarget.ScheduleParallel();

            ResetTargetOverrideJob resetTargetOverride = new ResetTargetOverrideJob
            {
                transfLookup = transfLookup,
                infoLookup = infoLookup,
            };
            resetTargetOverride.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ResetTargetJob: IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> transfLookup;
        [ReadOnly] public EntityStorageInfoLookup infoLookup;

        public void Execute(ref Target target)
        {
            if (target.target != Entity.Null)
            {
                if (!infoLookup.Exists(target.target) || !transfLookup.HasComponent(target.target))
                {
                    target.target = Entity.Null;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct ResetTargetOverrideJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> transfLookup;
        [ReadOnly] public EntityStorageInfoLookup infoLookup;

        public void Execute(ref TargetOverride target)
        {
            if (target.target != Entity.Null)
            {
                if (!infoLookup.Exists(target.target) || !transfLookup.HasComponent(target.target))
                {
                    target.target = Entity.Null;
                }
            }
        }
    }
}