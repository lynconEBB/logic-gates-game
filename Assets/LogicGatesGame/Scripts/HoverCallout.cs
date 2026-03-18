using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class HoverCallout : MonoBehaviour
    {
        [SerializeField] 
        private XRBaseInteractable interactable;
        [SerializeField] 
        private GameObject calloutPrefab;
        [SerializeField] 
        private float heightOffset = 0.3f;
        [SerializeField] 
        private float spawnAnimationDuration = 0.2f;
        [SerializeField]
        private bool updatePositionAlways;
        [SerializeField]
        private CalloutPositionMode positionMode;
        [SerializeField]
        private float lookDirectionOffset = 0f;

        private enum CalloutPositionMode { Interactable, InteractionPoint }

        private GameObject _calloutInstance;
        private Camera _camera;
        private int _hoverCount;
        private Vector3 _calloutOriginalScale;
        private Vector3 _scaleTarget;
        private Vector3 _scaleVelocity;
        private IXRInteractor _currentInteractor;

        private void Awake()
        {
            if (!interactable)
                interactable = GetComponent<XRBaseInteractable>();

            if (!interactable || !calloutPrefab)
                throw new System.Exception("HoverCallout requires an interactable and a callout prefab");
            
            _camera = Camera.main;
            _calloutOriginalScale = calloutPrefab.transform.localScale;
        }

        private void Start()
        {
            _calloutInstance = Instantiate(calloutPrefab);
            _calloutInstance.transform.position = transform.position + Vector3.up * heightOffset;
            _calloutInstance.transform.localScale = Vector3.zero;
            _scaleTarget = Vector3.zero;
            _calloutInstance.SetActive(false);
        }

        private void OnEnable()
        {
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }

        private void OnDisable()
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            _hoverCount++;
            if (_hoverCount == 1)
            {
                _currentInteractor = args.interactorObject;
                _calloutInstance.transform.position = GetCalloutPosition();
                _calloutInstance.SetActive(true);
                _scaleTarget = _calloutOriginalScale;
            }
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            _hoverCount--;
            if (_hoverCount <= 0)
            {
                _currentInteractor = null;
                _scaleTarget = Vector3.zero;
                _hoverCount = 0;
            }
        }

        private Vector3 GetCalloutPosition()
        {
            Vector3 lookOffset = _camera != null
                ? Vector3.ProjectOnPlane(_camera.transform.position - transform.position, Vector3.up).normalized * lookDirectionOffset
                : Vector3.zero;

            if (positionMode == CalloutPositionMode.InteractionPoint && _currentInteractor != null)
                return interactable.GetAttachTransform(_currentInteractor).position + Vector3.up * heightOffset + lookOffset;

            return transform.position + Vector3.up * heightOffset + lookOffset;
        }

        private void LateUpdate()
        {
            if (!_calloutInstance.activeSelf || _camera == null) return;
            if (updatePositionAlways)
                _calloutInstance.transform.position = GetCalloutPosition();

            Vector3 toCamera = _camera.transform.position - _calloutInstance.transform.position;
            _calloutInstance.transform.rotation = Quaternion.LookRotation(toCamera);

            Vector3 currentScale = _calloutInstance.transform.localScale;
            if ((currentScale - _scaleTarget).sqrMagnitude > 0.0000001f)
            {
                _calloutInstance.transform.localScale = Vector3.SmoothDamp(
                    currentScale, _scaleTarget, ref _scaleVelocity, spawnAnimationDuration);
            }
            else
            {
                _calloutInstance.transform.localScale = _scaleTarget;
                _scaleVelocity = Vector3.zero;
                if (_scaleTarget == Vector3.zero)
                    _calloutInstance.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_calloutInstance)
                Destroy(_calloutInstance.gameObject);
        }
    }
}
