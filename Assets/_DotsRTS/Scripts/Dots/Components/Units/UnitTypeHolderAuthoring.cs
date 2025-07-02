using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct UnitTypeHolder: IComponentData
    {
        public UnitType unitType;
    }

    class UnitTypeHolderAuthoring : MonoBehaviour
    {
        public UnitType unitType;

        class UnitTypeHolderAuthoringBaker : Baker<UnitTypeHolderAuthoring>
        {
            public override void Bake(UnitTypeHolderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new UnitTypeHolder
                {
                    unitType = authoring.unitType,
                });
            }
        }
    }
}