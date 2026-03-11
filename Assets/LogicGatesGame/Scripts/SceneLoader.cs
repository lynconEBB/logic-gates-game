using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LogicGatesGame.Scripts
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private SceneReference mainMenuScene;
        [SerializeField] private SceneReference gameScene;

        public void LoadMainMenu() => LoadScene(mainMenuScene);
        public void LoadGameScene() => LoadScene(gameScene);

        public void LoadScene(SceneReference scene)
        {
            SceneManager.LoadScene(scene.Name);
        }
    }
}
