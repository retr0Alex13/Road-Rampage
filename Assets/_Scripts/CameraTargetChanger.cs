using CarControllerwithShooting;
using UnityEngine;

namespace Voidwalker
{
    public class CameraTargetChanger : MonoBehaviour
    {
        [SerializeField] private GameObject _cameraTarget;
        [SerializeField] private GameObject _alternativeCameraTarget;

        private FollowTargetCamera _followTargetCamera;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out CarController car))
            {
                return;
            }
            foreach (Transform child in other.transform.parent)
            {
                if (child.TryGetComponent(out FollowTargetCamera cameraTarget))
                {
                    _followTargetCamera = cameraTarget;
                }
            }

            if (_followTargetCamera.Target.gameObject == _cameraTarget)
            {
                _followTargetCamera.Target = _alternativeCameraTarget.transform;
            }
            else
            {
                _followTargetCamera.Target = _cameraTarget.transform;
            }
        }
    }
}
