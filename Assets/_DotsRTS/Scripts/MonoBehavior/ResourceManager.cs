using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DotsRTS
{
    public class ResourceManager : MonoBehaviour
    {
        [SerializeField] private ResourceTypeListSO resourceList;

        private Dictionary<ResourceType, int> resources;

        public UnityAction OnResourceChanged;

        #region Singleton
        public static ResourceManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type ResourceManager in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;

            resources = new Dictionary<ResourceType, int>();
            foreach (var res in resourceList.resources)
            {
                resources[res.resourceType] = 0;
            }
        }
        #endregion

        public void AddResourceAmount(ResourceType res, int amount)
        {
            resources[res] += amount;
            OnResourceChanged?.Invoke();
        }

        public int GetResourceAmount(ResourceType res)
        {
            return resources[res];
        }
    }
}