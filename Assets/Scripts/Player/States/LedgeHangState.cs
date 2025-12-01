using UnityEngine;

public class LedgeHangState : IMovementState
{
    public void OnEnter(PlayerController player)
    {
        player.Controller.enabled = false;
        player.PlayerVelocity = Vector3.zero;

        // Snap into hang position
        player.transform.position = player.LedgeDetector.HangPoint;
        player.Controller.enabled = true;
    }

    public void OnExit(PlayerController player)
    {

    }

    public void HandleInput(PlayerController player)
    {
        player.PlayerVelocity = Vector3.zero;
    }

    public void Tick(PlayerController player)
    {

    }

    public IMovementState TryTransition(PlayerController player)
    {
        if (Input.GetKeyDown(KeyCode.W))
            return new LedgeClimbState();

        if (Input.GetKeyDown(KeyCode.S))
            return new AirborneState(); // drop down

        return null;
    }

}
