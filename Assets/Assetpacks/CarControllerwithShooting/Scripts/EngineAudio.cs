using UnityEngine;

namespace CarControllerwithShooting
{
    public class EngineAudio : MonoBehaviour
    {
        public enum EngineAudioOptions
        {
            Simple,
            FourChannel
        }
        [Header("Style")]
        public EngineAudioOptions engineSoundStyle = EngineAudioOptions.FourChannel;
        [Header("Clips"), Space(10)]
        public AudioClip lowAccelClip;
        public AudioClip lowDecelClip;
        public AudioClip highAccelClip;
        public AudioClip highDecelClip;
        [Header("Sound Settings"), Space(10)]
        public float pitchMultiplier = 1f;
        [SerializeField] private float _lowPitchMin = 1f;
        [SerializeField] private float _lowPitchMax = 6f;
        [SerializeField] private float _highPitchMultiplier = 0.25f;
        [SerializeField] private float _maxRolloffDistance = 500f;
        [SerializeField] private float _dopplerLevel = 1f;
        [SerializeField] private bool _useDoppler = true;

        private AudioSource _lowAccel;
        private AudioSource _lowDecel;
        private AudioSource _highAccel;
        private AudioSource _highDecel;
        private bool _soundStarted;
        private bool _stopEngineSound;
        private CarController _carController;

        private void StartSound()
        {
            _carController = GetComponent<CarController>();

            _highAccel = SetupEngineAudioSource(highAccelClip);

            if (engineSoundStyle == EngineAudioOptions.FourChannel)
            {
                _lowAccel = SetupEngineAudioSource(lowAccelClip);
                _lowDecel = SetupEngineAudioSource(lowDecelClip);
                _highDecel = SetupEngineAudioSource(highDecelClip);
            }

            _soundStarted = true;
        }

        private void StopSound()
        {
            if (_highAccel != null)
                _highAccel.Stop();

            if (engineSoundStyle == EngineAudioOptions.FourChannel)
            {
                if (_lowAccel != null)
                    _lowAccel.Stop();
                if (_lowDecel != null)
                    _lowDecel.Stop();
                if (_highDecel != null)
                    _highDecel.Stop();
            }

            _soundStarted = false;
        }

        private void Update()
        {
            if (Gasoline.Instance != null && Gasoline.Instance.CurrentFuel <= 0)
            {
                StopEngineSounds();
                return;
            }
            else if (_stopEngineSound)
            {
                StopEngineSounds();
                return;
            }

            if (!_soundStarted)
                StartSound();

            if (_soundStarted)
            {
                float pitch = UnclampedLerp(_lowPitchMin, _lowPitchMax, _carController.Revs);
                pitch = Mathf.Min(_lowPitchMax, pitch);

                if (engineSoundStyle == EngineAudioOptions.Simple)
                {
                    _highAccel.pitch = pitch * pitchMultiplier * _highPitchMultiplier;
                    _highAccel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
                    _highAccel.volume = 1;
                }
                else
                {
                    _lowAccel.pitch = pitch * pitchMultiplier;
                    _lowDecel.pitch = pitch * pitchMultiplier;
                    _highAccel.pitch = pitch * _highPitchMultiplier * pitchMultiplier;
                    _highDecel.pitch = pitch * _highPitchMultiplier * pitchMultiplier;

                    float accFade = Mathf.Abs(_carController.AccelInput);
                    float decFade = 1 - accFade;

                    float highFade = Mathf.InverseLerp(0.2f, 0.8f, _carController.Revs);
                    float lowFade = 1 - highFade;

                    highFade = 1 - ((1 - highFade) * (1 - highFade));
                    lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
                    accFade = 1 - ((1 - accFade) * (1 - accFade));
                    decFade = 1 - ((1 - decFade) * (1 - decFade));

                    _lowAccel.volume = lowFade * accFade;
                    _lowDecel.volume = lowFade * decFade;
                    _highAccel.volume = highFade * accFade;
                    _highDecel.volume = highFade * decFade;

                    _highAccel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
                    _lowAccel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
                    _highDecel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
                    _lowDecel.dopplerLevel = _useDoppler ? _dopplerLevel : 0;
                }
            }
        }

        public void SetStopEngineSound(bool value)
        {
            _stopEngineSound = value;
        }

        private void StopEngineSounds()
        {
            // Gradually fade out engine sounds if they're playing
            if (_soundStarted)
            {
                FadeOutEngineSounds();

                // If sounds have completely faded out, stop them
                if (_highAccel.volume <= 0 &&
                    (_lowAccel == null || _lowAccel.volume <= 0) &&
                    (_lowDecel == null || _lowDecel.volume <= 0) &&
                    (_highDecel == null || _highDecel.volume <= 0))
                {
                    StopSound();
                }
            }
            return;
        }

        private void FadeOutEngineSounds()
        {
            float fadeSpeed = 1.5f * Time.deltaTime; // Adjust speed as needed

            if (_highAccel != null)
                _highAccel.volume = Mathf.Max(0, _highAccel.volume - fadeSpeed);

            if (engineSoundStyle == EngineAudioOptions.FourChannel)
            {
                if (_lowAccel != null)
                    _lowAccel.volume = Mathf.Max(0, _lowAccel.volume - fadeSpeed);
                if (_lowDecel != null)
                    _lowDecel.volume = Mathf.Max(0, _lowDecel.volume - fadeSpeed);
                if (_highDecel != null)
                    _highDecel.volume = Mathf.Max(0, _highDecel.volume - fadeSpeed);
            }
        }

        private AudioSource SetupEngineAudioSource(AudioClip clip)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = 0;
            source.loop = true;
            source.time = Random.Range(0f, clip.length);
            source.Play();
            source.minDistance = 5f;
            source.maxDistance = _maxRolloffDistance;
            source.dopplerLevel = 0;
            return source;
        }

        private static float UnclampedLerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }
    }
}