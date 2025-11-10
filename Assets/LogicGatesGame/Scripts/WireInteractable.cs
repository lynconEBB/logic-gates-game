using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class WireInteractable : XRSimpleInteractable
    {
        [SerializeField] 
        private XRGrabInteractable startInteractable;
        [SerializeField] 
        private XRGrabInteractable endInteractable;
        [SerializeField] 
        private SplineContainer splineContainer;

        private Spline _spline;
        private BezierKnot _startKnot;
        private BezierKnot _endKnot;

        protected override void Awake()
        {
            _spline = splineContainer.Spline;
            _startKnot = _spline[0];
            _endKnot = _spline[1];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            startInteractable.selectEntered.AddListener(OnStartSelected);    
            endInteractable.selectEntered.AddListener(OnEndSelected);
        }

        private void OnEndSelected(SelectEnterEventArgs args)
        {
            if (args.interactorObject is ConnectionSocket)
            {
                Vector3 target = splineContainer.transform.InverseTransformPoint(args.interactorObject.GetAttachTransform(endInteractable).position);
                _endKnot.Position = target;
                _spline[1] = _endKnot;
            }
        }

        private void OnStartSelected(SelectEnterEventArgs args)
        {
            if (args.interactorObject is ConnectionSocket)
            {
                Vector3 target = splineContainer.transform.InverseTransformPoint(args.interactorObject.GetAttachTransform(startInteractable).position);
                _startKnot.Position = target;
                _spline[0] = _startKnot;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            startInteractable.selectEntered.RemoveListener(OnStartSelected);    
            endInteractable.selectEntered.RemoveListener(OnEndSelected);
        }

        public void SelectStart(IXRSelectInteractor interactor)
        {
            interactionManager.SelectEnter(interactor, startInteractable);
        }

        public void SelectEnd(IXRSelectInteractor interactor)
        {
            interactionManager.SelectEnter(interactor, endInteractable);
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (startInteractable.isSelected)
                {
                    Vector3 target = splineContainer.transform.InverseTransformPoint(startInteractable.firstInteractorSelecting.GetAttachTransform(startInteractable).position);
                    _startKnot.Rotation = quaternion.identity;
                    _startKnot.Position = target;
                    _spline[0] = _startKnot;
                }

                if (endInteractable.isSelected)
                {
                    Vector3 target = splineContainer.transform.InverseTransformPoint(endInteractable.firstInteractorSelecting.GetAttachTransform(endInteractable).position);
                    _startKnot.Rotation = quaternion.identity;
                    _endKnot.Position = target;
                    _spline[1] = _endKnot;
                }
            }
        }
    }
}