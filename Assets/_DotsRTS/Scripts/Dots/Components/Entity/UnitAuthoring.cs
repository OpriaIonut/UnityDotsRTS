using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Unit: IComponentData
    {
        public Faction faction;
    }

    class UnitAuthoring : MonoBehaviour
    {
        public Faction faction;

        class UnitAuthoringBaker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit
                {
                    faction = authoring.faction,
                });
            }
        }
    }
}