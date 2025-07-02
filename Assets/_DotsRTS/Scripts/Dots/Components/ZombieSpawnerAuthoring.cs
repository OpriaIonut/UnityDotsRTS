using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct ZombieSpawner: IComponentData
    {
        public float timer;
        public float timerMax;
        public float walkDistMin;
        public float walkDistMax;

        public int maxZombies;
        public float zombieSpawnCheckDist;
    }

    class ZombieSpawnerAuthoring : MonoBehaviour
    {
        public float timerMax;
        public float walkDistMin;
        public float walkDistMax;

        public int maxZombies;
        public float zombieSpawnCheckDist;

        class ZombieSpawnerAuthoringBaker : Baker<ZombieSpawnerAuthoring>
        {
            public override void Bake(ZombieSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ZombieSpawner
                {
                    timerMax = authoring.timerMax,
                    walkDistMin = authoring.walkDistMin,
                    walkDistMax = authoring.walkDistMax,
                    maxZombies = authoring.maxZombies,
                    zombieSpawnCheckDist = authoring.zombieSpawnCheckDist,
                });
            }
        }
    }
}