using UnityEngine;
using UnityEngine.UIElements;

public class GroundedState : IMovementState
{
    private bool _slideTriggered = false;
    private bool _dashTriggered = false;
    private bool _preserveMomentum = false;
    private Vector3 _initialHorizontalVelocity;
    private float _momentumDecayTime = 0.3f;
    private float _momentumTimer = 0f;
    
    public void OnEnter(PlayerController player)
    {
        _dashTriggered = false;
        if (player.PlayerVelocity.y < 0f)
            player.PlayerVelocity = new Vector3(player.PlayerVelocity.x, -2f, player.PlayerVelocity.z);
        player.WallRun.OnGroundedStateChanged(true);
        
        // Preserve horizontal velocity if coming from a high-speed state
        Vector3 horizontalVel = new Vector3(player.PlayerVelocity.x, 0, player.PlayerVelocity.z);
        float horizontalSpeed = horizontalVel.magnitude;
        
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

    public void OnExit(PlayerController player)
    {
        player.WallRun.OnGroundedStateChanged(false);
    }

    public void HandleInput(PlayerController player)
    {
        player.ReadMovementInput();
        bool isTryingToMove = player.HorizontalInput != 0 || player.VerticalInput != 0;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
        player.IsSprinting = isTryingToMove && isTryingToSprint;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.PlayerVelocity = new Vector3(player.PlayerVelocity.x, player.JumpForce, player.PlayerVelocity.z);
        }

        _slideTriggered = Input.GetKeyDown(KeyCode.LeftControl);

        if (Input.GetMouseButtonDown((int)MouseButton.RightMouse) && player.DashClock <= 0f)
        {
            _dashTriggered = true;
            player.DashClock = player.DashDelay;
        }
    }    public void Tick(PlayerController player)
    {
        Vector3 moveDirection;
        
        if (_preserveMomentum)
        {
            _momentumTimer += Time.deltaTime;
            float decayFactor = 1f - Mathf.Clamp01(_momentumTimer / _momentumDecayTime);
            
            float currentSpeed = player.IsSprinting ? player.SprintSpeed : player.MoveSpeed;
            Vector3 desired = (player.transform.forward * player.VerticalInput + player.transform.right * player.HorizontalInput).normalized * currentSpeed;
            
            // Blend between preserved momentum and desired direction
            moveDirection = Vector3.Lerp(desired, _initialHorizontalVelocity, decayFactor);
            
            if (_momentumTimer >= _momentumDecayTime)
            {
                _preserveMomentum = false;
            }
        }
        else
        {
            float currentSpeed = player.IsSprinting ? player.SprintSpeed : player.MoveSpeed;
            moveDirection = (player.transform.forward * player.VerticalInput + player.transform.right * player.HorizontalInput).normalized * currentSpeed;
        }
        
        player.PlayerVelocity = new Vector3(moveDirection.x, player.PlayerVelocity.y, moveDirection.z);
    }

    public IMovementState TryTransition(PlayerController player)
    {
        if (_dashTriggered)
            return new DashState();
        if (player.WallRun.IsWallRunning)
            return new WallRunState();
        if (!player.Controller.isGrounded)
            return new AirborneState();
        if (SlideRequired(player)) {
            return new SlideState();
        }
        return null;
    }

    private bool SlideRequired(PlayerController player) {
        return _slideTriggered  && player.Controller.isGrounded;
    }
}
