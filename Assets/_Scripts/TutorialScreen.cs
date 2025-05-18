using CarControllerwithShooting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Voidwalker
{
    public class TutorialScreen : MonoBehaviour
    {
        [SerializeField] private int _mouseButton = 0;

        [SerializeField] private Image _controlsImage;

        [SerializeField] private GameObject _mouseBtnImage;
        [SerializeField] private GameObject _arrowImage;

        [SerializeField] private TextMeshProUGUI _clickText;
        [SerializeField] private TextMeshProUGUI _tapText;

        private bool _isMobile;

        public void ShowTutorialScreen()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0;

            _arrowImage.SetActive(true);
            GameCanvas.Instance.joystick.gameObject.SetActive(false);
        }

        public void HideTutorialScreen()
        {
            _controlsImage.gameObject.SetActive(true);
            gameObject.SetActive(false);
            Time.timeScale = 1;

            if (_isMobile)
            {
                GameCanvas.Instance.joystick.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            _isMobile = Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform;

            if (_isMobile)
            {
                _mouseBtnImage.SetActive(false);
                _clickText.gameObject.SetActive(false);
                _tapText.gameObject.SetActive(true);
            }
            else
            {
                _mouseBtnImage.SetActive(true);
                _clickText.gameObject.SetActive(true);
                _tapText.gameObject.SetActive(false);
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
