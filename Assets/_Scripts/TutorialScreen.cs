using UnityEngine;

namespace Voidwalker
{
    public class TutorialScreen : MonoBehaviour
    {
        [SerializeField] private int _mouseButton = 0;

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
