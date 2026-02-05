using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

        public Node GetOtherNode()
        {
            return otherConnection.CurrentNode;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is ConnectionSocket socket)
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
                _lateDestroyRoutine = StartCoroutine(LateDestroyRoutine());
            }
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