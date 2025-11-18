using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float sprintSpeed = 8.0f;

    [Header("Look Settings")]
    public Camera playerCamera;
    public float mouseSensitivity = 2.0f;
    public float lookXLimit = 85.0f;

    [Header("Headbob Settings")]
    public float walkBobbingSpeed = 14f;
    public float sprintBobbingSpeed = 18f;
    public float bobbingAmount = 0.05f;
    public float cameraYMidpoint = 0f;

    [Header("Jumping Settings")]
    public float jumpForce = 7.0f;
    public float gravity = -19.62f;

    // How quickly the player can change horizontal velocity while airborne (higher = more control)
    public float airControl = 5f;

    private CharacterController controller;
    private Wallrun wallRun;
    public LayerMask ground;

    private bool isGrounded;
    private float verticalInput;
    private float horizontalInput;
    private bool isSprinting;
    private float rotationX = 0;
    private float currentCameraTilt = 0f;
    private float headbobTimer = 0f;
    private Vector3 playerVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        wallRun = GetComponent<Wallrun>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Store initial camera Y position as midpoint
        if (playerCamera != null)
        {
            cameraYMidpoint = playerCamera.transform.localPosition.y;
        }
    }

    void Update()
    {
        wallRun.HandleCameraTilt(ref currentCameraTilt);

        PlayerMovement();
        CameraMovement();
        HandleHeadbob();

        // Update wall run state and camera tilt
        wallRun.UpdateWallRun(isGrounded, ref currentCameraTilt);
    }

    void CameraMovement()
    {
        rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, currentCameraTilt);
        }

        float rotationY = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * rotationY);
    }

    void HandleHeadbob()
    {
        if (playerCamera == null) return;

        // Only headbob when grounded and moving (not wallrunning or in air)
        bool isMoving = horizontalInput != 0 || verticalInput != 0;

        if (isGrounded && isMoving && !wallRun.IsWallRunning)
        {
            // Use different bobbing speed based on sprinting
            float currentBobbingSpeed = isSprinting ? sprintBobbingSpeed : walkBobbingSpeed;

            // Increment timer
            headbobTimer += Time.deltaTime * currentBobbingSpeed;

            // Calculate bobbing offset using sine wave
            float bobbingOffset = Mathf.Sin(headbobTimer) * bobbingAmount;

            // Apply bobbing to camera Y position
            Vector3 newCameraPos = playerCamera.transform.localPosition;
            newCameraPos.y = cameraYMidpoint + bobbingOffset;
            playerCamera.transform.localPosition = newCameraPos;
        }
        else
        {
            // Reset timer when not moving/grounded
            headbobTimer = 0f;

            // Smoothly return camera to midpoint
            Vector3 newCameraPos = playerCamera.transform.localPosition;
            newCameraPos.y = Mathf.Lerp(newCameraPos.y, cameraYMidpoint, Time.deltaTime * 5f);
            playerCamera.transform.localPosition = newCameraPos;
        }
    }

    void PlayerMovement()
    {
        isGrounded = controller.isGrounded;

        // Reset vertical velocity if grounded
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2.0f;
            wallRun.OnGroundedStateChanged(true);
        }

        // Get input (only used when not wall running)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (wallRun.IsWallRunning)
        {
            // AUTOMATIC MOVEMENT - ignore player input during wall run
            wallRun.ApplyWallrunVelocity(ref playerVelocity);
        }
        else
        {
            // Normal movement
            bool isTryingToMove = horizontalInput != 0 || verticalInput != 0;
            bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
            isSprinting = isTryingToMove && isTryingToSprint;

            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            Vector3 moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized * currentSpeed;

            // If grounded, directly set horizontal velocity. If airborne, lerp towards desired horizontal
            // velocity so we don't instantly wipe momentum (preserve inertia from wall-jump).
            if (isGrounded)
            {
                playerVelocity.x = moveDirection.x;
                playerVelocity.z = moveDirection.z;
            }
            else
            {
                float t = airControl * Time.deltaTime;
                playerVelocity.x = Mathf.Lerp(playerVelocity.x, moveDirection.x, t);
                playerVelocity.z = Mathf.Lerp(playerVelocity.z, moveDirection.z, t);
            }
        }

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                playerVelocity.y = jumpForce;
            }
            else if (wallRun.IsWallRunning)
            {
                // Wall jump - preserve horizontal inertia and add outward/upward impulse
                Vector3 horizontalVel = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
                Vector3 outwardDir = Vector3.zero;

                if (wallRun.CurrentWallSide == Wallrun.WallRunSide.Right)
                {
                    outwardDir = -transform.right;
                }
                else if (wallRun.CurrentWallSide == Wallrun.WallRunSide.Left)
                {
                    outwardDir = transform.right;
                }

                const float outwardStrength = 12f;
                playerVelocity = horizontalVel + outwardDir * outwardStrength;
                playerVelocity.y = jumpForce;
                wallRun.StopWallRun();
            }
        }

        // Apply gravity (if not wallrunning)
        if (!wallRun.IsWallRunning)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        // Apply movement
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        wallRun.OnWallHit(hit, ref playerVelocity);
    }
}