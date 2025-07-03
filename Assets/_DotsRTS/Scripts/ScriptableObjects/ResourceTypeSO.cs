using UnityEngine;

namespace DotsRTS
{
    public enum ResourceType
    {
        None,
        Iron,
        Gold,
        Oil
    }

    [CreateAssetMenu(fileName = "ResourceTypeSO", menuName = "Scriptable Objects/ResourceTypeSO")]
    public class ResourceTypeSO : ScriptableObject
    {
        public ResourceType resourceType;
        public Sprite sprite;
    }
}