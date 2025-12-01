using UnityEngine;

public class AirborneState : IMovementState
{
    private bool _preserveMomentum = false;
    private Vector3 _initialHorizontalVelocity;
    private float _momentumDecayTime = 60f;
    private float _momentumTimer = 0f;
    private bool _slideTriggered = false;
    private bool _dashTriggered = false;
    private float _airControl = 5f;
    private bool _doubleJumpUsed = false;
    public void OnEnter(PlayerController player)
    {
        _dashTriggered = false;
        // Preserve horizontal velocity if coming from a high-speed state like slide
        Vector3 horizontalVel = new Vector3(player.PlayerVelocity.x, 0, player.PlayerVelocity.z);
        float horizontalSpeed = horizontalVel.magnitude;
        
        // If horizontal speed is significantly higher than sprint speed, preserve momentum
        if (horizontalSpeed > player.SprintSpeed * 1.5f)
        {
            _preserveMomentum = true;
            _initialHorizontalVelocity = horizontalVel;
            _momentumTimer = 0f;
        }
        else
        {
            _preserveMomentum = false;
        }
    }
    public void OnExit(PlayerController player) {}

    public void HandleInput(PlayerController player)
    {
        _slideTriggered = Input.GetKey(KeyCode.LeftControl);

        player.ReadMovementInput();
        bool isTryingToMove = player.HorizontalInput != 0 || player.VerticalInput != 0;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
        player.IsSprinting = isTryingToMove && isTryingToSprint; // Sprint only affects horizontal air control speed
        
        if (Input.GetKeyDown(KeyCode.Space) && !_doubleJumpUsed)
        {
            player.PlayerVelocity = new Vector3(player.PlayerVelocity.x, player.JumpForce, player.PlayerVelocity.z);
            _doubleJumpUsed = true;
        }
        
        if (Input.GetMouseButtonDown((int)UnityEngine.UIElements.MouseButton.RightMouse) && player.DashClock <= 0f)
        {
            _dashTriggered = true;
            player.DashClock = player.DashDelay;
        }
    }

    public void Tick(PlayerController player)
    {
        Vector3 hv;
        
        // TODO: airControl does not work when preserving momentum
        // It's more realistic but maybe not what we want
        if (_preserveMomentum)
        {
            _momentumTimer += Time.deltaTime;
            float decayFactor = 1f - Mathf.Clamp01(_momentumTimer / _momentumDecayTime);
            
            float targetSpeed = player.IsSprinting ? player.SprintSpeed : player.MoveSpeed;
            Vector3 desired = (player.transform.forward * player.VerticalInput + player.transform.right * player.HorizontalInput).normalized * targetSpeed;
            
            // Blend between preserved momentum and desired direction
            hv = Vector3.Lerp(desired, _initialHorizontalVelocity, decayFactor);
            
            if (_momentumTimer >= _momentumDecayTime)
            {
                _preserveMomentum = false;
            }
        }
        else
        {
            float targetSpeed = player.IsSprinting ? player.SprintSpeed : player.MoveSpeed;
            Vector3 desired = (player.transform.forward * player.VerticalInput + player.transform.right * player.HorizontalInput).normalized * targetSpeed;
            float t = _airControl * Time.deltaTime;
            hv = new(player.PlayerVelocity.x, 0f, player.PlayerVelocity.z);
            hv.x = Mathf.Lerp(hv.x, desired.x, t);
            hv.z = Mathf.Lerp(hv.z, desired.z, t);
        }
        
        player.PlayerVelocity = new Vector3(hv.x, player.PlayerVelocity.y, hv.z);

        // Gravity (unless currently wallrunning which would trigger transition soon)
        if (!player.WallRun.IsWallRunning)
        {
            player.PlayerVelocity = new Vector3(player.PlayerVelocity.x, player.PlayerVelocity.y + player.Gravity * Time.deltaTime, player.PlayerVelocity.z);
        }
    }

    public IMovementState TryTransition(PlayerController player)
    {
        if (_dashTriggered)
            return new DashState();
        if (player.WallRun.IsWallRunning)
            return new WallRunState();
        if (player.Controller.isGrounded)
            if (!_slideTriggered)
                return new GroundedState();
            else
                return new SlideState();
        if (player.LedgeDetector.detectLedge(player.transform))
        {
            return new LedgeHangState();
        }
        return null;
    }
}
