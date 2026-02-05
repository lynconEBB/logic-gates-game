using System;
using UnityEngine;

namespace LogicGatesGame.Scripts
{
    public class StateVisualizer : MonoBehaviour
    {
        [SerializeField] 
        private NodeComponent nodeComp;
        private Node _node;
        private Material _material;

        public Color trueColor = Color.cyan;
        public Color falseColor = Color.white;
        public Color undefinedColor = Color.paleVioletRed;
        
        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            _material.color = undefinedColor; 
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
                _material.color = undefinedColor; 
                return;
            }

            _material.color = state.Value ? trueColor : falseColor;
        }
    }
}