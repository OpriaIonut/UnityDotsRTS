using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DotsRTS
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Button mainMenuBtn;

        private void Start()
        {
            DotsEventsManager.Instance.OnHQDead += OnHQDead;
            mainMenuBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 1.0f;
                SceneManager.LoadScene(0);
            });
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