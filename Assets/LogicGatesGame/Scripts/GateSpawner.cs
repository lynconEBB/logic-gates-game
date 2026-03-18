using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace LogicGatesGame.Scripts
{
    public class GateDestroyNotifier : MonoBehaviour
    {
        public event Action OnGateDestroyed;

        private void OnDestroy() => OnGateDestroyed?.Invoke();
    }

    public class GateSpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("Gate prefab to spawn. Must be an XRBaseInteractable.")]
        private XRGrabInteractable _gatePrefab;

        [SerializeField, Tooltip("Maximum number of simultaneously live gates from this spawner.")]
        private int _maxGates = 5;

        [SerializeField, Tooltip("Distance from spawn point at which the held gate is freed and a new one spawns.")]
        private float _freeDistance = 0.5f;

        [SerializeField, Tooltip("Duration of the spawn scale-in animation in seconds.")]
        private float _spawnAnimDuration = 0.3f;

        private readonly List<XRGrabInteractable> _spawnedGates = new();
        private XRGrabInteractable _pendingGate;

        private static bool _isQuitting;

        private void Start()
        {
            SpawnGate(animate: false);
        }

        private void Update()
        {
            if (_pendingGate == null || !_pendingGate.isSelected)
                return;

            float dist = Vector3.Distance(_pendingGate.transform.position, transform.position);
            if (dist >= _freeDistance)
                FreePendingGate();
        }

        private void SpawnGate(bool animate = true)
        {
            if (_spawnedGates.Count >= _maxGates || _pendingGate != null)
                return;

            var gate = Instantiate(_gatePrefab, transform.position, transform.rotation);
            gate.movementType = XRBaseInteractable.MovementType.Kinematic;
            var rb = gate.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            var notifier = gate.gameObject.AddComponent<GateDestroyNotifier>();
            notifier.OnGateDestroyed += () => OnTrackedGateDestroyed(gate);

            _spawnedGates.Add(gate);
            _pendingGate = gate;

            gate.selectEntered.AddListener(OnPendingGateGrabbed);
            gate.selectExited.AddListener(OnPendingGateReleased);

            if (animate)
                StartCoroutine(ScaleIn(gate.transform));
        }

        private void OnTrackedGateDestroyed(XRGrabInteractable gate)
        {
            if (_isQuitting) return;

            _spawnedGates.Remove(gate);
            if (gate == _pendingGate)
            {
                _pendingGate = null;
            }

            SpawnGate(animate: true);
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnPendingGateGrabbed(SelectEnterEventArgs args)
        {
            var rb = _pendingGate.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            _pendingGate.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        }

        private void OnPendingGateReleased(SelectExitEventArgs args)
        {
            if (_pendingGate == null || _pendingGate.isSelected)
                return;

            // StartCoroutine(SnapBack(_pendingGate));
            
            _pendingGate.movementType = XRBaseInteractable.MovementType.Kinematic;
            var rb = _pendingGate.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            _pendingGate.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        private IEnumerator ScaleIn(Transform target)
        {
            var targetScale = target.localScale;
            target.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < _spawnAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _spawnAnimDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f); // cubic ease-out
                target.localScale = targetScale * eased;
                yield return null;
            }

            target.localScale = targetScale;
        }

        private void FreePendingGate()
        {
            _pendingGate.selectEntered.RemoveListener(OnPendingGateGrabbed);
            _pendingGate.selectExited.RemoveListener(OnPendingGateReleased);
            _pendingGate =  null;
            
            SpawnGate(animate: true);
        }
    }
}
