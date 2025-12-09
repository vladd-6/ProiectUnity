using UnityEngine;

public class LedgeHangState : IMovementState
{
    private float transitionTime = 0.4f;
    private float initialCameraY;
    private float finalCameraY;
    private float cameraDownOffset = -0.35f;


    public void OnEnter(PlayerController player)
    {
        //do a camera lerp so the snap won't feel too sudden
        initialCameraY = player.playerCamera.transform.localPosition.y;
        finalCameraY = initialCameraY + cameraDownOffset;
        player.SetPendingCameraLerp(finalCameraY, transitionTime);

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
        //climb if w is pressed
        if (Input.GetKey(KeyCode.W))
            return new LedgeClimbState();

        if (Input.GetKeyDown(KeyCode.S))
        {
            player.HangStateTimer = player.DelayTime;
            return new AirborneState(); // drop down
        }

        return null;
    }

}
