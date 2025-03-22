using CarControllerwithShooting;
using UnityEngine;

namespace Voidwalker
{
    public class FinishCrossed : MonoBehaviour
    {
        [SerializeField] private LevelManager _levelManager;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Car")) return;

            CarController car = other.GetComponentInChildren<CarController>();

            if (car != null)
            {
                car.SetCarStop(true);
                car.GetComponent<EngineAudio>().SetStopEngineSound(true);

                _levelManager.UnlockNextLevel();
            }
        }
    }
}