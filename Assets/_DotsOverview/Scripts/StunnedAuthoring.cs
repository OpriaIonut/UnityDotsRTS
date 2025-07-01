using Unity.Entities;
using UnityEngine;

namespace DotsOverview
{
    public struct Stunned: IComponentData, IEnableableComponent
    {

    }

    public class StunnedAuthoring : MonoBehaviour
    {
        public class Baker : Baker<StunnedAuthoring>
        {
            public override void Bake(StunnedAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Stunned());
                SetComponentEnabled<Stunned>(entity, false);
            }
        }
    }
}