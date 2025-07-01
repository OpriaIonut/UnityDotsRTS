using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DotsRTS
{
    partial struct HealthbarSystem : ISystem
    {
        private ComponentLookup<LocalTransform> transfLookup;
        private ComponentLookup<Health> healthLookup;
        private ComponentLookup<PostTransformMatrix> transfMatrixLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            transfLookup = state.GetComponentLookup<LocalTransform>();
            healthLookup = state.GetComponentLookup<Health>(true);
            transfMatrixLookup = state.GetComponentLookup<PostTransformMatrix>(false);
        }

        public void OnUpdate(ref SystemState state)
        {
            Vector3 cameraForward = Vector3.zero;
            if(Camera.main != null)
                cameraForward = Camera.main.transform.forward;

            transfLookup.Update(ref state);
            healthLookup.Update(ref state);
            transfMatrixLookup.Update(ref state);

            HealthbarJob healthbarJob = new HealthbarJob
            {
                cameraForward = cameraForward,
                transfLookup = transfLookup,
                healthLookup = healthLookup,
                transfMatrixLookup = transfMatrixLookup
            };
            healthbarJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct HealthbarJob: IJobEntity
    {
        public float3 cameraForward;

        [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> transfLookup;
        [ReadOnly] public ComponentLookup<Health> healthLookup;
        [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> transfMatrixLookup;

        public void Execute(in Healthbar healthbar, Entity entity)
        {
            RefRW<LocalTransform> transf = transfLookup.GetRefRW(entity);

            var parentTransf = transfLookup[healthbar.healthEntity];
            if (transf.ValueRW.Scale == 1f)
                transf.ValueRW.Rotation = parentTransf.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));

            Health health = healthLookup[healthbar.healthEntity];
            if (health.onHealthChanged == false)
                return;

            float healthNormalized = (float)health.health / health.healthMax;
            transf.ValueRW.Scale = (healthNormalized == 1f) ? 0f : 1f; //Hide healthbar if hp full

            var barTransf = transfMatrixLookup.GetRefRW(healthbar.barVisualEntity);
            barTransf.ValueRW.Value = float4x4.Scale(healthNormalized, 1, 1);
        }
    }
}