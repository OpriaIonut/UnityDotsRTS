using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct ShootAttackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences references = SystemAPI.GetSingleton<EntitiesReferences>();

            foreach(var (transf, attack, target, mover, pathQueue, enabledPathQueue, myEntity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShootAttack>, RefRO<Target>, RefRW<UnitMover>, RefRW<TargetPositionPathQueue>, EnabledRefRW<TargetPositionPathQueue>>().WithDisabled<MoveOverride>().WithPresent<TargetPositionPathQueue>().WithEntityAccess())
            {
                if (target.ValueRO.target == Entity.Null)
                    continue;

                RotateTowardsTarget(ref state, transf, mover, pathQueue, enabledPathQueue, target, attack.ValueRO.attackDistance);
            }

            foreach (var (transf, attack, target, myEntity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ShootAttack>, RefRO<Target>>().WithEntityAccess())
            {
                if (target.ValueRO.target == Entity.Null)
                    continue;

                LocalTransform targetTransf = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.target);
                if (math.distance(transf.ValueRO.Position, targetTransf.Position) > attack.ValueRO.attackDistance)
                    continue;

                if (SystemAPI.HasComponent<MoveOverride>(myEntity) && SystemAPI.IsComponentEnabled<MoveOverride>(myEntity))
                    continue;

                //Run logic only once in a while
                attack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (attack.ValueRO.timer > 0)
                    continue;
                attack.ValueRW.timer = attack.ValueRO.timerMax;

                float3 firePoint = transf.ValueRO.TransformPoint(attack.ValueRO.firePointLocal);
                attack.ValueRW.onShoot = true;
                ShootBullet(ref state, references, firePoint, attack.ValueRO.damage, target.ValueRO.target, myEntity);
            }
        }

        private bool RotateTowardsTarget(ref SystemState state, RefRW<LocalTransform> transf, RefRW<UnitMover> mover, RefRW<TargetPositionPathQueue> pathQueue, EnabledRefRW<TargetPositionPathQueue> enabledPathQueue, RefRO<Target> target, float attackDistance)
        {
            var targetTransf = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.target);
            if (math.distance(transf.ValueRO.Position, targetTransf.Position) > attackDistance)
            {
                pathQueue.ValueRW.targetPos = targetTransf.Position;
                enabledPathQueue.ValueRW = true;
                return true;
            }
            else
            {
                pathQueue.ValueRW.targetPos = transf.ValueRO.Position;
                enabledPathQueue.ValueRW = true;
            }

            float3 aimDirection = targetTransf.Position - transf.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);
            quaternion targetRot = quaternion.LookRotation(aimDirection, math.up());
            transf.ValueRW.Rotation = math.slerp(transf.ValueRO.Rotation, targetRot, SystemAPI.Time.DeltaTime * mover.ValueRO.rotationSpeed);
            return false;
        }

        private void ShootBullet(ref SystemState state, EntitiesReferences references, float3 pos, int damage, Entity target, Entity myEntity)
        {
            if (SystemAPI.HasComponent<TargetOverride>(target))
            {
                RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target);
                if (enemyTargetOverride.ValueRO.target == Entity.Null)
                {
                    enemyTargetOverride.ValueRW.target = myEntity;
                }
            }

            Entity bulletEntity = state.EntityManager.Instantiate(references.bulletEntity);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(pos));

            RefRW<Bullet> bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bullet.ValueRW.damage = damage;

            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.target = target;

            Entity shootLight = state.EntityManager.Instantiate(references.shootLightPrefab);
            SystemAPI.SetComponent(shootLight, LocalTransform.FromPosition(pos));
        }
    }
}