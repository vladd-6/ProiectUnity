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

    [Header("Jumping Settings")]
    public float jumpForce = 7.0f;
    public float gravity = -19.62f;

    // How quickly the player can change horizontal velocity while airborne (higher = more control)
    public float airControl = 5f;

    private CharacterController controller;
    private Wallrun wallRun;
    private Headbob headbob;
    public LayerMask ground;

    private bool isGrounded;
    private float verticalInput;
    private float horizontalInput;
    private bool isSprinting;
    private float rotationX = 0;
    private float currentCameraTilt = 0f;
    private Vector3 playerVelocity;

    // State machine
    private IMovementState _state;

    // Expose needed values to states
    public CharacterController Controller => controller;
    public Wallrun WallRun => wallRun;
    public Vector3 PlayerVelocity { get => playerVelocity; set => playerVelocity = value; }
    public bool IsSprinting { get => isSprinting; set => isSprinting = value; }
    public float HorizontalInput => horizontalInput;
    public float VerticalInput => verticalInput;
    public float MoveSpeed => moveSpeed;
    public float SprintSpeed => sprintSpeed;
    public float JumpForce => jumpForce;
    public float Gravity => gravity;
    public float AirControl => airControl;

    public void Start()
    {
        controller = GetComponent<CharacterController>();
        wallRun = GetComponent<Wallrun>();
        headbob = GetComponent<Headbob>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Store initial camera Y position as midpoint
        if (playerCamera != null)
        {
            headbob.SetCameraYMidpoint(playerCamera.transform.localPosition.y);
        }

        // Initialize state based on grounded status at start
        isGrounded = controller.isGrounded;
        _state = isGrounded ? new GroundedState() as IMovementState : new AirborneState();
        _state.OnEnter(this);
    }

    public void Update()
    {
        isGrounded = controller.isGrounded;

        // Camera tilt from wallrun helper
        wallRun.HandleCameraTilt(ref currentCameraTilt);

        // State machine processing
        _state.HandleInput(this);
        _state.Tick(this);
        var next = _state.TryTransition(this);
        if (next != null)
        {
            _state.OnExit(this);
            _state = next;
            _state.OnEnter(this);
        }

        CameraMovement();
        HandleHeadbob();

        // Update wall run lifecycle (may stop wall run affecting next frame transition)
        wallRun.UpdateWallRun(isGrounded, ref currentCameraTilt);

        // Apply movement via controller
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void HandleHeadbob()
    {
        var cameraPos = playerCamera.transform.localPosition;
        headbob.ApplyCameraTransform(ref cameraPos, new Headbob.Params
        {
            HorizontalInput = horizontalInput,
            VerticalInput = verticalInput,
            IsGrounded = isGrounded,
            IsWallRunning = wallRun.IsWallRunning,
            IsSprinting = isSprinting,
        });
        playerCamera.transform.localPosition = cameraPos;
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

    public void ReadMovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        wallRun.OnWallHit(hit, ref playerVelocity);
    }
}