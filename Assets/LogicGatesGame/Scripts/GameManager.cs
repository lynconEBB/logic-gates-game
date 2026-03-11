using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LogicGatesGame.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
