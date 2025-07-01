using UnityEngine;

namespace DotsRTS
{
    public class UnitSelectionManagerUI : MonoBehaviour
    {
        [SerializeField] private RectTransform selectionArea;
        [SerializeField] private Canvas canvas;

        private void Start()
        {
            UnitSelectionManager.Instance.OnSelectionAreaStart += OnSelectionStart;
            UnitSelectionManager.Instance.OnSelectionAreaEnd += OnSelectionEnd;
        }

        private void Update()
        {
            if (selectionArea.gameObject.activeSelf)
                UpdateVisuals();
        }

        private void OnSelectionStart()
        {
            selectionArea.gameObject.SetActive(true);
            UpdateVisuals();
        }
        private void OnSelectionEnd()
        {
            selectionArea.gameObject.SetActive(false);
        }

        public void UpdateVisuals()
        {
            Rect rect = UnitSelectionManager.Instance.GetSelectionRect();
            float canvasScale = canvas.transform.localScale.x;

            selectionArea.anchoredPosition = new Vector2(rect.x, rect.y) / canvasScale;
            selectionArea.sizeDelta = new Vector2(rect.width, rect.height) / canvasScale;
        }
    }
}