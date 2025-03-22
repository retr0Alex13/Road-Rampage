using UnityEngine;
using UnityEngine.SceneManagement;

namespace Voidwalker
{
    public class SceneController : MonoBehaviour
    {
       public void LoadThisScene(string name)
       {
            SceneManager.LoadScene(name);
       }

        public void LoadNextScene()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}
