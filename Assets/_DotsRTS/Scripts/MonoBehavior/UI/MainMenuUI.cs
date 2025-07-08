using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DotsRTS
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playBtn;
        [SerializeField] private Button quitBtn;

        private void Awake()
        {
            playBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(1);
            });
            quitBtn.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }
    }
}