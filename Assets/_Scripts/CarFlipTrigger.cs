using System.Collections;
using UnityEngine;

namespace Voidwalker
{
    public class CarFlipTrigger : MonoBehaviour
    {
        [SerializeField] private const string GROUND_TAG = "Ground";
        [SerializeField] private CarFlipper _carFlipper;

        private Coroutine _flipCoroutine;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GROUND_TAG))
            {
                _flipCoroutine = StartCoroutine(WaitAndFlip());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(GROUND_TAG))
            {
                if (_flipCoroutine != null)
                {
                    StopCoroutine(_flipCoroutine);
                    _flipCoroutine = null;
                }
            }
        }

        private IEnumerator WaitAndFlip()
        {
            float waitTime = _carFlipper.carFlipDelay;

            yield return new WaitForSeconds(waitTime);
            _carFlipper.FlipCar();
        }
    }
}
