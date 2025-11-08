using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class Circuit
    {

    }

    public class Gate
    {

    }

    public class Wire
    {

    }

    public class CircuitController : MonoBehaviour
    {
        [Header("Ports")]
        [SerializeField]
        private List<Port> inputPorts = new();
        [SerializeField] 
        private Port outputPort;

        private List<Gate> gates = new();
        private List<Wire> wires;

        public void StartWire(Port port, IXRInteractor interactor)
        {
            
        }
        
        
    }
}