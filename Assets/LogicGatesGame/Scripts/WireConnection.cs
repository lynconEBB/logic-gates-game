using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class WireConnection : XRGrabInteractable
    {
        [SerializeField]
        private InteractionLayerMask socketLayerMask;
        
        bool IsNearTarget(IXRInteractor interactor)
        {
            if (interactor is XRDirectInteractor || interactor is XRSocketInteractor || interactor is XRProximityInteractor) return true;

            if (interactor is NearFarInteractor nearFar)
            {
                var nearCaster = nearFar.nearInteractionCaster;
                if (nearCaster == null || !nearCaster.isInitialized)
                    return false;

                List<Collider> s_NearColliders = new List<Collider>();
                bool got = nearCaster.TryGetColliderTargets(interactionManager, s_NearColliders);
                if (!got)
                    return false;

                foreach (var col in s_NearColliders)
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

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is XRSocketInteractor)
            {
                interactionLayers &= ~socketLayerMask;
            }
            else
            {
                interactionLayers |= socketLayerMask;
            }
            
        }
    }
}