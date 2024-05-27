using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Vector2 curMovementInput;
    public float jumpForce;
    public LayerMask groundLayerMask;
    public int maxJumpCount;

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;
    public bool canLook = true;

    public Action inventory;
    private Vector2 mouseDelta;


    [HideInInspector]
    private int _jumpCount;
    private bool isSprinting;

    private Rigidbody rigidbody;
    public PlayerCondition condition;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        condition = CharacterManager.Instance.Player.condition;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded() || _jumpCount < maxJumpCount)
            {
                Vector3 velocity = rigidbody.velocity;
                velocity.y = 0;
                rigidbody.velocity = velocity;
                rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _jumpCount++;
            }
        }
    }

    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            isSprinting = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isSprinting = false;
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    private void Move()
    {
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= isSprinting && IsGrounded()? moveSpeed * 2 : moveSpeed;
        dir.y = rigidbody.velocity.y;

        rigidbody.velocity = dir;

        if (isSprinting)
        {
            condition.uiCondition.stamina.Subtract(20 * Time.deltaTime);
        }
    }

    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
        new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.5f), Vector3.down),
        new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.5f), Vector3.down),
        new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.5f), Vector3.down),
        new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.5f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.6f, groundLayerMask))
            {
                _jumpCount = 0;
                return true;
            }
        }

        return false;
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }
}