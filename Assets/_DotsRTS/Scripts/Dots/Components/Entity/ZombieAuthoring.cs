using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Zombie : IComponentData
    {

    }

    class ZombieAuthoring : MonoBehaviour
    {
        class ZombieAuthoringBaker : Baker<ZombieAuthoring>
        {
            public override void Bake(ZombieAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Zombie());
            }
        }
    }
}