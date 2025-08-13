using UnityEngine;

namespace CarControllerwithShooting
{
    public class CarSystemManager : MonoBehaviour
    {
        public ControllerType controllerType;
        public bool ShowRadar = true;

        public GameObject mainCamera;
        public GameObject car;
        public static CarSystemManager Instance;

        public bool isWeaponsActive = true;

        public void Awake()
        {
            Instance = this;

            if (PokiUnitySDK.Instance == null)
            {
                PokiUnitySDK.Instance.init();
            }
        }

        private void Start()
        {
            bool isMobile = Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform;
            if (SystemInfo.deviceModel.Contains("iPad") || SystemInfo.deviceModel.Contains("iPhone"))
            {
                isMobile = true;
            }
            if (isMobile)
            {
                //Screen.autorotateToLandscapeLeft = true;
                //Screen.orientation = ScreenOrientation.LandscapeLeft;

                controllerType = ControllerType.Mobile;
            }
            if (controllerType == ControllerType.KeyboardMouse)
            {
                GameCanvas.Instance.Configure_For_PCConsole();
                //Cursor.visible = false;
                //Cursor.lockState = CursorLockMode.Locked;
            }
            else if (controllerType == ControllerType.Mobile)
            {
                GameCanvas.Instance.Configure_For_Mobile();
            }
            if (!isWeaponsActive)
            {
                GunController.Instance.DeactivateWeapons();
            }
        }

        public Transform GetCamera()
        {
            return mainCamera.transform;
        }
    }

    public enum ControllerType
    {
        KeyboardMouse,
        Mobile
    }
}