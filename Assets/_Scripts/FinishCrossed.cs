using CarControllerwithShooting;
using UnityEngine;

namespace Voidwalker
{
    public class FinishCrossed : MonoBehaviour
    {
        private const string LATEST_LEVEL = "LatestLevel";
        private const string CURRENT_LEVEL = "Level";

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Car")) return;

            CarController car = other.GetComponentInChildren<CarController>();

            if (car != null)
            {
                HandleCarFinished(car);
                CheckAndUpdateLevel();
            }
        }

        private void HandleCarFinished(CarController car)
        {
            car.SetCarStop(true);
            car.GetComponent<EngineAudio>().SetStopEngineSound(true);
        }

        private static void CheckAndUpdateLevel()
        {
            int currentLevel = PlayerPrefs.GetInt(CURRENT_LEVEL);
            int latestLevel = PlayerPrefs.GetInt(LATEST_LEVEL);

            if (currentLevel == latestLevel)
            {
                latestLevel += 1;
                PlayerPrefs.SetInt(LATEST_LEVEL, latestLevel);
            }
        }
    }
}