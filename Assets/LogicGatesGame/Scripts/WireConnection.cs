using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{

    public class WireConnection : XRGrabInteractable
    {
        [SerializeField]
        private WireConnection otherConnection;
        
        private static readonly int FRAMES_TO_DESTROY = 5;
        private Coroutine _lateDestroyRoutine;
        public event Action OnDestroyed;
        
        public Node CurrentNode
        {
            get;
            set;
        }

        public NodeComponent NodeComponent
        {
            get;
            set;
        }
        
        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            if (interactor is XRRayInteractor)
                return false;
            
            if (interactor is ConnectionSocket socket)
            {
                List<IXRHoverInteractor> socketsHovering = interactorsHovering.FindAll(i => i is ConnectionSocket);
                return (!socket.hasHover && socketsHovering.Count == 0) ||
                       (socket.IsHovering(this) && socket.interactablesHovered.Count == 1);
            }
            
            return base.IsHoverableBy(interactor);
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            if (interactor is ConnectionSocket socket)
            {
                return !socket.hasHover || socket.IsHovering(this);
            }
            return base.IsSelectableBy(interactor);
        }

        public Node GetOtherNode()
        {
            return otherConnection.CurrentNode;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is ConnectionSocket)
            {
                foreach (var coll in colliders)
                {
                    coll.enabled = false;
                }

                if (_lateDestroyRoutine != null)
                {
                    StopCoroutine(_lateDestroyRoutine);
                    _lateDestroyRoutine = null;
                }
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (args.interactorObject is not ConnectionSocket)
            {
                DetectFailedConnection();
                _lateDestroyRoutine = StartCoroutine(LateDestroyRoutine());
            }
        }

        private void DetectFailedConnection()
        {
            ConnectionSocket[] connectionSockets = interactorsHovering.OfType<ConnectionSocket>().ToArray();
            if (connectionSockets.Length == 0)
            {
                TelemetryManager.Instance?.Increment(TelemetryManager.KeyConnectionCanceled);
                return;
            }
            
            foreach (var socket in connectionSockets)
            {
                if (socket.NodeComponent.CanConnect(GetOtherNode()?.Id))
                    return;
            }
            
            Debug.LogWarning("Connection failed");
            TelemetryManager.Instance?.Increment(TelemetryManager.KeyConnectionFailed);
        }

        private IEnumerator LateDestroyRoutine()
        {
            for (int i = 0; i < FRAMES_TO_DESTROY; i++)
            {
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyed?.Invoke();
        }
    }
}