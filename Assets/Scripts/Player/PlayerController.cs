using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    public float moveSpeed = 6.0f;
    public float sprintSpeed = 11.0f;

    [Header("Look Settings")]
    public Camera playerCamera;
    public float mouseSensitivity = 2.0f;
    public float lookXLimit = 85.0f;

    [Header("Jumping Settings")]
    public float jumpForce = 7.0f;
    public float gravity = -19.62f;

    [Header("Dash settings")]
    public float dashImpulse = 60f;
    public float dashDelay = 2f;
    public float dashClock = 0f;

    [Header("Airborne to ledge hang transition settings")]
    public float hangStateTimer = 0f;
    public float delayTime = 1f;

    private CharacterController controller;
    private Wallrun wallRun;
    private Headbob headbob;
    private PlayerSlide slide;
    private DashEffects dashEffects;
    private LedgeDetector ledgeDetector;
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
    
    // Camera lerp for slide transitions
    private bool _hasPendingCameraLerp = false;
    private float _pendingCameraTargetY;
    private float _pendingCameraLerpDuration;
    private float _pendingCameraLerpTimer = 0f;
    private float _pendingCameraStartY;

    // Expose needed values to states
    public CharacterController Controller => controller;
    public Wallrun WallRun => wallRun;
    public LedgeDetector LedgeDetector => ledgeDetector;
    public Vector3 PlayerVelocity { get => playerVelocity; set => playerVelocity = value; }
    public bool IsSprinting { get => isSprinting; set => isSprinting = value; }
    public float HorizontalInput => horizontalInput;
    public float VerticalInput => verticalInput;
    public float MoveSpeed => moveSpeed;
    public float SprintSpeed => sprintSpeed;
    public float JumpForce => jumpForce;
    public float Gravity => gravity;
    public float DashImpulse => dashImpulse;
    public float DashDelay => dashDelay;
    public float DashClock { get => dashClock; set => dashClock = value; }
    public DashEffects DashEffects => dashEffects;
    public float DelayTime => delayTime;
    public float HangStateTimer { get => hangStateTimer; set => hangStateTimer = value; }
    public void SetPendingCameraLerp(float targetY, float duration)
    {
        _hasPendingCameraLerp = true;
        _pendingCameraTargetY = targetY;
        _pendingCameraLerpDuration = duration;
        _pendingCameraLerpTimer = 0f;
        _pendingCameraStartY = playerCamera.transform.localPosition.y;
    }

    public void Start()
    {
        controller = GetComponent<CharacterController>();
        wallRun = GetComponent<Wallrun>();
        headbob = GetComponent<Headbob>();
        ledgeDetector = GetComponent<LedgeDetector>();
        dashEffects = GetComponent<DashEffects>();

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
        dashClock -= Time.deltaTime;
        hangStateTimer -= Time.deltaTime;

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
        if (isGrounded && !(_state is SlideState))
            HandleHeadbob();
        
        HandlePendingCameraLerp();

        // Update wall run lifecycle (may stop wall run affecting next frame transition)
        wallRun.UpdateWallRun(isGrounded, ref currentCameraTilt);

        // Apply movement via controller
        if (controller.enabled)
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
    
    void HandlePendingCameraLerp()
    {
        if (!_hasPendingCameraLerp || _state is SlideState)
            return;
        
        _pendingCameraLerpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_pendingCameraLerpTimer / _pendingCameraLerpDuration);
        
        // Ease out
        t = 1f - (1f - t) * (1f - t);
        
        var cameraPos = playerCamera.transform.localPosition;
        cameraPos.y = Mathf.Lerp(_pendingCameraStartY, _pendingCameraTargetY, t);
        playerCamera.transform.localPosition = cameraPos;
        
        if (_pendingCameraLerpTimer >= _pendingCameraLerpDuration)
        {
            _hasPendingCameraLerp = false;
        }
    }
}