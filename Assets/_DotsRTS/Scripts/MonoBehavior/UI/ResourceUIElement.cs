using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DotsRTS
{
    public class ResourceUIElement : MonoBehaviour
    {
        [SerializeField] private Image img;
        [SerializeField] private TextMeshProUGUI text;

        public void Setup(ResourceTypeSO data)
        {
            img.sprite = data.sprite;
            text.text = "0";
        }

        public void SetAmount(int amount)
        {
            text.text = amount.ToString();
        }
    }
}