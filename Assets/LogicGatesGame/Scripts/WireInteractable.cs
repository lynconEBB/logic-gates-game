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
        private StateVisualizer stateVisualizer;

        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            return base.IsHoverableBy(interactor) 
                   && pointA.CurrentNode != null 
                   && pointB.CurrentNode != null;
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            return base.IsSelectableBy(interactor) && interactor is not ConnectionSocket 
                                                   && pointA.CurrentNode != null 
                                                   && pointB.CurrentNode != null;
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
            
            pointA.selectEntered.AddListener(OnPointSelectEntered);
            pointB.selectEntered.AddListener(OnPointSelectEntered);
        }

        private void OnPointSelectEntered(SelectEnterEventArgs args)
        {
            if (args.interactorObject is ConnectionSocket)
            {
                WireConnection wireConn = args.interactableObject as WireConnection;

                if (wireConn.NodeComponent.Type == NodeType.Output)
                {
                    stateVisualizer.SetNodeObserved(wireConn.CurrentNode);
                }
            }
        }

        private void OnSourceEvaluated(bool? state)
        {
            
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