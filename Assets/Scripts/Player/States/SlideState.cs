using UnityEngine;

public class SlideState : IMovementState
{
    private float slideSpeed = 30f;
    private float _slideDuration = 1f;
    private float _slideTimer = 0f;
    private bool _isSliding;
    private Vector3 _slideDirection;
    
    public void HandleInput(PlayerController player)
    {

    }

    public void OnEnter(PlayerController player)
    {
        _slideTimer = _slideDuration;
        _isSliding = true;
        _slideDirection = player.transform.forward;
    }

    public void OnExit(PlayerController player)
    {

    }

    public void Tick(PlayerController player)
    {
        _slideTimer -= Time.deltaTime;
        if (_slideTimer <= 0f)
        {
            _isSliding = false;
            return;
        }

        player.PlayerVelocity = new Vector3(_slideDirection.x * slideSpeed, player.PlayerVelocity.y, _slideDirection.z * slideSpeed);
    }

    public IMovementState TryTransition(PlayerController player)
    {
        if (!_isSliding)
        {
            if (player.Controller.isGrounded)
                return new GroundedState();
            else
                return new AirborneState();
        }

        return null;
    }
}