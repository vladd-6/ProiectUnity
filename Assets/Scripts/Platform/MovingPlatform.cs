using UnityEngine;

public class SmoothPlatform : MonoBehaviour
{
    [Header("Setări")]
    public Vector3 moveOffset = new Vector3(100, 0, 0);
    public float viteza = 25.0f;

    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 target;
    
    // Componente necesare
    private Rigidbody rb;
    private CharacterController playerCC; 
    
    // Pentru a calcula mișcarea
    private Vector3 lastPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Configurare automată a Rigidbody-ului pentru a preveni erori
        rb.isKinematic = true; 
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Asta face mișcarea fină!

        startPos = transform.position;
        endPos = startPos + moveOffset;
        target = endPos;
        lastPosition = rb.position;
    }

    // Folosim FixedUpdate pentru că lucrăm cu Rigidbody
    void FixedUpdate()
    {
        // 1. Calculăm unde vrem să ajungem în acest pas de fizică
        Vector3 nextPosition = Vector3.MoveTowards(rb.position, target, viteza * Time.fixedDeltaTime);
        
        // 2. Calculăm exact cât ne mișcăm în acest frame (Delta)
        Vector3 platformMovement = nextPosition - rb.position;

        // 3. Mutăm platforma folosind FIZICA (foarte important pentru smooth)
        rb.MovePosition(nextPosition);

        // 4. Dacă avem jucătorul în Trigger, îl mutăm și pe el
        if (playerCC != null)
        {
            // CharacterController trebuie să primească mișcarea imediat
            playerCC.Move(platformMovement);
        }

        // 5. Verificăm dacă am ajuns la destinație
        if (Vector3.Distance(rb.position, target) < 0.02f)
        {
            target = (target == endPos) ? startPos : endPos;
        }
    }

    // --- DETECTARE CU TRIGGER ---
    // Asigură-te că ai acel al doilea BoxCollider setat pe "Is Trigger" deasupra platformei

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