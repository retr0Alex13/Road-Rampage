using System;
using UnityEngine;
namespace Voidwalker
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] _levels;

        private const string LATEST_LEVEL = "savedgame_latestlevel";
        private const string CURRENT_LEVEL = "savedgame_level";

        private void Start()
        {
            InitializeLevels();
            AdManager.Instance.ShowCommercialBreak();
            PokiUnitySDK.Instance.gameplayStart();
        }

        private void InitializeLevels()
        {
            int latestLevel = PlayerPrefs.GetInt(LATEST_LEVEL, 0);

            if (latestLevel == 0)
            {
                UnlockLevel(latestLevel);
            }
            else
            {
                if (latestLevel > _levels.Length) return;

                for (int i = 0; i <= latestLevel; i++)
                {
                    UnlockLevel(i);
                }
            }
        }

        private void UnlockLevel(int i)
        {
            LevelButton levelButton = _levels[i].GetComponent<LevelButton>();
            levelButton.SetIsLocked(false);
        }

        public void SetCurrentLevel(GameObject level)
        {
            int levelIndex = GetLevelIndex(level);
            PlayerPrefs.SetInt(CURRENT_LEVEL, levelIndex);
        }

        public int GetLevelIndex(GameObject level)
        {
            int levelIndex = Array.IndexOf(_levels, level);
            return levelIndex;
        }

        public void CompleteLevel(int levelIndex)
        {
            int latestLevel = PlayerPrefs.GetInt(LATEST_LEVEL, 0);

            if (levelIndex >= latestLevel)
            {
                int nextLevelIndex = levelIndex + 1;

                if (nextLevelIndex < _levels.Length)
                {
                    PlayerPrefs.SetInt(LATEST_LEVEL, nextLevelIndex);
                    UnlockLevel(nextLevelIndex);
                    PlayerPrefs.Save();
                }
            }
        }
    }
}