using UnityEngine;

namespace Voidwalker
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] _levelButtons;
        [SerializeField] private GameObject[] _lockedLevelButtons;

        private const string LEVEL_PREFS_KEY = "UnlockedLevel_";
        private int _maxUnlockedLevel = 0;

        private void Start()
        {
            _maxUnlockedLevel = GetLatestUnlockedLevel();

            if (_maxUnlockedLevel == 0)
            {
                UnlockLevel(0);
            }

            UpdateLevelButtons();
        }

        private int GetLatestUnlockedLevel()
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(LEVEL_PREFS_KEY + "Max", 0));
        }

        private void UnlockLevel(int levelIndex)
        {
            _maxUnlockedLevel = levelIndex;
            PlayerPrefs.SetInt(LEVEL_PREFS_KEY + "Max", _maxUnlockedLevel);

            UpdateLevelButtons();

            PlayerPrefs.Save();
        }

        private void UpdateLevelButtons()
        {
            for (int i = 0; i <= _maxUnlockedLevel; i++)
            {
                _levelButtons[i].SetActive(true);
                _lockedLevelButtons[i].SetActive(false);
            }
        }

        public void UnlockNextLevel()
        {
            int currentLevel = GetLatestUnlockedLevel();
            UnlockLevel(currentLevel + 1);
        }
    }
}