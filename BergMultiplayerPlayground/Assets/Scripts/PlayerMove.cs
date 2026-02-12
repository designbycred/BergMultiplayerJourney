using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Animator animator;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Jump / Gravity")]
    [SerializeField] private float jumpHeight = 1.8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedStick = -0.5f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standCameraHeight = 1.6f;
    [SerializeField] private float crouchCameraHeight = 1.0f;

    private CharacterController cc;

    private Vector2 moveInput;
    private bool sprintHeld;
    private bool crouchHeld;
    private bool jumpPressed;

    private float verticalVelocity;
    private float targetCameraHeight;

    private float originalHeight;
    private Vector3 originalCenter;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (!cameraTransform) cameraTransform = Camera.main.transform;
        if (!animator) animator = GetComponentInChildren<Animator>();

        originalHeight = cc.height;
        originalCenter = cc.center;

        targetCameraHeight = standCameraHeight;
    }

    // --- INPUT CALLBACKS ---

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        sprintHeld = ctx.ReadValueAsButton();
        if (animator) animator.SetBool("Sprint", sprintHeld);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) jumpPressed = true;
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        crouchHeld = ctx.ReadValueAsButton();

        if (animator)
            animator.SetBool("Crouch", crouchHeld);

        if (crouchHeld)
        {
            cc.height = crouchHeight;
            cc.center = new Vector3(0, crouchHeight * 0.5f, 0);
            targetCameraHeight = crouchCameraHeight;
        }
        else
        {
            cc.height = originalHeight;
            cc.center = originalCenter;
            targetCameraHeight = standCameraHeight;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleCameraHeight();
    }

    private void HandleMovement()
    {
        bool grounded = cc.isGrounded;

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;

        if (jumpPressed && grounded)
        {
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);
            if (animator) animator.SetTrigger("Jump");
        }

        jumpPressed = false;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDir = (right * moveInput.x + forward * moveInput.y);
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        float speed = walkSpeed;
        if (crouchHeld)
            speed = crouchSpeed;
        else if (sprintHeld)
            speed = runSpeed;

        Vector3 totalMove = (moveDir * speed) + (Vector3.up * verticalVelocity);
        cc.Move(totalMove * Time.deltaTime);

        grounded = cc.isGrounded;

        if (animator)
        {
            animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
            animator.SetFloat("MoveY", moveInput.y, 0.1f, Time.deltaTime);
            animator.SetBool("Grounded", grounded);
            animator.SetFloat("YVel", verticalVelocity);
        }
    }

    private void HandleCameraHeight()
    {
        if (!cameraPivot) return;

        Vector3 pos = cameraPivot.localPosition;
        pos.y = Mathf.Lerp(pos.y, targetCameraHeight, 10f * Time.deltaTime);
        cameraPivot.localPosition = pos;
    }
}
