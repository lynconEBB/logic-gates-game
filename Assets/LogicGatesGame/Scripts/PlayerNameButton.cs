using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogicGatesGame.Scripts
{
    /// <summary>
    /// A single selectable player-name entry on the main-menu name page.
    /// Owns its own label reference and click wiring so the spawning
    /// controller does not need to know the prefab's internal structure.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PlayerNameButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

        private string _playerName;
        private Action<string> _onSelected;

        private void Reset()
        {
            button = GetComponent<Button>();
            label = GetComponentInChildren<TMP_Text>();
        }

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            button.onClick.AddListener(HandleClick);
        }

        public void Setup(string playerName, Action<string> onSelected)
        {
            _playerName = playerName;
            _onSelected = onSelected;

            if (label != null)
                label.text = playerName;
        }

        private void HandleClick()
        {
            _onSelected?.Invoke(_playerName);
        }
    }
}
