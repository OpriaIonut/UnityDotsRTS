using System.Collections.Generic;
using UnityEngine;

namespace DotsRTS
{
    public class ResourceManagerUI : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private Transform template;
        [SerializeField] private ResourceTypeListSO resourceList;

        private Dictionary<ResourceType, ResourceUIElement> spawnedResources = new Dictionary<ResourceType, ResourceUIElement>();

        private void Awake()
        {
            template.gameObject.SetActive(false);
        }

        private void Start()
        {
            ResourceManager.Instance.OnResourceChanged += UpdateAmounts;
            Setup();
            UpdateAmounts();
        }

        public void Setup()
        {
            foreach(Transform child in container)
            {
                if (child == template)
                    continue;
                Destroy(child.gameObject);
            }

            foreach(ResourceTypeSO res in resourceList.resources)
            {
                Transform transf = Instantiate(template, container);
                transf.gameObject.SetActive(true);

                ResourceUIElement elem = transf.GetComponent<ResourceUIElement>();
                elem.Setup(res);
                spawnedResources[res.resourceType] = elem;
            }
        }

        public void UpdateAmounts()
        {
            foreach (ResourceTypeSO res in resourceList.resources)
            {
                spawnedResources[res.resourceType].SetAmount(ResourceManager.Instance.GetResourceAmount(res.resourceType));
            }
        }
    }
}