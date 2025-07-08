using TMPro;
using UnityEngine;

namespace DotsRTS
{
    public class TooltipScreenSpaceUI : MonoBehaviour
    {
        [SerializeField] private RectTransform canvas;
        [SerializeField] private RectTransform background;
        [SerializeField] private TextMeshProUGUI tooltip;

        private RectTransform transf;

        private System.Func<string> getTooltipTextFunc;
        private float? showTimer;

        #region Singleton
        public static TooltipScreenSpaceUI Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type TooltipScreenSpaceUI in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;
        }
        #endregion

        private void Start()
        {
            transf = transform.GetComponent<RectTransform>();

            HideTooltip();
        }

        private void Update()
        {
            SetText(getTooltipTextFunc());
            PositionTooltip();

            //if(showTimer != null)
            //{
            //    showTimer -= Time.deltaTime;
            //    if (showTimer <= 0)
            //        HideTooltip();
            //}
        }

        private void SetText(string text)
        {
            tooltip.SetText(text);
            tooltip.ForceMeshUpdate();

            Vector2 textSize = tooltip.GetRenderedValues(false);
            Vector2 paddingSize = Vector2.one * 24;

            background.sizeDelta = textSize + paddingSize;
        }

        private void PositionTooltip()
        {
            Vector2 anchoredPos = Input.mousePosition / canvas.localScale.x;

            if(anchoredPos.x + background.rect.width > canvas.rect.width)
                anchoredPos.x = canvas.rect.width - background.rect.width;
            if(anchoredPos.y + background.rect.height > canvas.rect.height)
                anchoredPos.y = canvas.rect.height - background.rect.height;

            transf.anchoredPosition = anchoredPos;
        }

        private void ShowTooltip(string tooltipText, float? showTimer)
        {
            ShowTooltip(() => tooltipText, showTimer);
        }

        private void ShowTooltip(System.Func<string> getTooltipTextFunc, float? showTimer)
        {
            this.getTooltipTextFunc = getTooltipTextFunc;
            this.showTimer = showTimer;
            gameObject.SetActive(true);
            SetText(getTooltipTextFunc());
            PositionTooltip();
        }

        private void HideTooltip()
        {
            gameObject.SetActive(false);
        }

        public static void ShowTooltip_Static(string tooltipText, float? showTimer)
        {
            Instance.ShowTooltip(tooltipText, showTimer);
        }

        public static void ShowTooltip_Static(System.Func<string> getTooltipTextFunc, float? showTimer)
        {
            Instance.ShowTooltip(getTooltipTextFunc, showTimer);
        }

        public static void HideTooltip_Static()
        {
            Instance.HideTooltip();
        }
    }
}