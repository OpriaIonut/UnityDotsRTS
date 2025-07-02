using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Faction: IComponentData
    {
        public FactionType faction;
    }

    class FactionAuthoring : MonoBehaviour
    {
        public FactionType faction;

        class FactionAuthoringBaker : Baker<FactionAuthoring>
        {
            public override void Bake(FactionAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Faction
                {
                    faction = authoring.faction
                });
            }
        }
    }
}