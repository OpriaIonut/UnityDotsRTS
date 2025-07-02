using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DotsRTS
{
    partial struct BuildingBarracksSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntitiesReferences>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntitiesReferences entities = SystemAPI.GetSingleton<EntitiesReferences>();

            foreach(var (barrack, spawnBuffer, buildingEnqueue, enabledEnqueue) in SystemAPI.Query<RefRW<BuildingBarracks>, DynamicBuffer<SpawnUnitTypeBuffer>, RefRO<BuildingBarracksUnitEnqueue>, EnabledRefRW<BuildingBarracksUnitEnqueue>>())
            {
                spawnBuffer.Add(new SpawnUnitTypeBuffer
                {
                    unitType = buildingEnqueue.ValueRO.unitType
                });
                enabledEnqueue.ValueRW = false;

                barrack.ValueRW.onUnitQueueChanged = true;
            }

            foreach(var (transf, barrack, spawnBuffer) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<BuildingBarracks>, DynamicBuffer<SpawnUnitTypeBuffer>>())
            {
                if (spawnBuffer.IsEmpty)
                    continue;
                if (barrack.ValueRO.activeUnitType != spawnBuffer[0].unitType)
                {
                    barrack.ValueRW.activeUnitType = spawnBuffer[0].unitType;

                    UnitTypeSO activeUnitData = GameAssets.Instance.unitTypeList.GetUnitDataSO(barrack.ValueRW.activeUnitType);
                    barrack.ValueRW.progressMax = activeUnitData.spawnDuration;
                }

                barrack.ValueRW.progress += SystemAPI.Time.DeltaTime;
                if (barrack.ValueRO.progress < barrack.ValueRO.progressMax)
                    continue;
                barrack.ValueRW.progress = 0f;

                UnitType unitToSpawn = spawnBuffer[0].unitType;
                UnitTypeSO unitData = GameAssets.Instance.unitTypeList.GetUnitDataSO(unitToSpawn);
                spawnBuffer.RemoveAt(0);
                barrack.ValueRW.onUnitQueueChanged = true;

                Entity clone = state.EntityManager.Instantiate(unitData.GetPrefabEntity(entities));
                SystemAPI.SetComponent(clone, LocalTransform.FromPosition(transf.ValueRO.Position));

                SystemAPI.SetComponent(clone, new MoveOverride
                {
                    targetPos = transf.ValueRO.Position + barrack.ValueRO.rallyPositionOffset
                });
                SystemAPI.SetComponentEnabled<MoveOverride>(clone, true);
            }
        }
    }
}