using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DotsOverview
{
    public partial struct HandleCubesSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach(var item in SystemAPI.Query<RotatingMovingCubeAspect>())
            {
                item.MoveAndRotate(SystemAPI.Time.DeltaTime);
            }
        }
    }
}