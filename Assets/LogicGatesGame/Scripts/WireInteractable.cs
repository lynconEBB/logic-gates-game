using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class WireInteractable : XRSimpleInteractable
    {
        [FormerlySerializedAs("startInteractable")] [SerializeField] 
        private WireConnection pointA;
        [FormerlySerializedAs("endInteractable")] [SerializeField] 
        private WireConnection pointB;

        [SerializeField] 
        private MeshRenderer renderer;


        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            return base.IsHoverableBy(interactor) 
                   && pointA.CurrentNodeId.HasValue 
                   && pointB.CurrentNodeId.HasValue;
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            return base.IsSelectableBy(interactor) && interactor is not ConnectionSocket 
                                                   && pointA.CurrentNodeId.HasValue 
                                                   && pointB.CurrentNodeId.HasValue;
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            renderer.material.color = Color.yellow;     
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            if (renderer)
                renderer.material.color = Color.white;     
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            Destroy(gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            pointA.OnDestroyed += AutoDestroy;
            pointB.OnDestroyed += AutoDestroy;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            pointA.OnDestroyed -= AutoDestroy;
            pointB.OnDestroyed -= AutoDestroy;
        }

        public void SelectStart(IXRSelectInteractor interactor)
        {
            interactionManager.SelectEnter(interactor, pointA);
        }

        public void SelectEnd(IXRSelectInteractor interactor)
        {
            interactionManager.SelectEnter(interactor, pointB);
        }

        public void AutoDestroy()
        {
           Destroy(gameObject); 
        }
    }
}