using UnityEngine;

namespace DotsRTS
{
    public class GameOverUI : MonoBehaviour
    {
        private void Start()
        {
            DotsEventsManager.Instance.OnHQDead += OnHQDead;
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnHQDead()
        {
            Show();
            Time.timeScale = 0f;
        }
    }
}