using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Unit: IComponentData
    {

    }

    class UnitAuthoring : MonoBehaviour
    {
        class UnitAuthoringBaker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit
                {

                });
            }
        }
    }
}