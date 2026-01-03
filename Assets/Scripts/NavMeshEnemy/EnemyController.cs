using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack
}

public class EnemyController : MonoBehaviour
{
    private EnemyState currentState;
    [Header("detection settings")]
    public float detectionRange = 30f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    public float shootingRange = 5f;
    public float fireRate = 1f;
    private EnemyGunSystem gun;

    private Transform player;
    private NavMeshAgent agent;
    private float fireTimer;

    //used to add a delay before starting to shoot
    private float reactionTime = 0.3f;
    private float reactionTimeCounter;

    [Header("Accuracy")]
    [Range(0f, 1f)]
    [SerializeField] private float accuracy = 0.1f; //1=best
    [SerializeField] private float spreadAngle = 10f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitAtPoint = 1.5f;

    private int patrolIndex;
    private float waitTimer;

    [Header("Combat Movement")]
    private float preferredCombatDistance = 3f;
    private float combatMoveSpeedMultiplier = 0.6f;

    private float originalSpeed;
    private float acceleration;

    private Animator animator;

    void Start()
    {
        reactionTimeCounter = 0;
        //get player and agent (enemy)
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }
        else
        {
            Debug.LogError("Player not found");
        }
        agent = GetComponent<NavMeshAgent>();
        //get the gun component
        gun = GetComponentInChildren<EnemyGunSystem>();
        //set initial state to idle
        if (patrolPoints.Length == 0)
            currentState = EnemyState.Idle;
        else
            currentState = EnemyState.Patrol;

        originalSpeed = agent.speed;
        acceleration = agent.acceleration;

        animator = GetComponentInChildren<Animator>();

        /*Debug.Log(agent.isOnNavMesh);*/
        if (animator == null)
            Debug.Log("animator is null");
        Debug.Log(animator);
    }


    void Update()
    {
        //calculate distance to the player
        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle(distance); break;

            case EnemyState.Chase:
                HandleChase(distance); break;

            case EnemyState.Attack:
                HandleAttack(distance); break;

            case EnemyState.Patrol:
                HandlePatrol(distance); break;
        }

        UpdateAnimations();

    }
    void UpdateAnimations()
    {
        if (animator == null || agent == null)
            return;

        float speedPercent = agent.velocity.magnitude / 10;
        animator.SetFloat("speed", speedPercent);
    }

    //idle state
    void HandleIdle(float distance)
    {
        agent.isStopped = true;
        if(distance <= detectionRange && HasLineOfSight())
        {
            ChangeState(EnemyState.Chase);
        }
    }
    //at the beginning, the enemy is patrolling
    void HandlePatrol(float distance)
    {
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtPoint)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                waitTimer = 0f;
            }
        }

        //if the player is detected the enemy start chasing
        if (HasLineOfSight())
        {
            ChangeState(EnemyState.Chase);
        }
    }
    void HandleChase(float distance)
    {
        agent.isStopped = false;
        /*agent.speed = originalSpeed;*/
        if(distance > detectionRange - 10)
        {
            agent.speed = acceleration;
        }
        else if (distance <= detectionRange - 10)
        {
            agent.speed = originalSpeed;
        }


        agent.SetDestination(player.position); //set the player as the target destination
        //if enemy enters shooting range, attack
        if(distance  <= shootingRange)
        {
            reactionTimeCounter = 0;
            ChangeState(EnemyState.Attack);
        }
        //stop if the player exit detection range
        else if(distance > detectionRange)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    void HandleAttack(float distance)
    {
        //add a reaction time before starting to shoot
        reactionTimeCounter += Time.deltaTime;
        if (reactionTimeCounter < reactionTime)
            return;

        agent.isStopped = false;
        //decrease the speed when shooting while walking
        agent.speed = originalSpeed * combatMoveSpeedMultiplier;

        OrientToPlayer();

        if (distance > preferredCombatDistance)
        {
            agent.SetDestination(player.position);
            animator.SetBool("HoldingGround", false);
        }
        else
        {
            agent.SetDestination(transform.position);
            animator.SetBool("HoldingGround", true);
        }

        //start the shooting a little bit forward to avoid hitting himself
        Vector3 perfectDirection = (player.position - (gun.FireOrigin.transform.position)+gun.FireOrigin.transform.forward*0.5f).normalized;
        Vector3 imperfectDirection = getInnacurateDirection(perfectDirection);
        gun.TryShoot(imperfectDirection);

        if(distance > shootingRange &&  distance <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else if(distance > detectionRange)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    void ChangeState(EnemyState newState)
    {
        if(currentState == newState)
            { return; }

        //if enters the attack state change to shooting animations
        if (animator != null)
            animator.SetBool("isShooting", newState == EnemyState.Attack);

        currentState = newState;
    }

    bool HasLineOfSight()
    {
        //get the distance between position of the player and the enemy
        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 direction = (player.position - transform.position).normalized;

        //check if the enemy can see the player taking into account distance, view angle and obstacles
        if (distance <= detectionRange)
        {
            float angle = Vector3.Angle(transform.forward, direction);
            if(angle <= viewAngle / 2)
            {
                if (!Physics.Linecast(transform.position + Vector3.up*1.2f, player.position, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void OrientToPlayer()
    {
        Vector3 direction = (player.position-transform.position).normalized;
        direction.y = 0;
        //compute rotation in direction of the player
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }

    Vector3 getInnacurateDirection(Vector3 direction)
    {
        //determine the impact the distance might have on the accuracy
        float distance = Vector3.Distance(transform.position, player.position);
        float distanceImpact = Mathf.InverseLerp(shootingRange, detectionRange, distance);

        //use a formula to calculate the final accuracy
        float finalAccuracy = accuracy * (1f - distanceImpact * 0.5f);

        float spread = Mathf.Lerp(spreadAngle, 0, finalAccuracy);

        //get point on y-axis
        float yaw = Random.Range(-spread, spread);
        //get point on x-axis
        float pitch = Random.Range(-spread, spread);

        Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0);
        return spreadRotation*direction;
    }


}
