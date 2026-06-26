using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

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
            return IsXrActive() ? xrScene : firstPersonScene;
        }

        // A real HMD makes XRSettings.isDeviceActive true. The XR Interaction
        // Toolkit simulator does not, so detect it separately to still pick the
        // VR scene variant when simulating in the editor.
        private static bool IsXrActive()
        {
            return XRSettings.isDeviceActive || FindFirstObjectByType<XRInteractionSimulator>() != null;
        }
    }
}
