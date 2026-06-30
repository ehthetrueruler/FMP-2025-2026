using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HoldJumpMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float holdJumpHeight = 4f; // Twice as high
    public float gravity = -20f;
    public float maxHoldTime = 0.25f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;

    private bool isGrounded;
    private bool isHoldingJump;

    private float jumpHoldTimer;

    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GroundCheck();
        Move();
        MouseLook();
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;

            // Reset jump state when landing
            isHoldingJump = false;
            jumpHoldTimer = 0f;
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = Input.GetKey(KeyCode.LeftShift)
            ? sprintSpeed
            : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        // Start jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            isHoldingJump = true;
            jumpHoldTimer = 0f;
        }

        // Hold jump for extra height
        if (Input.GetButton("Jump") && isHoldingJump)
        {
            jumpHoldTimer += Time.deltaTime;

            if (jumpHoldTimer < maxHoldTime)
            {
                // Adds upward force while holding
                velocity.y += 25f * Time.deltaTime;
            }
        }

        // Stop boosting when released
        if (Input.GetButtonUp("Jump"))
        {
            isHoldingJump = false;
        }

        // Cap maximum jump height
        float maxJumpVelocity =
            Mathf.Sqrt(holdJumpHeight * -2f * gravity);

        velocity.y = Mathf.Min(velocity.y, maxJumpVelocity);

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") *
                       mouseSensitivity *
                       Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") *
                       mouseSensitivity *
                       Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundDistance
        );
    }
}