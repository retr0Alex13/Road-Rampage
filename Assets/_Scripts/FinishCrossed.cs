using CarControllerwithShooting;
using UnityEngine;

namespace Voidwalker
{
    public class FinishCrossed : MonoBehaviour
    {
        private const string LATEST_LEVEL = "savedgame_latestlevel";
        private const string CURRENT_LEVEL = "savedgame_level";

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Car")) return;

            CarController car = other.GetComponentInChildren<CarController>();

            if (car != null)
            {
                HandleCarFinished(car);

                int currentLevelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL);
                int latestLevel = PlayerPrefs.GetInt(LATEST_LEVEL, 0);

                if (currentLevelIndex >= latestLevel)
                {
                    PlayerPrefs.SetInt(LATEST_LEVEL, currentLevelIndex + 1);
                    PlayerPrefs.Save();
                }
            }
        }

        private void HandleCarFinished(CarController car)
        {
            car.SetCarStop(true);
            car.GetComponent<EngineAudio>().SetStopEngineSound(true);
        }
    }
}