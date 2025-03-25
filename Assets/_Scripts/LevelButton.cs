using UnityEngine;
using UnityEngine.UI;

namespace Voidwalker
{
    public class LevelButton : MonoBehaviour
    {
        [field:SerializeField] public bool IsLocked { get; private set; } = true;

        [SerializeField] private GameObject _levelButton;
        [SerializeField] private GameObject _lockedLevelButton;

        private void Start()
        {
            UpdateButtonState();
            InitializeButton();
        }

        private void InitializeButton()
        {
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();

            Button button = _levelButton.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                levelManager.SetCurrentLevel(gameObject);
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
