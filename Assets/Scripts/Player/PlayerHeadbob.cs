using UnityEngine;

public class Headbob : MonoBehaviour
{
    public float walkBobbingSpeed = 14f;
    public float sprintBobbingSpeed = 18f;
    public float bobbingAmount = 0.05f;
    public float cameraYMidpoint = 0f;
    private float headbobTimer = 0f;

    public struct Params
    {
        public float HorizontalInput;
        public float VerticalInput;
        public bool IsGrounded;
        public bool IsWallRunning;
        public bool IsSprinting;
    }

    public void SetCameraYMidpoint(float midPoint)
    {
        cameraYMidpoint = midPoint;
    }

    public void ApplyCameraTransform(ref Vector3 localCameraPosition, Params p)
    {
        // Only headbob when grounded and moving (not wallrunning or in air)
        bool isMoving = p.HorizontalInput != 0 || p.VerticalInput != 0;

        if (p.IsGrounded && isMoving && !p.IsWallRunning)
        {
            // Use different bobbing speed based on sprinting
            float currentBobbingSpeed = p.IsSprinting ? sprintBobbingSpeed : walkBobbingSpeed;

            // Increment timer
            headbobTimer += Time.deltaTime * currentBobbingSpeed;

            // Calculate bobbing offset using sine wave
            float bobbingOffset = Mathf.Sin(headbobTimer) * bobbingAmount;

            // Apply bobbing to camera Y position
            Vector3 newCameraPos = localCameraPosition;
            newCameraPos.y = cameraYMidpoint + bobbingOffset;
            localCameraPosition = newCameraPos;
        }
        else
        {
            // Reset timer when not moving/grounded
            headbobTimer = 0f;

            // Smoothly return camera to midpoint
            Vector3 newCameraPos = localCameraPosition;
            newCameraPos.y = Mathf.Lerp(newCameraPos.y, cameraYMidpoint, Time.deltaTime * 5f);
            localCameraPosition = newCameraPos;
        }
    }
}