using UnityEngine;

namespace Voidwalker
{
    public class PerformanceController : MonoBehaviour
    {
        void Start()
        {
            Application.targetFrameRate = 60;
        }
    }
}
