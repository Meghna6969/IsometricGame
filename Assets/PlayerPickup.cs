using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPosition;
    public float pickupDistance = 3f;

    [Header("Pickup Offset")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Throw Settings")]
    public float maxThrowDistance = 10f;
    public LayerMask groundLayer;
    public float arcHeight = 2f;

    [Header("Trajectory Visulization")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public GameObject targetIndicator;

    [Header("UI")]
    public TextMeshProUGUI pickupPromptText;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private Collider heldPhysicsCollider;
    private Collider heldTriggerCollider;
    private InputAction throwAction;
    private InputAction dropAction;
    private Camera mainCamera;

    private bool isAiming = false;
    private Vector3 targetPosition;

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
        mainCamera = Camera.main;
        pickupPromptText.gameObject.SetActive(false);
        trajectoryLine.enabled = false;
        targetIndicator.SetActive(false);
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
    public void PickupObject(GameObject obj, Collider physicsCollider, Collider triggerCollider)
    {
        if (heldObject != null) return;
        heldObject = obj;
        heldObjectRb = obj.GetComponent<Rigidbody>();
        heldPhysicsCollider = physicsCollider;
        heldTriggerCollider = triggerCollider;

        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = true;
            heldObjectRb.useGravity = false;
        }

        heldPhysicsCollider.enabled = false;
        heldTriggerCollider.enabled = false;

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
        }
        else if (throwAction.WasPressedThisFrame())
        {
            StartAiming();
        }
        else if (throwAction.IsPressed() && isAiming)
        {
            UpdateAiming();

        }
        else if (isAiming && !throwAction.IsPressed())
        {
            ThrowObject();
        }
    }
    private void StartAiming()
    {
        isAiming = true;

        trajectoryLine.enabled = true;
        targetIndicator.SetActive(true);
    }
    private void UpdateAiming()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 clampedPosition = hit.point;
            float distance = Vector3.Distance(transform.position, hit.point);

            if (distance > maxThrowDistance)
            {
                Vector3 direction = (hit.point - transform.position).normalized;
                clampedPosition = transform.position + direction * maxThrowDistance;
                clampedPosition.y = hit.point.y;
            }
            targetPosition = clampedPosition;
            targetIndicator.transform.position = targetPosition;
            DrawTrajectory(heldObject.transform.position, targetPosition);
        }
    }
    private void DrawTrajectory(Vector3 start, Vector3 end)
    {
        if (trajectoryLine == null) return;
        trajectoryLine.positionCount = trajectoryResolution;
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = i / (float)(trajectoryResolution - 1);
            Vector3 point = CalculateArcPoint(start, end, t);
            trajectoryLine.SetPosition(i, point);
        }
    }
    private Vector3 CalculateArcPoint(Vector3 start, Vector3 end, float t)
    {
        Vector3 midPoint = Vector3.Lerp(start, end, t);
        float arc = arcHeight * Mathf.Sin(t * Mathf.PI);
        midPoint.y += arc;
        return midPoint;
    }
    private Vector3 CalculateThrowVelocity(Vector3 start, Vector3 target)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        return Vector3.zero; // Placeholder for future implementation 
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

        if (heldPhysicsCollider != null)
        {
            heldPhysicsCollider.enabled = true;
        }
        if (heldTriggerCollider != null)
        {
            heldTriggerCollider.enabled = true;
        }
        heldObject = null;
        heldObjectRb = null;
        heldPhysicsCollider = null;
        heldTriggerCollider = null;
    }
    private void ThrowObject()
    {
        if (heldObject == null) return;
        heldObject.transform.SetParent(null);
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.useGravity = true;

            Vector3 throwDirection = transform.forward;
            heldObjectRb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
        heldPhysicsCollider.enabled = true;
        heldTriggerCollider.enabled = true;
        heldObject = null;
        heldObjectRb = null;
        heldPhysicsCollider = null;
        heldTriggerCollider = null;
    }
}
