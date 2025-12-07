using UnityEngine;
using System.Collections;

public class LedgeClimbState : IMovementState
{
    public float climbDuration = 0.25f;
    private float timer = 0f;
    private Vector3 initialPosition;
    private Vector3 finalPosition;

    public void OnEnter(PlayerController player)
    {
        timer = 0f;
        initialPosition = player.transform.position;
        finalPosition = player.LedgeDetector.ClimbPoint;

        player.Controller.enabled = false;
        player.PlayerVelocity = Vector3.zero;

    }
    public void OnExit(PlayerController player)
    {
        player.Controller.enabled = true;
    }

    public void HandleInput(PlayerController player)
    {

    }

    public void Tick(PlayerController player)
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / climbDuration);
        player.transform.position = Vector3.Lerp(initialPosition, finalPosition, t);

    }

    public IMovementState TryTransition(PlayerController player)
    {
        //timer exceeds climb duration
        if (timer >= climbDuration)
        {
            return new GroundedState();
        }
        return null;
    }



}
