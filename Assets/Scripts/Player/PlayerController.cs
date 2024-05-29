using System;
using UnityChan;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintMultiplier = 2.0f;
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
    public bool isSprinting;

    private Rigidbody rigidbody;
    private Transform mainCameraTransform;
    public float originalMoveSpeed;
    private Transform originalParent;

    private CameraController cameraController;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        mainCameraTransform = Camera.main.transform;
        cameraController = mainCameraTransform.GetComponent<CameraController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        originalMoveSpeed = moveSpeed;
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
            CharacterManager.Instance.Player.animator.SetBool("isWalking", true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            CharacterManager.Instance.Player.animator.SetBool("isWalking", false);
            curMovementInput = Vector2.zero;
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded() || _jumpCount < maxJumpCount)
            {
                CharacterManager.Instance.Player.animator.SetTrigger("isJumping");
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
            CharacterManager.Instance.Player.animator.SetBool("isRunning", true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isSprinting = false;
            CharacterManager.Instance.Player.animator.SetBool("isRunning", false);
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    private void Move()
    {
        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * curMovementInput.y + right * curMovementInput.x;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        moveDirection *= moveSpeed;
        moveDirection.y = rigidbody.velocity.y;

        rigidbody.velocity = moveDirection;
        UpdateMoveSpeed();
    }

    void UpdateMoveSpeed()
    {
        if (isSprinting && IsGrounded())
        {
            if (CharacterManager.Instance.Player.condition.UseStamina(15 * Time.deltaTime))
            {
                moveSpeed = originalMoveSpeed * sprintMultiplier;
            }
            else
            {
                isSprinting = false;
                moveSpeed = originalMoveSpeed;
            }
        }
        else if (!isSprinting)
        {
            moveSpeed = originalMoveSpeed;
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
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
        cameraController.canMove = !toggle;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            originalParent = transform.parent;
            transform.SetParent(collision.transform);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(originalParent);
        }
    }
}