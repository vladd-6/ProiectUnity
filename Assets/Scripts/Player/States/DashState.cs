using UnityEngine;

public class DashState : IMovementState
{
    private Vector3 _dashDirection;
    private Vector3 _previousVelocity;
    private Vector3 _dashBoost;
    private float _dashTimer = 0f;
    private const float DASH_DURATION = 0.25f;
    
    public void OnEnter(PlayerController player)
    {
        // Store previous velocity to preserve it
        _previousVelocity = player.PlayerVelocity;
        
        // Determine dash direction based on input, or forward if no input
        Vector3 inputDirection = (player.transform.forward * player.VerticalInput + 
                                  player.transform.right * player.HorizontalInput).normalized;
        
        if (inputDirection.magnitude < 0.1f)
        {
            // No input, dash forward
            _dashDirection = player.transform.forward;
        }
        else
        {
            _dashDirection = inputDirection;
        }
        
        _dashTimer = 0f;
        
        // Calculate and store the dash boost
        _dashBoost = _dashDirection * player.DashImpulse;
    }

    public void OnExit(PlayerController player)
    {
        // Optionally blend back to previous velocity or let next state handle it
    }

    public void HandleInput(PlayerController player)
    {
        player.ReadMovementInput();
        // Player maintains control during dash but dash velocity takes priority
    }

    public void Tick(PlayerController player)
    {
        // Calculate dash progress (0 to 1)
        float progress = Mathf.Clamp01(_dashTimer / DASH_DURATION);
        
        // Ease out the dash boost for a smooth deceleration
        float dashIntensity = 1f - progress;
        dashIntensity = dashIntensity * dashIntensity; // Quadratic ease out
        
        // Apply the dash boost with decreasing intensity
        Vector3 currentDashBoost = _dashBoost * dashIntensity;
        Vector3 currentVelocity = _previousVelocity + currentDashBoost;
        
        // Apply gravity
        float verticalVelocity = player.PlayerVelocity.y + player.Gravity * Time.deltaTime;
        
        player.PlayerVelocity = new Vector3(currentVelocity.x, verticalVelocity, currentVelocity.z);
        
        _dashTimer += Time.deltaTime;
    }

    public IMovementState TryTransition(PlayerController player)
    {
        // Dash complete, transition back to appropriate state
        if (_dashTimer >= DASH_DURATION)
        {
            if (player.Controller.isGrounded)
                return new GroundedState();
            else
                return new AirborneState();
        }
        
        // Can transition to wall run during dash
        if (player.WallRun.IsWallRunning)
            return new WallRunState();
        
        return null;
    }
}
