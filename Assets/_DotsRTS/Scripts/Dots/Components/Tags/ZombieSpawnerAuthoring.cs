using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct ZombieSpawner: IComponentData
    {
        public float timer;
        public float timerMax;
    }

    class ZombieSpawnerAuthoring : MonoBehaviour
    {
        public float timerMax;

        class ZombieSpawnerAuthoringBaker : Baker<ZombieSpawnerAuthoring>
        {
            public override void Bake(ZombieSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ZombieSpawner
                {
                    timerMax = authoring.timerMax,
                });
            }
        }
    }
}