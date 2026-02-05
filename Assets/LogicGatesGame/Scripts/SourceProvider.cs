using UnityEngine;
using UnityEngine.Events;

namespace LogicGatesGame.Scripts
{
    public class SourceProvider : MonoBehaviour
    {
        private bool _value = false;
        
        public bool Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged.Invoke(_value);
            }
        }
        public event UnityAction<bool> OnValueChanged;
        
        public void ToggleValue() => Value = !Value;
    }
}