using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class StateVisualizer : MonoBehaviour
    {
        [SerializeField] 
        private NodeComponent nodeComp;
        private Node _node;
        private MeshRenderer _renderer;

        public Material trueMaterial;
        public Material falseMaterial;
        public Material undefinedMaterial;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _renderer.material = undefinedMaterial;
        }

        private void Start()
        {
            if (!nodeComp || nodeComp.Node == null)
                return;
            
            SetNodeObserved(nodeComp.Node);
        }

        private void OnDestroy()
        {
            if (_node == null) 
                return;
            
            _node.OnEvaluated -= OnNodeEvaluated;
        }

        public void SetNodeObserved(Node node)
        {
            _node = node;
            _node.OnEvaluated += OnNodeEvaluated;
            _node.Evaluate();
        }

        private void OnNodeEvaluated(bool? state)
        {
            if (!state.HasValue)
            {
                _renderer.material = undefinedMaterial;
                return;
            }

            _renderer.material = state.Value ? trueMaterial : falseMaterial;
        }
    }
}