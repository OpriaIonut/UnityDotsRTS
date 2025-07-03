using System.Collections.Generic;
using UnityEngine;

namespace DotsRTS
{
    [CreateAssetMenu(fileName = "ResourceTypeListSO", menuName = "Scriptable Objects/ResourceTypeListSO")]
    public class ResourceTypeListSO : ScriptableObject
    {
        public List<ResourceTypeSO> resources;
    }
}