using UnityEngine;

public class SlideState : IMovementState
{
    private float slideSpeed = 30f;
    private float _slideDuration = 1f;
    private float _slideTimer = 0f;
    private bool _isSliding;
    private Vector3 _slideDirection;
    private float _initialCameraY;
    private float _targetCameraY = 0f;
    private float _slideInDuration = 0.15f;  // How long to slide camera down
    private float _slideOutDuration = 0.15f; // How long to slide camera back up
    private bool _jumpedFromSlide = false;
    public void HandleInput(PlayerController player)
    {

    }

    public void OnEnter(PlayerController player)
    {
        _slideTimer = _slideDuration;
        _isSliding = true;
        _slideDirection = player.transform.forward;
        _initialCameraY = player.playerCamera.transform.localPosition.y;
    }

    public void OnExit(PlayerController player)
    {
        // Store slide exit info for next state to handle camera lerp
        if (_jumpedFromSlide)
        {
            player.SetPendingCameraLerp(_initialCameraY, 0.2f);
        }
    }

    public void Tick(PlayerController player)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.PlayerVelocity = new Vector3(player.PlayerVelocity.x, player.JumpForce, player.PlayerVelocity.z);
            _jumpedFromSlide = true;
            _isSliding = false;
            return;
        }

        _slideTimer -= Time.deltaTime;
        if (_slideTimer <= 0f)
        {
            _isSliding = false;
            return;
        }

        float elapsedTime = _slideDuration - _slideTimer;
        float t;

        if (elapsedTime < _slideInDuration)
        {
            // Ease in phase
            float progress = elapsedTime / _slideInDuration;
            t = progress * progress;  // Quadratic ease in
        }
        else if (_slideTimer < _slideOutDuration)
        {
            // Ease out phase
            float progress = (_slideOutDuration - _slideTimer) / _slideOutDuration;
            t = 1f - (progress * progress);  // Quadratic ease out from target back to initial
        }
        else
        {
            // Hold at target position
            t = 1f;
        }

        var cameraPos = player.playerCamera.transform.localPosition;
        cameraPos.y = Mathf.Lerp(_initialCameraY, _targetCameraY, t);
        player.playerCamera.transform.localPosition = cameraPos;

        player.PlayerVelocity = new Vector3(_slideDirection.x * slideSpeed, player.PlayerVelocity.y, _slideDirection.z * slideSpeed);
    }

    // TODO: Add lerp when slide->airborne->ground (lerp on ground)
    public IMovementState TryTransition(PlayerController player)
    {
        // sliding ended
        if (!_isSliding)
        {
            if (player.Controller.isGrounded)
                return new GroundedState();
            else
                return new AirborneState();
        }
        else
        { // stop mid-slide
            if (!player.Controller.isGrounded)
                return new AirborneState();
        }

        return null;
    }
}