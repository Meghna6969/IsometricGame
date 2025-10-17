using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.8f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;
    private InputAction moveAction;
    private InputAction jumpAction;

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
    }
    void OnDisable()
    {
        moveAction?.Disable();
        if(jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }
    }
    void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            Debug.Log("JUMP WORKING");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            Debug.Log("Jump Failed");
        }
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        Vector2 input = moveAction.ReadValue<Vector2>();
        float x = input.x;
        float z = input.y;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
