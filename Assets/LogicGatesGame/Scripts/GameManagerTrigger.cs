using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class GameManagerTrigger : MonoBehaviour
    {
        public void QuitGame() => ApplicationManager.Instance.QuitGame();
    }
}
