using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float sprintIncrease = 2.0f;
    public float rotateSpeed = 180.0f;

    [Header("Jumping Settings")]
    public float jumpForce = 7.0f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private float groundCheckDistance = 0.6f;
    private float verticalInput;
    private float horizontalInput;
    private bool isSprinting;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            groundCheckDistance = col.bounds.extents.y + 0.1f;
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = moveSpeed;
        if (isSprinting)
        {
            currentSpeed += sprintIncrease;
        }

        Vector3 moveDirection = transform.forward * verticalInput * currentSpeed;
        rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);

        float rotation = horizontalInput * rotateSpeed * Time.fixedDeltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}