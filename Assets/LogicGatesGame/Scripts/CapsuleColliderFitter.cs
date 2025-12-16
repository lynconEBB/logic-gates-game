using UnityEngine;

namespace LogicGatesGame.Scripts
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleColliderFitter : MonoBehaviour
    {
        [SerializeField] 
        private Transform transformA;
        [SerializeField]
        private Transform transformB;
        
        private CapsuleCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<CapsuleCollider>();
        }

        public void Fit()
        {
            _collider.transform.position = (transformA.position + transformB.position) / 2;
            Vector3 diff = transformA.position - transformB.position;
            _collider.transform.rotation = Quaternion.LookRotation(diff.normalized);
            _collider.height = Mathf.Max(diff.magnitude - 0.1f, 0f);
        }

        private void Update()
        {
            Fit();
        }
    }
}