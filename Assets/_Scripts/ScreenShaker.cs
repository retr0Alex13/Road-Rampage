using CarControllerwithShooting;
using Unity.Cinemachine;
using UnityEngine;

namespace Voidwalker
{
    public class ScreenShaker : MonoBehaviour
    {
        [SerializeField] private CinemachineBasicMultiChannelPerlin cinemachineMultiChannelPerlin;
        [SerializeField] private NoiseSettings noiseProfile;

        [SerializeField] private float screenShakeDuration = 0.3f;
        [SerializeField] private float maxDistance = 90f;
        [SerializeField] private float minDistance = 10f;
        [SerializeField] private float maxAmplitude = 5f;
        [SerializeField] private float maxFrequency = 5f;

        public async void StartShake(Vector3 explosionPosition)
        {
            float distance = Vector3.Distance(CarSystemManager.Instance.car.transform.position, explosionPosition);
            float t = Mathf.Clamp01((maxDistance - distance) / (maxDistance - minDistance));

            float amplitude = maxAmplitude * t;
            float frequency = maxFrequency * t;

            await Shake(amplitude, frequency);
        }

        private async Awaitable Shake(float amplitude, float frequency)
        {
            cinemachineMultiChannelPerlin.NoiseProfile = noiseProfile;
            cinemachineMultiChannelPerlin.AmplitudeGain = amplitude;
            cinemachineMultiChannelPerlin.FrequencyGain = frequency;

            await Awaitable.WaitForSecondsAsync(screenShakeDuration);

            cinemachineMultiChannelPerlin.AmplitudeGain = 1f;
            cinemachineMultiChannelPerlin.FrequencyGain = 1f;
            cinemachineMultiChannelPerlin.NoiseProfile = null;
        }
    }
}
