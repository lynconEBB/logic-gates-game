using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace LogicGatesGame.Scripts
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private SceneReference mainMenuScene;
        [SerializeField] private SceneReference gameScene;
        [SerializeField] private SceneReference firstPersonMainMenuScene;
        [SerializeField] private SceneReference firstPersonGameScene;

        public void LoadMainMenu() => LoadScene(PickScene(mainMenuScene, firstPersonMainMenuScene));
        public void LoadGameScene() => LoadScene(PickScene(gameScene, firstPersonGameScene));

        public void LoadScene(SceneReference scene)
        {
            SceneManager.LoadScene(scene.Name);
        }

        private static SceneReference PickScene(SceneReference xrScene, SceneReference firstPersonScene)
        {
            return XRSettings.isDeviceActive ? xrScene : firstPersonScene;
        }
    }
}
