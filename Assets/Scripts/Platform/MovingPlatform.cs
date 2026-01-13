using UnityEngine;

public class SmoothPlatform : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 moveOffset = new Vector3(100, 0, 0);
    public float viteza = 25.0f;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 target;
    
    private Rigidbody rb;
    private CharacterController playerCC; 
    
    private Vector3 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.isKinematic = true; 
        rb.interpolation = RigidbodyInterpolation.Interpolate; 

        startPos = transform.position;
        endPos = startPos + moveOffset;
        target = endPos;
        lastPosition = rb.position;
    }

    void FixedUpdate()
    {
        Vector3 nextPosition = Vector3.MoveTowards(rb.position, target, viteza * Time.fixedDeltaTime);
        
        Vector3 platformMovement = nextPosition - rb.position;

        rb.MovePosition(nextPosition);

        if (playerCC != null)
        {
            playerCC.Move(platformMovement);
        }

        if (Vector3.Distance(rb.position, target) < 0.02f)
        {
            target = (target == endPos) ? startPos : endPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCC = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCC = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 s = Application.isPlaying ? startPos : transform.position;
        Vector3 e = s + moveOffset;
        if (moveOffset != Vector3.zero) Gizmos.DrawLine(s, e);
    }
}