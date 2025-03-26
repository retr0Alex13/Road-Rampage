using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Voidwalker
{
    public class LevelButton : MonoBehaviour
    {
        [field: SerializeField] public bool IsLocked { get; private set; } = true;

        [SerializeField] private GameObject _levelButton;
        [SerializeField] private GameObject _lockedLevelButton;

        private Button _unlockedButton;
        private TextMeshProUGUI _levelTitle;

        private LevelManager _levelManager;

        private void Awake()
        {
            _levelManager = FindFirstObjectByType<LevelManager>();

            _unlockedButton = _levelButton.GetComponent<Button>();

            _levelTitle = _levelButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateButtonState();
            InitializeButton();
        }

        private void InitializeButton()
        {
            int levelIndex = _levelManager.GetLevelIndex(gameObject) + 1;

            _levelTitle.text = levelIndex.ToString();

            _unlockedButton.onClick.AddListener(() =>
            {
                _levelManager.SetCurrentLevel(gameObject);
            });
        }

        private void UpdateButtonState()
        {
            if (IsLocked)
            {
                _levelButton.SetActive(false);
                _lockedLevelButton.SetActive(true);
            }
            else
            {
                _levelButton.SetActive(true);
                _lockedLevelButton.SetActive(false);
            }
        }

        public void SetIsLocked(bool state)
        {
            IsLocked = state;
            UpdateButtonState();
        }
    }
}
