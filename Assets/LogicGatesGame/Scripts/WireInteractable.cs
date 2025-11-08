using System;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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
        }

        private void Start()
        {
            _spline = splineContainer.Spline;
            _startKnot = _spline[0];
            _endKnot = _spline[1];
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (startInteractable.isSelected)
                {
                    Vector3 target = splineContainer.transform.InverseTransformPoint(startInteractable.firstInteractorSelecting.GetAttachTransform(startInteractable).position);
                    _startKnot.Position = target;
                    _spline[0] = _startKnot;
                }

                if (endInteractable.isSelected)
                {
                    Vector3 target = splineContainer.transform.InverseTransformPoint(endInteractable.firstInteractorSelecting.GetAttachTransform(endInteractable).position);
                    _endKnot.Position = target;
                    _spline[1] = _endKnot;
                }
            }
        }
    }
}