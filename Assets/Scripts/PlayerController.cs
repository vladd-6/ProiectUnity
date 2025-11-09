using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float sprintSpeed = 8.0f;

    [Header("Look Settings")]
    public Camera playerCamera; // attach main camera in inspector
    public float mouseSensitivity = 2.0f;
    public float lookXLimit = 85.0f; // limit for up/down looking

    [Header("Jumping Settings")]
    public float jumpForce = 7.0f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.6f;

    [Header("Stamina Settings")]
    public float maxStamina = 100.0f;
    public float staminaDrainRate = 25.0f;
    public float staminaRegenRate = 15.0f;

    private Rigidbody rb;
    private bool isGrounded;
    private float verticalInput;
    private float horizontalInput;
    private bool isSprinting;
    private float currentStamina;
    private float rotationX = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 0.1f; // drag

        currentStamina = maxStamina;

        // lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // computing ground distance (1/2 cylinder height + margin)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            groundCheckDistance = col.bounds.extents.y + 0.1f;
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // check if player is on ground, then jump
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // stamina system (wip)
        bool isTryingToMove = horizontalInput != 0 || verticalInput != 0;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);

        if (isTryingToMove && isTryingToSprint && currentStamina > 0)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime; // drain with set rate
        }
        else
        {
            isSprinting = false;
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime; // regen with set rate
            }
        }
        // clamp stamina ( 0 <= stamina <= max )
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // mouse look up / down
        rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // computing combined direction (normalized for diagonal speed = straight speed)
        Vector3 moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized * currentSpeed;

        rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);

        // mouse look left / right
        float rotationY = Input.GetAxis("Mouse X") * mouseSensitivity;
        Quaternion deltaRotation = Quaternion.Euler(0f, rotationY, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}