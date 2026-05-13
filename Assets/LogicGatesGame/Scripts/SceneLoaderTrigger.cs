using Eflatun.SceneReference;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class SceneLoaderTrigger : MonoBehaviour
    {
        public void LoadScene(SceneReference scene) => SceneLoader.Instance.LoadScene(scene);
        public void LoadMainMenu() => SceneLoader.Instance.LoadMainMenu();
        public void LoadGameScene() => SceneLoader.Instance.LoadGameScene();

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
