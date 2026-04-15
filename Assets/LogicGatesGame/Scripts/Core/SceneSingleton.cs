using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = (T)(MonoBehaviour)this;
        }
    }
}
