using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Voidwalker;

namespace CarControllerwithShooting
{
    public class GameCanvas : MonoBehaviour
    {
        public Button button_Missile;
        public Button button_HandBrake;
        public Button button_Machinegun;
        public GameObject joystick;
        public Button button_CameraChange;
        public int isFiringUpdate = 0;
        public Slider Slider_CurrentFuel;
        public Text Text_CurrentFuel;
        public Text pauseText;

        public Text Text_Ammo_Machinegun;
        public Text Text_Ammo_Missile;

        public static GameCanvas Instance;
        public GameObject GameUI;
        public GameObject RadarUI;
        public GameObject GasolineUI;
        public Text text_health;
        public Text text_speed;
        public bool isPaused = false;
        public float gameOverScreenDelay = 5f;
        public float winScreenDelay = 1f;

        public GameObject Panel_Pause;
        public GameObject Panel_GameOver;
        public GameObject Panel_Win;
        public GameObject Button_Pause;
        public GameObject Panel_Settings;

        private bool anyKeyPressed;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (CarSystemManager.Instance.ShowRadar)
            {
                RadarUI.SetActive(true);
            }
            else
            {
                RadarUI.SetActive(false);
            }
            if(CarSystemManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                Button_Pause.SetActive(false);
            }
            else
            {
                Button_Pause.SetActive(true);
            }
            Update_Text_Health();
        }

        public void Update_Text_Health()
        {
            text_health.text = "HEALTH: " + CarController.Instance.Health.ToString();
        }

        public void Update_Text_Speed()
        {
            text_speed.text = Convert.ToInt32(CarController.Instance.speed).ToString();
        }

        public void Configure_For_Mobile()
        {
            joystick.gameObject.SetActive(true);
            pauseText.gameObject.SetActive(false);
            GameCanvas.Instance.button_HandBrake.gameObject.SetActive(true);
        }

        public void Configure_For_PCConsole()
        {
            joystick.gameObject.SetActive(false);
            pauseText.gameObject.SetActive(true);
            button_CameraChange.GetComponentInChildren<Text>().text = "Camera (C)";
        }

        public void Click_ButtonPause()
        {
            if (isPaused)
            {
                Click_Continue();
            }
            else
            {
                Click_Pause();
            }
        }

        public void Show_GameOver()
        {
            StartCoroutine(ShowGameOverPanel());
        }

        IEnumerator ShowGameOverPanel()
        {
            yield return new WaitForSeconds(gameOverScreenDelay);
            Time.timeScale = 0;
            Panel_GameOver.SetActive(true);
            if(CarSystemManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }

            PokiUnitySDK.Instance.gameplayStop();
        }

        public void Show_WinScreen()
        {
            StartCoroutine(ShowWinPanel());
        }

        IEnumerator ShowWinPanel()
        {
            yield return new WaitForSeconds(winScreenDelay);
            Time.timeScale = 0;
            Panel_Win.SetActive(true);
            isPaused = true;
            if (CarSystemManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }

            PokiUnitySDK.Instance.gameplayStop();
        }

        public void Click_Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Click_Continue()
        {
            AdManager.Instance.ShowCommercialBreak();
        }

        public void UnpauseGame()
        {
            if (CarSystemManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            Time.timeScale = 1;
            isPaused = false;
            Panel_Pause.SetActive(false);
            Panel_Settings.SetActive(false);
            AudioListener.pause = false;

            PokiUnitySDK.Instance.gameplayStart();
        }

        public void Click_Settings()
        {
            Panel_Settings.SetActive(true);
            Panel_Pause.SetActive(false);
        }

        public void Click_BacktoMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void Click_Close_Settings()
        {
            Panel_Settings.SetActive(false);
            Panel_Pause.SetActive(true);
        }

        public void Click_Pause()
        {
            if (Panel_GameOver.activeInHierarchy) return;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Time.timeScale = 0;
            isPaused = true;
            Panel_Pause.SetActive(true);
            AudioListener.pause = true;

            PokiUnitySDK.Instance.gameplayStop();
        }

        public void Click_Button_CameraSwitch()
        {
            if (button_CameraChange.IsInteractable())
            {
                if (CarSystemManager.Instance.cameraFPS != null && CarSystemManager.Instance.cameraFPS.activeSelf)
                {
                    CarSystemManager.Instance.cameraFPS.SetActive(false);
                    CarSystemManager.Instance.cameraTPS.SetActive(true);
                }
                else if (CarSystemManager.Instance.cameraTPS != null)
                {
                    CarSystemManager.Instance.cameraFPS.SetActive(true);
                    CarSystemManager.Instance.cameraTPS.SetActive(false);
                }
            }
        }

        private void Update()
        {
            if (!anyKeyPressed && (Input.anyKeyDown || Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
            {
                PokiUnitySDK.Instance.gameplayStart();
                StartCoroutine(AdManager.Instance.ShowDelayedCommercialBreak(0.5f));
                anyKeyPressed = true;
            }

            if (Input.GetKeyUp(KeyCode.C) && CarSystemManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                Click_Button_CameraSwitch();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && CarSystemManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                if(Panel_Settings.activeSelf)
                {
                    Click_Close_Settings();
                    return;
                }
                if (isPaused)
                {
                    Click_Continue();
                }
                else
                {
                    Click_Pause();
                }
            }
        }

        public void Click_Button_HandBrake_Down()
        {
            CarController.Instance.handBrake = 1;
        }

        public void Click_Button_HandBrake_Up()
        {
            CarController.Instance.handBrake = 0;
        }

        public void Hide_GameUI()
        {
            GameUI.SetActive(false);
        }

        public void Click_Button_MachineGun_Down()
        {
            isFiringUpdate = 1;
        }

        public void Click_Button_Guns_Up()
        {
            isFiringUpdate = 0;
        }

        public void Click_Button_Missle_Down()
        {
            isFiringUpdate = -1;
        }

        private void OnDisable()
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
        }
    }
}