using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Voidwalker
{
    public class SetLevelID : MonoBehaviour
    {
        private LevelManager _levelManager;

        private Button _button;

        private TextMeshProUGUI _levelLabel;

        private void Start()
        {
            _levelManager = FindFirstObjectByType<LevelManager>();

            _levelLabel = GetComponentInChildren<TextMeshProUGUI>();

            _button = GetComponent<Button>();

            _button.onClick.AddListener(() =>
            {
                _levelManager.SetCurrentLevel(int.Parse(_levelLabel.text) - 1);
            });
        }
    }
}
