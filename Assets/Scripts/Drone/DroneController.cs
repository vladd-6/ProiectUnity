using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Light viewCone; // drone spotting light
    public LayerMask obstacleMask;

    [Header("Movement Settings")]
    public float stoppingDistance = 5f;
    public float hoverHeight = 2.5f;
    public float movementSmoothTime = 0.5f;
    public float maxSpeed = 10f;
    public float rotationSpeed = 5f;

    [Header("Sensor Settings")]
    public float maxSightDistance = 20f;

    [Range(0, 180)]
    public float fieldOfViewAngle = 90f; // spotting range

    private Vector3 currentVelocityRef;
    public bool isPlayerVisible = false;
    private float detectionTimer = 0f;

    void Start()
    {
        // update light angle
        if (viewCone != null)
            viewCone.spotAngle = fieldOfViewAngle;

        // automatically set to target player
        if (player == null)
        {
            // search player tag
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogError("Player not found");
            }
        }
    }

    void Update()
    {
        if (player == null) 
            return;

        // check if player is seen
        CheckVisibility();

        // update drone light
        UpdateLightColor();

        // handle movement
        if (isPlayerVisible)
        {
            RotateTowardsPlayer();
            Move();
        }
    }

    void Move()
    {
        // calculate direction from player to drone (to know where to push back)
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;
        
        // force the drone to be at exactly stoppingDistance on that direction
        Vector3 targetPosition = player.position + (directionFromPlayer * stoppingDistance);

        // update height
        targetPosition.y = player.position.y + hoverHeight;

        // SmoothDamp for fluid physics-like movement
        transform.position = Vector3.SmoothDamp(
            transform.position,     // current pos
            targetPosition,         // target pos
            ref currentVelocityRef, // reference var for smoothing
            movementSmoothTime,     // inertia
            maxSpeed                // max speed cap
        );
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // rotation is horizontal

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void CheckVisibility()
    {
        bool currentlySeeing = false;
        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 direction = (player.position - transform.position).normalized;

        // check range
        if (distance < maxSightDistance)
        {
            // if player is inside the FOV cone
            if (Vector3.Angle(transform.forward, direction) < fieldOfViewAngle / 2)
            {
                // raycast slightly forward (0.5) to avoid hitting own body
                Vector3 rayStart = transform.position + (transform.forward * 0.5f);

                // checks for walls between drone and player
                if (!Physics.Linecast(rayStart, player.position, obstacleMask))
                {
                    currentlySeeing = true;
                }
            }
        }

        // Stability Buffer (0.2s) to prevent light flickering on edges
        if (currentlySeeing)
        {
            isPlayerVisible = true;
            detectionTimer = 0.2f;
        }
        else
        {
            detectionTimer -= Time.deltaTime;
            if (detectionTimer <= 0) isPlayerVisible = false;
        }
    }

    void UpdateLightColor()
    {
        if (!viewCone) 
            return;

        // update drone light 
        Color targetColor = isPlayerVisible ? Color.red : Color.green;
        viewCone.color = Color.Lerp(viewCone.color, targetColor, Time.deltaTime * 5f);
    }

    // visual debugging aids in Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxSightDistance);
    }
}