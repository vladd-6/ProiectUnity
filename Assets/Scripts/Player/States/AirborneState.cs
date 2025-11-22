using UnityEngine;

public class AirborneState : IMovementState
{
    public void OnEnter(PlayerController player) {}
    public void OnExit(PlayerController player) {}

    public void HandleInput(PlayerController player)
    {
        player.ReadMovementInput();
        bool isTryingToMove = player.HorizontalInput != 0 || player.VerticalInput != 0;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
        player.IsSprinting = isTryingToMove && isTryingToSprint; // Sprint only affects horizontal air control speed
        // No jump here (only wall / grounded)
    }

    public void Tick(PlayerController player)
    {
        float targetSpeed = player.IsSprinting ? player.SprintSpeed : player.MoveSpeed;
        Vector3 desired = (player.transform.forward * player.VerticalInput + player.transform.right * player.HorizontalInput).normalized * targetSpeed;
        float t = player.AirControl * Time.deltaTime;
        Vector3 hv = new(player.PlayerVelocity.x, 0f, player.PlayerVelocity.z);
        hv.x = Mathf.Lerp(hv.x, desired.x, t);
        hv.z = Mathf.Lerp(hv.z, desired.z, t);
        player.PlayerVelocity = new Vector3(hv.x, player.PlayerVelocity.y, hv.z);

        // Gravity (unless currently wallrunning which would trigger transition soon)
        if (!player.WallRun.IsWallRunning)
        {
            player.PlayerVelocity = new Vector3(player.PlayerVelocity.x, player.PlayerVelocity.y + player.Gravity * Time.deltaTime, player.PlayerVelocity.z);
        }
    }

    public IMovementState TryTransition(PlayerController player)
    {
        if (player.WallRun.IsWallRunning)
            return new WallRunState();
        if (player.Controller.isGrounded)
            return new GroundedState();
        return null;
    }
}
