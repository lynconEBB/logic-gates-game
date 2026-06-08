using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class PlaneAttachOffset : MonoBehaviour
    {
        [SerializeField] float m_Offset = 0.1f;

        public float Offset => m_Offset;
    }
}
