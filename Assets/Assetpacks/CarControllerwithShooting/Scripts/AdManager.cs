using CarControllerwithShooting;
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
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }


        public void ShowCommercialBreak()
        {
            bool isGamePaused = Time.timeScale == 1 ? true : false;

            if (!isGamePaused || GameCanvas.Instance != null)
            {
                GameCanvas.Instance.Click_Pause();
            }

            PokiUnitySDK.Instance.commercialBreakCallBack = OnCommercialBreakEnd;
            PokiUnitySDK.Instance.commercialBreak();
        }

        private void OnCommercialBreakEnd()
        {
            if (GameCanvas.Instance != null)
            {
                GameCanvas.Instance.UnpauseGame();
            }

            PokiUnitySDK.Instance.gameplayStart();
        }
    }
}
