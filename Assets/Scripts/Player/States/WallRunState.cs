using UnityEngine;

public class WallRunState : IMovementState
{
    // TODO: Make wallrunning have
    private bool _wallJumpTriggered;

    public void OnEnter(PlayerController player) { _wallJumpTriggered = false; }

    public void OnExit(PlayerController player)
    {
        if (player.WallRun.IsWallRunning)
        {
            player.WallRun.StopWallRun();
        }
    }

    public void HandleInput(PlayerController player)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 horizontalVel = new(player.PlayerVelocity.x, 0f, player.PlayerVelocity.z);
            Vector3 outwardDir = Vector3.zero;
            if (player.WallRun.CurrentWallSide == Wallrun.WallRunSide.Right)
                outwardDir = -player.transform.right;
            else if (player.WallRun.CurrentWallSide == Wallrun.WallRunSide.Left)
                outwardDir = player.transform.right;
            const float outwardStrength = 5f;
            Vector3 newVel = horizontalVel + outwardDir * outwardStrength;
            newVel.y = player.JumpForce;
            player.PlayerVelocity = newVel;
            player.WallRun.StopWallRun();
            _wallJumpTriggered = true; // skip wallrun override this frame
        }
    }

    public void Tick(PlayerController player)
    {
        if (_wallJumpTriggered)
            return; // preserve jump impulse until state transitions

        var v = player.PlayerVelocity;
        player.WallRun.ApplyWallrunVelocity(ref v);
        player.PlayerVelocity = v;
    }

    public IMovementState TryTransition(PlayerController player)
    {
        if (_wallJumpTriggered)
        {
            return player.Controller.isGrounded ? new GroundedState() : new AirborneState();
        }
        if (!player.WallRun.IsWallRunning)
        {
            return player.Controller.isGrounded ? new GroundedState() : new AirborneState();
        }
        return null;
    }
}
