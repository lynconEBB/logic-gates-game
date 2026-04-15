using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = (T)(MonoBehaviour)this;
            DontDestroyOnLoad(gameObject);
            Init();
        }

        protected virtual void Init() { }
    }
}
