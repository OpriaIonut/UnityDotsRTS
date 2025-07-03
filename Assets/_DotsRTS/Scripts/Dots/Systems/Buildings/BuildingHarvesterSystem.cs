using Unity.Burst;
using Unity.Entities;

namespace DotsRTS
{
    partial struct BuildingHarvesterSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach(var harvester in SystemAPI.Query<RefRW<BuildingHarvester>>())
            {
                harvester.ValueRW.harvestTimer -= SystemAPI.Time.DeltaTime;
                if (harvester.ValueRO.harvestTimer > 0)
                    continue;
                harvester.ValueRW.harvestTimer = harvester.ValueRO.harvestTimerMax;

                ResourceManager.Instance.AddResourceAmount(harvester.ValueRO.resourceType, 1);
            }
        }
    }
}