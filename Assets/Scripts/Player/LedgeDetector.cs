using UnityEngine;


public class LedgeDetector : MonoBehaviour
{
    [Header("Detection")]
    public float forwardCheckDistance = 1f;
    public float downwardCheckDistance = 3f;
    public float ledgeHeightHigh = 2.5f;
    public LayerMask ground;

    public Vector3 HangPoint { get; private set; }
    public Vector3 ClimbPoint { get; private set; }

    public bool detectLedge(Transform transform)
    {
        //do a forward ray cast from the player to check if there is a wall in front of him
        RaycastHit wallHit;
        Vector3 origin = transform.position;

        if (!Physics.Raycast(origin, transform.forward, out wallHit, forwardCheckDistance, ground))
        {
            return false;
        }
        //if yes, do a vertical ray cast to determine if there is space above the wall
        Vector3 topOrigin = wallHit.point + Vector3.up * ledgeHeightHigh;
        RaycastHit topHit;

        if (!Physics.Raycast(topOrigin, Vector3.down, out topHit, downwardCheckDistance, ground))
        {
            return false;
        }

        //calculate the hang point and the new position of the player after climbing
        HangPoint = topHit.point - transform.forward * 0.5f;
        HangPoint -= Vector3.up * 0.3f;
        ClimbPoint = topHit.point + transform.forward * 0.5f + Vector3.up * 0.1f;

        return true;

    }



}
