using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float speed = 12f;
    public float sprintSpeed = 20f;
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;
    public float rotationSmoothness = 0.1f;

    [Header("Jump Settings")]
    public float jumpHeight = 3f;
    public float gravity = -19.6f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Animation Settings")]
    public Animator animator;
    public float animationSmoothTime = 0.1f;

    private Vector3 velocity;
    private Vector3 currentVelocity;
    private float currentSpeed;
    private float velocityY;
    private bool isGrounded;
    public float animationVelocity;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    void OnEnable()
    {
        moveAction = new InputAction(type: InputActionType.Value, binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.Enable();

        jumpAction = new InputAction(type: InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");
        jumpAction.performed += OnJump;
        jumpAction.Enable();

        sprintAction = new InputAction(type: InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.AddBinding("<Keyboard>/rightShift");
        sprintAction.AddBinding("<Gamepad>/leftTrigger");
        sprintAction.Enable();
    }
    void OnDisable()
    {
        moveAction?.Disable();
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }
        sprintAction?.Disable();
    }
    void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            Debug.Log("JUMP WORKING");
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if(animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
        else
        {
            Debug.Log("Jump Failed");
        }
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleRotation();
        HandleGravity();
        UpdateAnimator();
    }
    private void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;
        if(animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }
        if (isGrounded && velocityY < 0)
        {
            velocityY = -2f;
        }
    }
    private void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.IsPressed();
        float targetSpeed = isSprinting ? sprintSpeed : speed;

        if (input.magnitude > 0.1f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.deltaTime);
        }
        Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;

        Vector3 targetVelocity = moveDirection * currentSpeed;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

        controller.Move(currentVelocity * Time.deltaTime);
    }
    private void HandleRotation()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        if (input.magnitude > 0.1f)
        {
            Vector3 inputDirection = new Vector3(input.x, 0f, input.y).normalized;
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;

            float currentAngle = transform.eulerAngles.y;
            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSmoothness);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }
    }
    private void HandleGravity()
    {
        velocityY += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocityY, 0f) * Time.deltaTime);
    }
    private void UpdateAnimator()
    {
        if (animator == null) return;
        bool isSprinting = sprintAction.IsPressed();
        float targetAnimVelocity;

        if (currentSpeed < 0.1f)
        {
            targetAnimVelocity = 0f;
        }
        else if (isSprinting)
        {
            float sprintRatio = Mathf.Clamp01(currentSpeed / sprintSpeed);
            targetAnimVelocity = Mathf.Lerp(0.5f, 1f, sprintRatio);
        }
        else
        {
            if (currentSpeed > speed)
            {
                float decelerationRatio = Mathf.Clamp01(currentSpeed / sprintSpeed);
                targetAnimVelocity = Mathf.Lerp(0.5f, 1f, decelerationRatio);
            }
            else
            {
                float walkRatio = Mathf.Clamp01(currentSpeed / speed);
                targetAnimVelocity = walkRatio * 0.5f;
            }
        }
        animationVelocity = Mathf.Lerp(animationVelocity, targetAnimVelocity, Time.deltaTime / animationSmoothTime);

        animator.SetFloat("Velocity", animationVelocity);
    }
    
    
}
