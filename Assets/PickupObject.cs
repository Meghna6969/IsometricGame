using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : MonoBehaviour
{
    [Header("Pickup Prompt")]
    public string pickupPrompt = "Press E to pick up";

    public Collider triggerCollider;
    public Collider physicsCollider;

    [Header("Damage Settings")]
    public float throwDamage = 50f;
    public float damageVelocityThreshold = 2f;

    private bool isInRange = false;
    private PlayerPickup playerPickup;
    private InputAction pickupAction;

    private Rigidbody rb;
    private bool isBeingHeld = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        pickupAction = new InputAction(type: InputActionType.Button);
        pickupAction.AddBinding("<Keyboard>/e");
        pickupAction.AddBinding("<Gamepad>/buttonWest");
        pickupAction.Enable();
    }
    
    void OnDisable()
    {
        pickupAction?.Disable();
    }

    void Update()
    {
        if (isInRange && playerPickup != null && pickupAction.WasPressedThisFrame())
        {
            playerPickup.PickupObject(gameObject, physicsCollider, triggerCollider);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            playerPickup = other.GetComponent<PlayerPickup>();
            if (playerPickup != null)
            {
                playerPickup.ShowPickupPrompt(pickupPrompt);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            if (playerPickup != null)
            {
                playerPickup.HidePickupPrompt();
                playerPickup = null;
            }
        }
    }

    public void OnPickedUp()
    {
        isBeingHeld = true;
    }

    public void OnThrown()
    {
        isBeingHeld = false;
    }

    public void OnDropped()
    {
        isBeingHeld = false;
    }
}