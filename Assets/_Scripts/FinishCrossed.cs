using CarControllerwithShooting;
using UnityEngine;

public class FinishCrossed : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Car")) return;

        CarController car = other.GetComponentInChildren<CarController>();

        if (car != null)
        {
            car.SetCarStop(true);
            car.GetComponent<EngineAudio>().SetStopEngineSound(true);
        }
    }
}
