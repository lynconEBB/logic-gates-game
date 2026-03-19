using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR.Interaction.Toolkit;

namespace LogicGatesGame.Scripts
{
    public class WireSplineController : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private WireConnection pointA;
        [SerializeField] private WireConnection pointB;
        [SerializeField] private Transform midpoint;

        private Spline _spline;
        private BezierKnot _startKnot;
        private BezierKnot _endKnot;

        protected void Awake()
        {
            _spline = splineContainer.Spline;
            _startKnot = _spline[0];
            _endKnot = _spline[1];
        }

        private void OnEnable()
        {
            pointA.selectEntered.AddListener(OnPointSelected);
            pointB.selectEntered.AddListener(OnPointSelected);
        }

        private void OnPointSelected(SelectEnterEventArgs args)
        {
            if (args.interactorObject is ConnectionSocket)
            {
                Vector3 target = splineContainer.transform.InverseTransformPoint(args.interactorObject.GetAttachTransform(args.interactableObject).position);
                if (ReferenceEquals(args.interactableObject, pointA))
                {
                    _startKnot.Position = target;
                    _spline[0] = _startKnot;
                }
                else
                {
                    _endKnot.Position = target;
                    _spline[1] = _endKnot;
                }
            }
        }

        private void OnDisable()
        {
            pointA.selectEntered.RemoveListener(OnPointSelected);    
            pointB.selectEntered.RemoveListener(OnPointSelected);
        }

        private void Update()
        {
            if (pointA.isSelected)
            {
                Vector3 target = splineContainer.transform.InverseTransformPoint(pointA.firstInteractorSelecting.GetAttachTransform(pointA).position);
                _startKnot.Rotation = quaternion.identity;
                _startKnot.Position = target;
                _spline[0] = _startKnot;
            }

            if (pointB.isSelected)
            {
                Vector3 target = splineContainer.transform.InverseTransformPoint(pointB.firstInteractorSelecting.GetAttachTransform(pointB).position);
                _startKnot.Rotation = quaternion.identity;
                _endKnot.Position = target;
                _spline[1] = _endKnot;
            }

            if (midpoint != null)
            {
                Vector3 worldA = splineContainer.transform.TransformPoint(_spline[0].Position);
                Vector3 worldB = splineContainer.transform.TransformPoint(_spline[1].Position);
                midpoint.position = (worldA + worldB) * 0.5f + Vector3.up;
            }
        }
    }
}