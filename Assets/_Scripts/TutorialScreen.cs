using UnityEngine;

namespace Voidwalker
{
    public class TutorialScreen : MonoBehaviour
    {
        [SerializeField] private int _mouseButton = 0;

        [SerializeField] private GameObject _mouseBtnImage;

        public void ShowTutorialScreen()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0;
        }

        public void HideTutorialScreen()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        private void Start()
        {
            bool isMobile = Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform;

            if (isMobile)
            {
                _mouseBtnImage.SetActive(false);
            }
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            if (Input.GetMouseButtonDown(_mouseButton))
            {
                HideTutorialScreen();
            }
        }
    }
}
