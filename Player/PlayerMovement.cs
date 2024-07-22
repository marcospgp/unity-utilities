using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityUtilities
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private float movementSpeed = 5f;

        [SerializeField]
        private float acceleration = 5f;

        [SerializeField]
        private float jumpImpulse = 100f;

        [SerializeField]
        private float mouseSensitivity = 0.5f;

        [Header("References")]
        [SerializeField]
        private InputActionAsset inputActions;

        [SerializeField]
        private Camera playerCamera;

        [SerializeField]
        private CharacterController characterController;

        private InputAction moveInput;
        private InputAction sprintInput;
        private InputAction jumpInput;
        private InputAction lookInput;

        private Vector3 velocity = Vector3.zero;
        private float yLook = 0f;

        public void Start()
        {
            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            this.inputActions.FindActionMap("On foot", throwIfNotFound: true).Enable();

            this.moveInput = this.inputActions.FindAction("Move", throwIfNotFound: true);
            this.sprintInput = this.inputActions.FindAction("Sprint", throwIfNotFound: true);
            this.jumpInput = this.inputActions.FindAction("Jump", throwIfNotFound: true);
            this.lookInput = this.inputActions.FindAction("Look", throwIfNotFound: true);
        }

        public void Update()
        {
            this.ApplyGravity();
            this.Move();
            this.LookAround();
        }

        private void Move()
        {
            if (this.jumpInput.WasPerformedThisFrame())
            {
                this.velocity.y = this.jumpImpulse;
            }

            Vector2 input = this.moveInput.ReadValue<Vector2>().normalized;

            Vector3 worldInput = this.transform.TransformDirection(
                new Vector3(input.x, 0f, input.y)
            );

            // Calculate target XZ velocity.
            Vector2 targetVelocity = this.movementSpeed * new Vector2(worldInput.x, worldInput.z);

            var xzVelocity = new Vector2(this.velocity.x, this.velocity.z);

            Vector2 delta = targetVelocity - xzVelocity;

            Vector2 impulse = this.acceleration * Time.deltaTime * delta.normalized;

            // Avoid pushing velocity past target velocity.
            impulse = Vector2.ClampMagnitude(impulse, delta.magnitude);

            xzVelocity += impulse;

            xzVelocity = Vector2.ClampMagnitude(xzVelocity, this.movementSpeed);

            this.velocity = new Vector3(xzVelocity.x, this.velocity.y, xzVelocity.y);

            _ = this.characterController.Move(this.velocity * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            // Reset gravity if grounded.
            // Ensure player is not trying to jump (which would be indicated by
            // positive Y velocity).
            if (this.characterController.isGrounded && this.velocity.y < 0)
            {
                this.velocity.y = 0;
            }

            this.velocity.y += Physics.gravity.y * Time.deltaTime;
        }

        private void LookAround()
        {
            Vector2 mouseDelta = this.mouseSensitivity * this.lookInput.ReadValue<Vector2>();

            this.transform.Rotate(Vector3.up, mouseDelta.x);

            this.yLook += -1f * mouseDelta.y;
            this.yLook = Mathf.Clamp(this.yLook, -90f, 90f);

            this.playerCamera.transform.localRotation = Quaternion.Euler(this.yLook, 0f, 0f);
        }
    }
}
