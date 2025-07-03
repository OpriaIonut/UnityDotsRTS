using Unity.Entities;
using UnityEngine;

namespace DotsRTS
{
    public struct Horde: IComponentData
    {
        public float startTimer;
        public float spawnTimer;
        public float spawnTimerMax;
        public int zombiesToSpawn;
        public float spawnAreaWidth;
        public float spawnAreaHeight;
        public Unity.Mathematics.Random rand;
    }

    class HordeAuthoring : MonoBehaviour
    {
        public float startTimer;
        public float spawnTimerMax;
        public int zombiesToSpawn;
        public float spawnAreaWidth;
        public float spawnAreaHeight;

        class HordeAuthoringBaker : Baker<HordeAuthoring>
        {
            public override void Bake(HordeAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Horde
                {
                    startTimer = authoring.startTimer,
                    spawnTimerMax = authoring.spawnTimerMax,
                    zombiesToSpawn = authoring.zombiesToSpawn,
                    spawnAreaHeight = authoring.spawnAreaHeight,
                    spawnAreaWidth = authoring.spawnAreaWidth,
                    rand = new Unity.Mathematics.Random((uint)entity.Index)
                });
            }
        }
    }
}