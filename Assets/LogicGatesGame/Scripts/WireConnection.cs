using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{

public class WireConnection : XRGrabInteractable
    {
        private static readonly int FRAMES_TO_DESTROY = 5;
        private Coroutine _lateDestroyRoutine;
        public event Action OnDestroyed;
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is ConnectionSocket socket)
            {
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
            Debug.Log("Wire connection exited");
        }

        private IEnumerator LateDestroyRoutine()
        {
            for (int i = 0; i < FRAMES_TO_DESTROY; i++)
            {
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
            // The connection is destroyed here
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyed?.Invoke();
        }
    }
}