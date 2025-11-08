using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace LogicGatesGame.Scripts
{
    public class XRProximityInteractor : XRBaseInteractor
    {
        private List<IXRInteractable> _unsortedValidTargets = new();
        private TriggerContactHelper _triggerContactHelper = new(); 
        private readonly HashSet<Collider> _stayedColliders = new();
        private Coroutine _triggerLateUpdateCoroutine;
        static readonly WaitForFixedUpdate s_WaitForFixedUpdate = new();

        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            if (!isActiveAndEnabled)
                return;
            
            var filter = targetFilter;
            if (filter != null && filter.canProcess)
                filter.Process(this, _unsortedValidTargets, targets);
            else
                SortingHelper.SortByDistanceToInteractor(this, _unsortedValidTargets, targets);
        }

        protected override void Awake()
        {
            base.Awake();
            _triggerContactHelper.interactionManager = interactionManager;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _triggerContactHelper.contactAdded += OnContactAdded;
            _triggerContactHelper.contactRemoved += OnContactRemoved;
            
            ResetCollidersAndValidTargets();
            _triggerLateUpdateCoroutine = StartCoroutine(UpdateCollidersAfterOnTriggerStay());
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            _triggerContactHelper.contactAdded -= OnContactAdded;
            _triggerContactHelper.contactRemoved -= OnContactRemoved;
            
            ResetCollidersAndValidTargets();
            StopCoroutine(_triggerLateUpdateCoroutine);
        }

        protected override void OnRegistered(InteractorRegisteredEventArgs args)
        {
            base.OnRegistered(args);

            args.manager.interactableRegistered += OnInteractableRegistered;
            args.manager.interactableUnregistered += OnInteractableUnregistered;

            _triggerContactHelper.interactionManager = args.manager;
            _triggerContactHelper.ResolveUnassociatedColliders();
            XRToolkitUtils.RemoveAllUnregistered(interactionManager, _unsortedValidTargets);
        }
        
        protected override void OnUnregistered(InteractorUnregisteredEventArgs args)
        {
            base.OnUnregistered(args);
            
            args.manager.interactableRegistered -= OnInteractableRegistered;
            args.manager.interactableUnregistered -= OnInteractableUnregistered;
        }

        private void OnInteractableRegistered(InteractableRegisteredEventArgs args)
        {
            _triggerContactHelper.ResolveUnassociatedColliders(args.interactableObject);
            if (_triggerContactHelper.IsContacting(args.interactableObject) && !_unsortedValidTargets.Contains(args.interactableObject))
                _unsortedValidTargets.Add(args.interactableObject);
        }
        
        private void OnInteractableUnregistered(InteractableUnregisteredEventArgs args)
        {
            _unsortedValidTargets.Remove(args.interactableObject);
        }

        private void OnTriggerEnter(Collider other)
        {
           _triggerContactHelper.AddCollider(other); 
        }
        
        protected void OnTriggerStay(Collider other)
        {
            _stayedColliders.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            _triggerContactHelper.RemoveCollider(other); 
        }

        protected void OnContactAdded(IXRInteractable interactable)
        {
            if (!_unsortedValidTargets.Contains(interactable))
                _unsortedValidTargets.Add(interactable);
        }
        
        protected void OnContactRemoved(IXRInteractable interactable)
        {
            _unsortedValidTargets.Remove(interactable);
        }

        
        IEnumerator UpdateCollidersAfterOnTriggerStay()
        {
            while (true)
            {
                yield return s_WaitForFixedUpdate;

                _triggerContactHelper.UpdateStayedColliders(_stayedColliders);
            }
        }
        
        void ResetCollidersAndValidTargets()
        {
            _unsortedValidTargets.Clear();
            _stayedColliders.Clear();
            _triggerContactHelper.UpdateStayedColliders(_stayedColliders);
        }
    }
}