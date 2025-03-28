using UnityEngine;

namespace Voidwalker
{
    public class TutorialPickup : MonoBehaviour
    {
        [SerializeField] private TutorialScreen _tutorialScreen;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Car"))
            {
                _tutorialScreen.ShowTutorialScreen();
            }
        }
    }
}
