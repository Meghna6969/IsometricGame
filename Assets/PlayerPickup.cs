using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPosition;
    public float pickupDistance = 3f;

    [Header("Pickup Offset")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Throw Settings")]
    public float throwForce = 10f;

    [Header("UI")]
    public TextMeshProUGUI pickupPromptText;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private Collider heldObjectCollider;
    private InputAction throwAction;
    private InputAction dropAction;

    void OnEnable()
    {
        dropAction = new InputAction(type: InputActionType.Button);
        dropAction.AddBinding("<Keyboard>/q");
        dropAction.AddBinding("<Gamepad>/buttonNorth");
        dropAction.Enable();

        throwAction = new InputAction(type: InputActionType.Button);
        throwAction.AddBinding("<Mouse>/leftButton");
        throwAction.AddBinding("<Gamepad>/rightTrigger");
        throwAction.Enable();
    }
    void OnDisable()
    {
        dropAction?.Disable();
        throwAction?.Disable();
    }

    void Start()
    {
        if (pickupPromptText != null)
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }
    public void ShowPickupPrompt(string message)
    {
        if (pickupPromptText != null && heldObject == null)
        {
            pickupPromptText.text = message;
            pickupPromptText.gameObject.SetActive(true);
        }
    }
    public void HidePickupPrompt()
    {
        pickupPromptText.gameObject.SetActive(false);
    }
    public void PickupObject(GameObject obj)
    {
        if (heldObject != null) return;
        heldObject = obj;
        heldObjectRb = obj.GetComponent<Rigidbody>();

        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = true;
            heldObjectRb.useGravity = false;
        }
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
        }
        obj.transform.SetParent(holdPosition);
        obj.transform.localPosition = positionOffset;
        obj.transform.localRotation = Quaternion.Euler(rotationOffset);

        HidePickupPrompt();
    }
    void Update()
    {
        if (heldObject != null && dropAction.WasPressedThisFrame())
        {
            DropObject();
        }else if (throwAction.WasPressedThisFrame())
        {
            ThrowObject();
        }
    }
    private void DropObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null);
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.useGravity = true;
        }
        Collider objCollider = heldObject.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = true;
        }
        heldObject = null;
        heldObjectRb = null;
        heldObjectCollider = null;
    }
}
