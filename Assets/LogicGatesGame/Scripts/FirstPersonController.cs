using UnityEngine;
using UnityEngine.InputSystem;

namespace LogicGatesGame.Scripts
{
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;

        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookXAction;
        [SerializeField] private InputActionReference lookYAction;

        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float lookSensitivity = 0.1f;
        [SerializeField] private Transform cameraTransform;

        private float pitch;

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            Vector2 move = moveAction.action.ReadValue<Vector2>();
            Vector3 motion = (transform.right * move.x + transform.forward * move.y) * moveSpeed;
            characterController.SimpleMove(motion);

            float lookX = lookXAction.action.ReadValue<float>() * lookSensitivity;
            float lookY = lookYAction.action.ReadValue<float>() * lookSensitivity;
            
            transform.Rotate(0f, lookX, 0f);

            if (cameraTransform != null)
            {
                pitch = Mathf.Clamp(pitch - lookY, -89f, 89f);
                cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
            }
        }
    }
}
