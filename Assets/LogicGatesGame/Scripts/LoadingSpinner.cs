using UnityEngine;

namespace LogicGatesGame.Scripts
{
    /// <summary>
    /// Sits on the loading page and rotates only the assigned spinner graphic.
    /// The page can hold other elements (e.g. a "Loading..." label) that stay
    /// still. Update only runs while the page GameObject is active, so the spin
    /// starts and stops simply by toggling the page on/off.
    /// </summary>
    public class LoadingSpinner : MonoBehaviour
    {
        [SerializeField] private RectTransform spinner;
        [SerializeField] private float rotationSpeed = 180f;

        private void Reset()
        {
            spinner = transform as RectTransform;
        }

        private void Update()
        {
            if (spinner != null)
                spinner.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
    }
}
