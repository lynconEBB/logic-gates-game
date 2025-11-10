using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class PortInteractable : XRSimpleInteractable
    {
        [SerializeField] 
        private Port port;
        [SerializeField]
        private WireInteractable wirePrefab;
        [SerializeField]
        private ConnectionSocket connectionSocket;
        private WireInteractable _lastWire;
        
        private CircuitController _circuitController;

        protected override void Awake()
        {
            base.Awake();
            
            _circuitController = GetComponentInParent<CircuitController>();
            _circuitController.AddPort(port);
        }
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            
            interactionManager.SelectCancel(args.interactorObject, this);
            
            _lastWire = Instantiate(wirePrefab);
            _lastWire.SelectStart(connectionSocket);
            _lastWire.SelectEnd(args.interactorObject);
        }

        bool IsNearTarget(IXRInteractor interactor)
        {
            if (interactor is XRDirectInteractor || interactor is XRSocketInteractor || interactor is XRProximityInteractor) return true;

            if (interactor is NearFarInteractor nearFar)
            {
                var nearCaster = nearFar.nearInteractionCaster;
                if (nearCaster == null || !nearCaster.isInitialized)
                    return false;

                List<Collider> nearColliders = new List<Collider>();
                bool got = nearCaster.TryGetColliderTargets(interactionManager, nearColliders);
                if (!got)
                    return false;

                foreach (var col in nearColliders)
                {
                    if (col == null) continue;

                    var candidate = col.GetComponentInParent<XRBaseInteractable>();
                    if (candidate == this)
                        return true;
                }
            }

            return false;
        }
        
        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            return IsNearTarget(interactor);
        }
    }
}