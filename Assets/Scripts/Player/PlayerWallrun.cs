using System.Collections;
using UnityEngine;

public class Wallrun : MonoBehaviour
{
    public float wallRunSpeed = 10f;
    public float wallRunUpwardForce = 3f;
    public float wallRunCameraTilt = 40f;
    public float wallRunTiltSpeed = 5f;
    public float minWallRunSpeed = 4f;
    public float wallRunDuration = 2f;
    private bool canWallRun = true;
    private bool isWallRunning = false;
    public enum WallRunSide
    {
        None,
        Left,
        Right
    }

    private WallRunSide currentWallSide = WallRunSide.None;
    private float horizontalSpeed;
    private Vector3 wallRunDirection;
    private float wallRunTimer;
    private bool touchedGroundSinceWallRun = true;
    private static WaitForSeconds _waitForSeconds0_3 = new(0.3f);
    public LayerMask ground;

    private bool isGrounded;
    private GameObject lastWallObject = null;

    public bool IsWallRunning => isWallRunning;
    public WallRunSide CurrentWallSide => currentWallSide;

    public void UpdateWallRun(bool isGrounded, ref float currentCameraTilt)
    {
        this.isGrounded = isGrounded;

        if (isWallRunning)
        {
            // Update wall run timer
            wallRunTimer -= Time.deltaTime;

            // Stop wallrunning if timer expires, no wall contact, or grounded
            if (wallRunTimer <= 0 || !IsTouchingWall() || this.isGrounded)
            {
                StopWallRun();
            }
        }
    }

    public void OnWallHit(ControllerColliderHit hit, ref Vector3 playerVelocity)
    {
        // Check if we hit something in our ground layer mask
        bool isInGroundLayer = ((1 << hit.gameObject.layer) & ground) != 0;

        if (isInGroundLayer &&
            Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f && // It's a wall (not ground)
            canWallRun &&
            !isWallRunning &&
            !isGrounded)
        {
            // Check if we have sufficient horizontal speed to start wall running
            Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            float currentHorizontalSpeed = horizontalVelocity.magnitude;

            if (currentHorizontalSpeed >= minWallRunSpeed)
            {
                // Prevent starting a wall run on the same wall unless we've touched ground
                if (!touchedGroundSinceWallRun && lastWallObject == hit.gameObject)
                {
                    return;
                }
                StartWallRun(hit, ref playerVelocity);
            }
        }
    }

    private void StartWallRun(ControllerColliderHit hit, ref Vector3 playerVelocity)
    {
        // Determine which wall we're hitting
        Vector3 localHitNormal = transform.InverseTransformDirection(hit.normal);

        Debug.Log("Collided!");

        if (localHitNormal.x > 0.5f) // Wall on our right
        {
            currentWallSide = WallRunSide.Left;
            wallRunDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;
        }
        else if (localHitNormal.x < -0.5f) // Wall on our left
        {
            currentWallSide = WallRunSide.Right;
            wallRunDirection = Vector3.Cross(Vector3.up, hit.normal).normalized;
        }
        else
        {
            // Front wall - run upward
            currentWallSide = WallRunSide.None;
            wallRunDirection = Vector3.up * 0.3f + transform.forward * 0.7f;
        }

        // Start wall running
        isWallRunning = true;
        wallRunTimer = wallRunDuration;

        // Give initial upward boost
        playerVelocity.y = wallRunUpwardForce;

        // Record which wall we started on and clear the 'touched ground' flag
        lastWallObject = hit.gameObject;
        touchedGroundSinceWallRun = false;
    }

    public void ApplyWallrunVelocity(ref Vector3 playerVelocity)
    {
        playerVelocity = wallRunDirection * wallRunSpeed;
        playerVelocity.y = Mathf.MoveTowards(playerVelocity.y, -2f, Time.deltaTime * 3f);
    }

    public void OnGroundedStateChanged(bool grounded)
    {
        if (grounded)
        {
            touchedGroundSinceWallRun = true;
        }
    }

    private bool IsTouchingWall()
    {
        float checkDistance = 1.1f;

        if (currentWallSide == WallRunSide.Left)
        {
            return Physics.Raycast(transform.position, -transform.right, checkDistance, ground);
        }
        else if (currentWallSide == WallRunSide.Right)
        {
            return Physics.Raycast(transform.position, transform.right, checkDistance, ground);
        }

        return false;
    }

    public void StopWallRun()
    {
        isWallRunning = false;
        currentWallSide = WallRunSide.None;
        StartCoroutine(WallRunCooldown());
    }

    private IEnumerator WallRunCooldown()
    {
        canWallRun = false;
        yield return _waitForSeconds0_3;
        canWallRun = true;
    }

    public void HandleCameraTilt(ref float currentCameraTilt)
    {
        float targetTilt = 0f;

        if (isWallRunning)
        {
            switch (currentWallSide)
            {
                case WallRunSide.Left:
                    targetTilt = -wallRunCameraTilt;
                    break;
                case WallRunSide.Right:
                    targetTilt = wallRunCameraTilt;
                    break;
            }
        }

        currentCameraTilt = Mathf.Lerp(currentCameraTilt, targetTilt, wallRunTiltSpeed * Time.deltaTime);
    }
}