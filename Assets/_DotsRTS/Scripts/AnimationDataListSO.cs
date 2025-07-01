using System.Collections.Generic;
using UnityEngine;

namespace DotsRTS
{
    [CreateAssetMenu(fileName = "AnimationDataListSO", menuName = "Scriptable Objects/AnimationDataListSO")]
    public class AnimationDataListSO : ScriptableObject
    {
        public List<AnimationDataSO> animations;

        public AnimationDataSO GetAnimationData(AnimationType type)
        {
            foreach(var item in animations)
            {
                if (item.animType == type)
                    return item;
            }
            return null;
        }
    }
}