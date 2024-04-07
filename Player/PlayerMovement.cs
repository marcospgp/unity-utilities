using UnityEngine;
using UnityEngine.InputSystem;
using UnityUtilities;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float movementSpeed = 0.5f;

    [SerializeField]
    [Tooltip("Time it takes to move halfway towards target speed.")]
    private float acceleration = 0.1f;

    [SerializeField]
    [Range(0f, 2f)]
    private float mouseSensitivity = 0.5f;

    [SerializeField]
    private bool lockCursor = true;

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
    private const float MIN_SPEED = 0.5f;

    private void Start()
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

    private void Update()
    {
        // if (this.jumpInput.WasPerformedThisFrame()) {
        // }

        this.Move();

        this.LookAround();
    }

    private void Move()
    {
        this.ApplyGravity();

        Vector2 input = this.moveInput.ReadValue<Vector2>();

        Vector3 worldInput = this.transform.TransformVector(new Vector3(input.x, 0f, input.y));

        Vector3 targetVelocity = this.movementSpeed * 10f * worldInput;

        Vector2 xzVelocity = FILerp.Get(
            new Vector2(this.velocity.x, this.velocity.z),
            new Vector2(targetVelocity.x, targetVelocity.z),
            Time.deltaTime,
            this.acceleration
        );

        if (xzVelocity.magnitude < MIN_SPEED)
        {
            xzVelocity = Vector2.zero;
        }

        this.velocity.x = xzVelocity.x;
        this.velocity.z = xzVelocity.y;

        _ = this.characterController.SimpleMove(this.velocity);
    }

    private void ApplyGravity()
    {
        if (this.characterController.isGrounded)
        {
            // Reset gravity if grounded
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
