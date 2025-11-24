using UnityEngine;

public interface IMovementState
{
    void OnEnter(PlayerController player);
    void OnExit(PlayerController player);
    void HandleInput(PlayerController player);
    void Tick(PlayerController player);
    IMovementState TryTransition(PlayerController player);
}
