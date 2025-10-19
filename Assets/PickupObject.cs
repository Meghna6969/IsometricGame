using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : MonoBehaviour
{
    [Header("Pickup Prompt")]
    public string pickupPrompt = "Press E to pick up";

    private bool isInRange = false;
    private PlayerPickup playerPickup;
    private InputAction pickupAction;

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
    void Update()
    {
        if(isInRange && playerPickup != null && pickupAction.WasPressedThisFrame())
        {
            playerPickup.PickupObject(gameObject);
        }
    }
}
