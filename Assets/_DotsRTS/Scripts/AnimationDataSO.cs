using UnityEngine;

namespace DotsRTS
{
    public enum AnimationType
    {
        None,
        SoldierIdle,
        SoldierWalk,
        ZombieIdle,
        ZombieWalk
    }

    [CreateAssetMenu(fileName = "AnimationDataSO", menuName = "Scriptable Objects/AnimationDataSO")]
    public class AnimationDataSO : ScriptableObject
    {
        public AnimationType animType;
        public Mesh[] meshArray;
        public float frameTimerMax;
    }
}