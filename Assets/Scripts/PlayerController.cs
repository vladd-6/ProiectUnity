using UnityEngine;

[RequireComponent(typeof(CharacterController))]
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
    public float gravity = -19.62f; // 2 * 9.81 (for snappier jumping)

    private CharacterController controller;

    private bool isGrounded;
    private float verticalInput;
    private float horizontalInput;
    private bool isSprinting;
    private float rotationX = 0;

    private Vector3 playerVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // once per frame
    void Update()
    {
        playerMovement();
        cameraMovement();
    }

    void FixedUpdate()
    {

    }

    void cameraMovement()
    {
        // mouse look up / down
        rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }

        // mouse look left / right
        float rotationY = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * rotationY);
    }

    void playerMovement()
    {
        isGrounded = controller.isGrounded;

        // if grounded, apply negative velocity to stay on ground
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2.0f;
        }

        // get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // check sprint 
        bool isTryingToMove = horizontalInput != 0 || verticalInput != 0;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
        isSprinting = isTryingToMove && isTryingToSprint;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // computing combined direction (normalized for diagonal speed = straight speed)
        Vector3 moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized * currentSpeed;

        // apply set jump force
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerVelocity.y = jumpForce;
        }

        // apply gravity
        playerVelocity.y += gravity * Time.deltaTime;

        // set y-component on movment vector
        moveDirection.y = playerVelocity.y;

        // apply combined movement vector
        controller.Move(moveDirection * Time.deltaTime);
    }
}