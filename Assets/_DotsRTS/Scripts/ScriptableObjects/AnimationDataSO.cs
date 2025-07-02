using UnityEngine;

namespace DotsRTS
{
    public enum AnimationType
    {
        None,
        SoldierIdle,
        SoldierWalk,
        ZombieIdle,
        ZombieWalk,
        SoldierAim,
        SoldierShoot,
        ZombieAttack,
        ScoutIdle,
        ScoutWalk,
        ScoutShoot,
        ScoutAim
    }

    [CreateAssetMenu(fileName = "AnimationDataSO", menuName = "Scriptable Objects/AnimationDataSO")]
    public class AnimationDataSO : ScriptableObject
    {
        public AnimationType animType;
        public Mesh[] meshArray;
        public float frameTimerMax;

        public static bool IsAnimationUninterruptible(AnimationType anim)
        {
            switch(anim)
            {
                default:
                    return false;
                case AnimationType.ScoutShoot:
                case AnimationType.SoldierShoot:
                case AnimationType.ZombieAttack:
                    return true;
            }
        }
    }
}