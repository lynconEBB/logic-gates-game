using UnityEngine;
using UnityEngine.Serialization;
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