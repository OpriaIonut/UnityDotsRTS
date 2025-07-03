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
            AddResourceAmount(ResourceType.Gold, 50);
            AddResourceAmount(ResourceType.Iron, 50);
            AddResourceAmount(ResourceType.Oil, 50);
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

        public bool CanSpendResourceAmount(ResourceAmount amount)
        {
            return resources[amount.resourceType] >= amount.amount;
        }
        public bool CanSpendResourceAmount(ResourceAmount[] amount)
        {
            foreach(var item in amount)
            {
                if (resources[item.resourceType] < item.amount)
                    return false;
            }
            return true;
        }

        public void SpendResourceAmount(ResourceAmount res)
        {
            resources[res.resourceType] -= res.amount;
            OnResourceChanged?.Invoke();
        }
        public void SpendResourceAmount(ResourceAmount[] res)
        {
            foreach (var item in res)
            {
                resources[item.resourceType] -= item.amount;
            }
            OnResourceChanged?.Invoke();
        }
    }
}