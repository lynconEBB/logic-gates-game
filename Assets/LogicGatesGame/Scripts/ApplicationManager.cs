using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LogicGatesGame.Scripts
{
    public class ApplicationManager : Singleton<ApplicationManager>
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
