using System.Collections.Generic;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public abstract class TelemetryManager : SceneSingleton<TelemetryManager>
    {
        private Dictionary<string, int> _data;

        private void Start()
        {
            _data = new Dictionary<string, int>();
            RegisterKeys();
        }

        protected abstract void RegisterKeys();

        protected void RegisterKey(string key)
        {
            if (_data.ContainsKey(key))
            {
                Debug.LogWarning($"[TelemetryManager] Key '{key}' is already registered.");
                return;
            }
            _data[key] = 0;
        }

        public void Increment(string key)
        {
            if (!_data.ContainsKey(key))
            {
                Debug.LogWarning($"[TelemetryManager] Key '{key}' not found.");
                return;
            }
            _data[key]++;
        }

        public void Decrement(string key)
        {
            if (!_data.ContainsKey(key))
            {
                Debug.LogWarning($"[TelemetryManager] Key '{key}' not found.");
                return;
            }
            _data[key] = Mathf.Max(0, _data[key] - 1);
        }

        public int GetCount(string key)
        {
            _data.TryGetValue(key, out var count);
            return count;
        }

        public IReadOnlyDictionary<string, int> GetAll() => _data;
    }
}
