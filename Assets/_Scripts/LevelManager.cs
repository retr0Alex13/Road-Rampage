using TMPro;
using UnityEngine;
namespace Voidwalker
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] _levelButtons;
        [SerializeField] private GameObject[] _lockedLevelButtons;
        private const string LEVEL_PREFS_KEY = "UnlockedLevel_";
        private const string CURRENT_LEVEL_KEY = "CurrentLevel_";
        private int _maxUnlockedLevel = 0;
        private int _currentLevel = 0;

        private void Start()
        {
            //PlayerPrefs.DeleteAll();
            _maxUnlockedLevel = GetLatestUnlockedLevel();
            _currentLevel = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);

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
            _maxUnlockedLevel = Mathf.Max(_maxUnlockedLevel, levelIndex);
            PlayerPrefs.SetInt(LEVEL_PREFS_KEY + "Max", _maxUnlockedLevel);
            UpdateLevelButtons();
            PlayerPrefs.Save();
        }

        private void UpdateLevelButtons()
        {
            for (int i = 0; i <= _maxUnlockedLevel && i < _levelButtons.Length; i++)
            {
                _levelButtons[i].SetActive(true);
                _lockedLevelButtons[i].SetActive(false);
                _levelButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
            }
        }

        public void SetCurrentLevel(int level)
        {
            _currentLevel = level;
            PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, _currentLevel);
            PlayerPrefs.Save();
        }

        public void UnlockNextLevel()
        {
            int nextLevel = _currentLevel + 1;
            if (nextLevel < _levelButtons.Length)
            {
                UnlockLevel(nextLevel);
            }
        }

        public int GetCurrentLevel()
        {
            return _currentLevel;
        }
    }
}