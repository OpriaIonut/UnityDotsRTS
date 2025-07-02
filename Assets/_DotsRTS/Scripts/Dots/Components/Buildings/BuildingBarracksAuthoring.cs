using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRTS
{
    public struct BuildingBarracks : IComponentData
    {
        public float progress;
        public float progressMax;
        public UnitType activeUnitType;
        public float3 rallyPositionOffset;
    }

    [InternalBufferCapacity(10)]
    public struct SpawnUnitTypeBuffer: IBufferElementData
    {
        public UnitType unitType;
    }

    class BuildingBarracksAuthoring : MonoBehaviour
    {
        public float progressMax;

        class BuildingBarracksAuthoringBaker : Baker<BuildingBarracksAuthoring>
        {
            public override void Bake(BuildingBarracksAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingBarracks
                {
                    progressMax = authoring.progressMax,
                    rallyPositionOffset = new float3(10, 0, 0)
                });

                var buffer = AddBuffer<SpawnUnitTypeBuffer>(entity);
                buffer.Add(new SpawnUnitTypeBuffer { unitType = UnitType.Soldier });
                buffer.Add(new SpawnUnitTypeBuffer { unitType = UnitType.Soldier });
                buffer.Add(new SpawnUnitTypeBuffer { unitType = UnitType.Scout });
            }
        }
    }
}