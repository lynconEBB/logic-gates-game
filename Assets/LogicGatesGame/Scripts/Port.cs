using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public enum PortDirection
    {
        Input,
        Output 
    } 
    
    [CreateAssetMenu(fileName = "Port", menuName = "LogicGatesGame/Port", order = 0)]
    public class Port : ScriptableObject
    {
        public PortDirection direction;
        public bool initialValue;
        [HideInInspector]
        public bool value;
    }
}