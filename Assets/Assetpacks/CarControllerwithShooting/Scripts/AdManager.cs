using CarControllerwithShooting;
using System.Collections;
using UnityEngine;

namespace Voidwalker
{
    public class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        public static AdManager Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }


        public void ShowCommercialBreak()
        {
            bool isGamePaused = GameCanvas.Instance.isPaused;

            if (!isGamePaused && GameCanvas.Instance != null)
            {
                GameCanvas.Instance.Click_Pause();
            }

            PokiUnitySDK.Instance.commercialBreak();

            if (GameCanvas.Instance != null)
            {
                GameCanvas.Instance.UnpauseGame();
            }
        }

        public IEnumerator ShowDelayedCommercialBreak(float delay)
        {
            yield return new WaitForSeconds(delay);

            ShowCommercialBreak();
        }
    }
}
