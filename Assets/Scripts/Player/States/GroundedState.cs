using UnityEngine;

public class GroundedState : IMovementState
{
    private bool _slideTriggered = false;
    public void OnEnter(PlayerController player)
    {
        if (player.PlayerVelocity.y < 0f)
            player.PlayerVelocity = new Vector3(player.PlayerVelocity.x, -2f, player.PlayerVelocity.z);
        player.WallRun.OnGroundedStateChanged(true);
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
    }

    public void Tick(PlayerController player)
    {
        float currentSpeed = player.IsSprinting ? player.SprintSpeed : player.MoveSpeed;
        Vector3 moveDirection = (player.transform.forward * player.VerticalInput + player.transform.right * player.HorizontalInput).normalized * currentSpeed;
        player.PlayerVelocity = new Vector3(moveDirection.x, player.PlayerVelocity.y, moveDirection.z);
    }

    public IMovementState TryTransition(PlayerController player)
    {
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
