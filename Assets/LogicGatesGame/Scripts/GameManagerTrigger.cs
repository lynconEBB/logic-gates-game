using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class GameManagerTrigger : MonoBehaviour
    {
        public void QuitGame() => GameManager.Instance.QuitGame();
    }
}
