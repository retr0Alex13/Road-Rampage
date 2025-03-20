using UnityEngine;
using UnityEngine.UI;

namespace Voidwalker
{
    public class SettingsChanger : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private float _defaultVolume = 0.2f;

        private void Start()
        {
            LoadVolumeSettings();

            _slider.onValueChanged.AddListener((v) =>
            {
                AudioListener.volume = v;
                PlayerPrefs.SetFloat("SoundVolume", v);
            });
        }

        private void LoadVolumeSettings()
        {
            if (PlayerPrefs.HasKey("SoundVolume"))
            {
                _slider.value = PlayerPrefs.GetFloat("SoundVolume");
                AudioListener.volume = _slider.value;
            }
            else
            {
                _slider.value = _defaultVolume;
                AudioListener.volume = _defaultVolume;
            }
        }
    }
}
