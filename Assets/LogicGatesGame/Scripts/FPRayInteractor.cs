using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class FPRayInteractor : XRRayInteractor
    {
        [SerializeField] LayerMask m_AttachPlaneLayer;

        Vector3 m_DefaultAttachLocalPosition;
        Quaternion m_DefaultAttachLocalRotation;
        bool m_DefaultAttachCaptured;

        protected override void Start()
        {
            base.Start();
            CaptureDefaultAttachPosition();
        }

        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                return;

            if (attachTransform == null || attachTransform == transform || m_AttachPlaneLayer == 0)
                return;

            if (!m_DefaultAttachCaptured)
                CaptureDefaultAttachPosition();

            var origin = rayOriginTransform != null ? rayOriginTransform : transform;

            if (Physics.Raycast(origin.position, origin.forward, out var hit, Mathf.Infinity, m_AttachPlaneLayer))
            {
                attachTransform.position = hit.point + hit.normal * GetPlaneAttachOffset();
                var forward = Vector3.ProjectOnPlane(origin.forward, hit.normal).normalized;
                if (forward == Vector3.zero)
                    forward = Vector3.ProjectOnPlane(origin.up, hit.normal).normalized;
                attachTransform.rotation = Quaternion.LookRotation(forward, hit.normal);
            }
            else
            {
                attachTransform.localPosition = m_DefaultAttachLocalPosition;
                attachTransform.localRotation = m_DefaultAttachLocalRotation;
            }
        }

        public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride
            => XRBaseInteractable.MovementType.Instantaneous;

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            if (args.interactableObject is XRGrabInteractable grab)
            {
                grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
                grab.attachEaseInTime = 0f;
            }
        }

        public override Transform GetAttachTransform(IXRInteractable interactable)
        {
            return base.GetAttachTransform(interactable);
        }

        float GetPlaneAttachOffset()
        {
            if (!hasSelection)
                return 0f;

            var grabbed = interactablesSelected[0] as MonoBehaviour;
            if (grabbed != null && grabbed.TryGetComponent<PlaneAttachOffset>(out var provider))
                return provider.Offset;

            return 0f;
        }

        void CaptureDefaultAttachPosition()
        {
            if (attachTransform != null && attachTransform != transform)
            {
                m_DefaultAttachLocalPosition = attachTransform.localPosition;
                m_DefaultAttachLocalRotation = attachTransform.localRotation;
                m_DefaultAttachCaptured = true;
            }
        }
    }
}
