using UnityEngine;

namespace Voidwalker
{
    public class CarFlipper : MonoBehaviour
    {
        public float carFlipDelay { get; private set; } = 1f;

        public void FlipCar()
        {
            gameObject.transform.rotation = Quaternion.identity;
        }
    }
}
