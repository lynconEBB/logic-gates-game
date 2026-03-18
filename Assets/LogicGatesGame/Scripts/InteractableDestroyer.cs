using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    [RequireComponent(typeof(BoxCollider))]
    public class InteractableDestroyer : MonoBehaviour
    {
        private readonly HashSet<XRBaseInteractable> _heldInside = new();

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var interactable = GetInteractable(other);
            if (interactable == null) return;

            if (!interactable.isSelected)
            {
                Destroy(interactable.gameObject);
                return;
            }

            if (_heldInside.Add(interactable))
                interactable.selectExited.AddListener(OnSelectExited);
        }

        private void OnTriggerExit(Collider other)
        {
            var interactable = GetInteractable(other);
            if (interactable == null) return;

            if (_heldInside.Remove(interactable))
                interactable.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            var interactable = args.interactableObject as XRBaseInteractable;
            if (interactable == null) return;

            if (_heldInside.Contains(interactable))
                Destroy(interactable.gameObject);
        }

        private static XRBaseInteractable GetInteractable(Collider other)
        {
            var root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
            return root.GetComponent<XRBaseInteractable>();
        }
    }
}
