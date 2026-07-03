using Eflatun.SceneReference;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class SceneLoaderTrigger : MonoBehaviour
    {
        public void LoadScene(SceneReference scene) => SceneLoader.Instance.LoadScene(scene);
        public void LoadMainMenu() => SceneLoader.Instance.LoadMainMenu();
        public void LoadGameScene() => SceneLoader.Instance.LoadGameScene();

        // Exit path for the game scene: persist the session as abandoned before
        // leaving. The save is awaited so the scene (and the SceneSingleton
        // TelemetryManager) is not torn down before the write completes.
        public async void SaveTelemetryAndExitToMainMenu()
        {
            if (TelemetryManager.Instance != null)
                await TelemetryManager.Instance.SaveAbandonedSessionAsync();

            SceneLoader.Instance.LoadMainMenu();
        }

        public void LoadGameSceneAsEasy()
        {
            DifficultyManager.SelectedDifficulty = Difficulty.Easy;
            LoadGameScene();
        }

        public void LoadGameSceneAsMedium()
        {
            DifficultyManager.SelectedDifficulty = Difficulty.Medium;
            LoadGameScene();
        }

        public void LoadGameSceneAsHard()
        {
            DifficultyManager.SelectedDifficulty = Difficulty.Hard;
            LoadGameScene();
        }
    }
}
