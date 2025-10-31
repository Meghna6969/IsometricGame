using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

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
    public float throwForceMultiplier = 1.5f;
    public float ignoreCollisionTime = 0.5f;

    [Header("Trajectory Visulization")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public GameObject targetIndicator;
    public Transform throwStartPoint;

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

        PickupObject pickupObj = obj.GetComponent<PickupObject>();
        if(pickupObj != null)
        {
            pickupObj.OnPickedUp();
        }

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
        if (heldObject == null) return;

        isAiming = true;

        trajectoryLine.enabled = true;
        targetIndicator.SetActive(true);
    }
    private void UpdateAiming()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            Vector3 groundPoint = hit.point;

            Vector3 flatDirection = new Vector3(groundPoint.x - transform.position.x, 0, groundPoint.z - transform.position.z);
            float distance = flatDirection.magnitude;

            if (distance > maxThrowDistance)
            {
                flatDirection = flatDirection.normalized * maxThrowDistance;
                groundPoint = new Vector3(transform.position.x + flatDirection.x, hit.point.y, transform.position.z + flatDirection.z);
            }

            targetPosition = groundPoint;
            if (targetIndicator != null)
            {
                targetIndicator.transform.position = targetPosition + Vector3.up * 0.1f;
            }
            Vector3 lineStartPos = GetLineStartPosition();
            DrawTrajectory(lineStartPos, targetPosition);

        }
    }
    private Vector3 GetLineStartPosition()
    {
        if (throwStartPoint != null)
        {
            return throwStartPoint.position;
        }
        return transform.position;
    }
    private void DrawTrajectory(Vector3 start, Vector3 end)
    {
        if (trajectoryLine == null) return;
        
        trajectoryLine.positionCount = trajectoryResolution;
        float heightOffset = start.y - end.y;
        Debug.Log($"Start: {start}, End: {end}");
        for (int i = 0; i < trajectoryResolution; i++)
        {
            float t = i / (float)(trajectoryResolution - 1);
            Vector3 point = CalculateArcPoint(start, end, t, heightOffset);
            trajectoryLine.SetPosition(i, point);
        }
    }
    private Vector3 CalculateArcPoint(Vector3 start, Vector3 end, float t, float heightOffset)
    {
        float x = Mathf.Lerp(start.x, end.x, t);
        float z = Mathf.Lerp(start.z, end.z, t);

        float baseY = Mathf.Lerp(start.y, end.y, t);
        float arc = arcHeight * Mathf.Sin(t * Mathf.PI);

        float y = baseY + arc;

        return new Vector3(x, y, z);
    }
    private Vector3 CalculateThrowVelocity(Vector3 start, Vector3 target)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        Vector3 direction = target - start;
        float horizontalDistance = new Vector3(direction.x, 0, direction.z).magnitude;
        float verticalDistance = direction.y;

        float time = Mathf.Sqrt((2 * arcHeight) / gravity) + Mathf.Sqrt(Mathf.Max(0, (2 * (arcHeight - verticalDistance)) / gravity));

       if (time <= 0) time = 1f;

        Vector3 velocity = new Vector3(direction.x, 0, direction.z) / time;
        velocity.y = (verticalDistance / time) + (0.5f * gravity * time);

        return velocity * throwForceMultiplier;
    }
    private void DropObject()
    {
        if (heldObject == null) return;

        PickupObject pickupObj = heldObject.GetComponent<PickupObject>();
        if(pickupObj != null)
        {
            pickupObj.OnDropped();
        }
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

        isAiming = false;

        trajectoryLine.enabled = false;
        targetIndicator.SetActive(false);

        Vector3 throwStartPos = heldObject.transform.position;
        heldObject.transform.SetParent(null);

        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.useGravity = true;

            Vector3 throwVelocity = CalculateThrowVelocity(throwStartPos, targetPosition);
            heldObjectRb.linearVelocity = throwVelocity;
        }
        heldPhysicsCollider.enabled = true;
        heldTriggerCollider.enabled = true;

        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null && heldPhysicsCollider != null)
        {
            Physics.IgnoreCollision(heldPhysicsCollider, playerCollider, true);

            Collider objectCol = heldPhysicsCollider;
            StartCoroutine(ReenableCollisionAfterDelay(objectCol, playerCollider));
        }


        heldObject = null;
        heldObjectRb = null;
        heldPhysicsCollider = null;
        heldTriggerCollider = null;
    }
    private IEnumerator ReenableCollisionAfterDelay(Collider objectCol, Collider playerCol)
    {
        yield return new WaitForSeconds(ignoreCollisionTime);
        if(objectCol != null && playerCol != null)
        {
            Physics.IgnoreCollision(objectCol, playerCol, false);
        }
    }
}
