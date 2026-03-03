using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class HoverCallout : MonoBehaviour
    {
        [SerializeField] private XRBaseInteractable interactable;
        [SerializeField] private GameObject calloutPrefab;
        [SerializeField] private float heightOffset = 0.3f;

        private GameObject _calloutInstance;
        private Camera _camera;
        private int _hoverCount;

        private void Awake()
        {
            if (interactable == null)
                interactable = GetComponent<XRBaseInteractable>();

            _camera = Camera.main;
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
            if (_calloutInstance == null)
                _calloutInstance = Instantiate(calloutPrefab);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            _hoverCount--;
            if (_hoverCount <= 0)
            {
                Destroy(_calloutInstance);
                _calloutInstance = null;
                _hoverCount = 0;
            }
        }

        private void LateUpdate()
        {
            if (_calloutInstance == null || _camera == null) return;

            _calloutInstance.transform.position = transform.position + Vector3.up * heightOffset;

            Vector3 toCamera = _camera.transform.position - _calloutInstance.transform.position;
            _calloutInstance.transform.rotation = Quaternion.LookRotation(toCamera);
        }
    }
}
