using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_3 = new(0.3f);

    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float sprintSpeed = 8.0f;

    [Header("Look Settings")]
    public Camera playerCamera;
    public float mouseSensitivity = 2.0f;
    public float lookXLimit = 85.0f;

    [Header("Jumping Settings")]
    public float jumpForce = 7.0f;
    public float gravity = -19.62f;

    // How quickly the player can change horizontal velocity while airborne (higher = more control)
    public float airControl = 5f;

    [Header("Wall Running Settings")]
    public float wallRunSpeed = 10f;
    public float wallRunUpwardForce = 3f;
    public float wallRunCameraTilt = 40f;
    public float wallRunTiltSpeed = 5f;
    public float minWallRunSpeed = 4f;
    public float wallRunDuration = 2f;

    private CharacterController controller;
    public LayerMask ground;

    private bool isGrounded;
    private float verticalInput;
    private float horizontalInput;
    private bool isSprinting;
    private float rotationX = 0;
    private float currentCameraTilt = 0f;

    private bool canWallRun = true;
    private bool isWallRunning = false;
    private Vector3 playerVelocity;
    private WallRunSide currentWallSide = WallRunSide.None;
    private float horizontalSpeed;
    private Vector3 wallRunDirection;
    private float wallRunTimer;
    // Remember last wall we ran on so we don't immediately re-start on the same wall
    private GameObject lastWallObject = null;
    // Set to true when player touches ground since the last wall run
    private bool touchedGroundSinceWallRun = true;

    private enum WallRunSide
    {
        None,
        Left,
        Right
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        PlayerMovement();
        CameraMovement();
        HandleCameraTilt();

        // Compute horizontal speed
        horizontalSpeed = new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude;

        if (isWallRunning)
        {
            // Update wall run timer
            wallRunTimer -= Time.deltaTime;

            // Stop wallrunning if timer expires, no wall contact, or grounded
            if (wallRunTimer <= 0 || !IsTouchingWall() || isGrounded)
            {
                StopWallRun();
            }
        }
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

    void HandleCameraTilt()
    {
        float targetTilt = 0f;

        if (isWallRunning)
        {
            switch (currentWallSide)
            {
                case WallRunSide.Left:
                    targetTilt = -wallRunCameraTilt;
                    break;
                case WallRunSide.Right:
                    targetTilt = wallRunCameraTilt;
                    break;
            }
        }

        currentCameraTilt = Mathf.Lerp(currentCameraTilt, targetTilt, wallRunTiltSpeed * Time.deltaTime);
    }

    void PlayerMovement()
    {
        isGrounded = controller.isGrounded;

        // Reset vertical velocity if grounded
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2.0f;

            // Stop wall running if we touch ground
            if (isWallRunning)
            {
                StopWallRun();
            }
            // mark we've touched ground (allows re-using same wall later)
            touchedGroundSinceWallRun = true;
        }

        // Get input (only used when not wall running)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (isWallRunning)
        {
            // AUTOMATIC MOVEMENT - ignore player input during wall run
            // Move automatically along the wall
            playerVelocity = wallRunDirection * wallRunSpeed;
            playerVelocity.y = Mathf.MoveTowards(playerVelocity.y, -2f, Time.deltaTime * 3f);
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
            else if (isWallRunning)
            {
                // Wall jump - preserve horizontal inertia and add outward/upward impulse
                Vector3 horizontalVel = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
                Vector3 outwardDir = Vector3.zero;

                if (currentWallSide == WallRunSide.Right)
                {
                    outwardDir = -transform.right;
                }
                else if (currentWallSide == WallRunSide.Left)
                {
                    outwardDir = transform.right;
                }

                const float outwardStrength = 12f;
                playerVelocity = horizontalVel + outwardDir * outwardStrength;
                playerVelocity.y = jumpForce;
                StopWallRun();
            }
        }

        // Apply gravity (if not wallrunning)
        if (!isWallRunning)
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }

        // Apply movement
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if we hit something in our ground layer mask
        bool isInGroundLayer = ((1 << hit.gameObject.layer) & ground) != 0;

        if (isInGroundLayer &&
            Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f && // It's a wall (not ground)
            canWallRun &&
            !isWallRunning &&
            !isGrounded)
        {
            // Check if we have sufficient horizontal speed to start wall running
            Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            float currentHorizontalSpeed = horizontalVelocity.magnitude;

            if (currentHorizontalSpeed >= minWallRunSpeed)
            {
                // Prevent starting a wall run on the same wall unless we've touched ground
                if (!touchedGroundSinceWallRun && lastWallObject == hit.gameObject)
                {
                    return;
                }
                StartWallRun(hit);
            }
        }
    }

    private void StartWallRun(ControllerColliderHit hit)
    {
        // Determine which wall we're hitting
        Vector3 localHitNormal = transform.InverseTransformDirection(hit.normal);

        if (localHitNormal.x > 0.5f) // Wall on our right
        {
            currentWallSide = WallRunSide.Left;
            wallRunDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;
        }
        else if (localHitNormal.x < -0.5f) // Wall on our left
        {
            currentWallSide = WallRunSide.Right;
            wallRunDirection = Vector3.Cross(Vector3.up, hit.normal).normalized;
        }
        else
        {
            // Front wall - run upward
            currentWallSide = WallRunSide.None;
            wallRunDirection = Vector3.up * 0.3f + transform.forward * 0.7f;
        }

        // Start wall running
        isWallRunning = true;
        wallRunTimer = wallRunDuration;

        // Give initial upward boost
        playerVelocity.y = wallRunUpwardForce;

        // Record which wall we started on and clear the 'touched ground' flag
        lastWallObject = hit.gameObject;
        touchedGroundSinceWallRun = false;
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        currentWallSide = WallRunSide.None;
        StartCoroutine(WallRunCooldown());
    }

    private bool IsTouchingWall()
    {
        float checkDistance = 1.1f;

        if (currentWallSide == WallRunSide.Left)
        {
            return Physics.Raycast(transform.position, -transform.right, checkDistance, ground);
        }
        else if (currentWallSide == WallRunSide.Right)
        {
            return Physics.Raycast(transform.position, transform.right, checkDistance, ground);
        }

        return false;
    }

    private IEnumerator WallRunCooldown()
    {
        canWallRun = false;
        yield return _waitForSeconds0_3;
        canWallRun = true;
    }

    // Public properties
    public float HorizontalSpeed => horizontalSpeed;
    public bool IsWallRunning => isWallRunning;
}