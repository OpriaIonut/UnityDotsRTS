using UnityEngine;
using UnityEngine.UI;

namespace DotsRTS
{
    public class BuildingPlacementBtn : MonoBehaviour
    {
        [SerializeField] private Image img;
        [SerializeField] private Image selectedImg;

        private BuildingTypeSO btnData;

        public void Setup(BuildingTypeSO data)
        {
            btnData = data;
            img.sprite = data.sprite;

            GetComponent<Button>().onClick.AddListener(() =>
            {
                BuildingPlacementManager.Instance.SetActiveBuildingTypeSO(btnData);
            }); 
        }

        public void SetSelected(bool value)
        {
            selectedImg.enabled = value;
        }
    }
}