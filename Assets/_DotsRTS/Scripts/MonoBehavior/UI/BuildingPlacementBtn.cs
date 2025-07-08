using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DotsRTS
{
    public class BuildingPlacementBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image img;
        [SerializeField] private Image selectedImg;

        public UnityAction OnAnyButtonSingleMouseOver;
        public UnityAction OnAnyButtonSingleMouseOut;

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

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipScreenSpaceUI.ShowTooltip_Static(btnData.name + "\n" + ResourceAmount.GetString(btnData.buildCosts), 99f);

            OnAnyButtonSingleMouseOver?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipScreenSpaceUI.HideTooltip_Static();
            OnAnyButtonSingleMouseOut?.Invoke();
        }
    }
}