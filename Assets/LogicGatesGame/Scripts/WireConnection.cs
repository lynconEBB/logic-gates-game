using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{

public class WireConnection : XRGrabInteractable
    {
        private Coroutine _lateDestroyRoutine;
        public event Action OnDestroyed;
        
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (_lateDestroyRoutine != null)
            {
                StopCoroutine(_lateDestroyRoutine);
                _lateDestroyRoutine = null;
            }

            if (args.interactorObject is ConnectionSocket)
            {
                 
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            Debug.Log("Wire connection exited");
            foreach (var hoverInteractor in interactorsHovering)
            {
                Debug.Log((hoverInteractor as MonoBehaviour).name);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyed?.Invoke();
        }
    }
}