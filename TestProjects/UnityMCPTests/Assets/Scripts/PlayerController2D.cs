using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float groundCheckDistance = 0.1f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayerMask = 1;
    
    private Rigidbody rb;
    private bool isGrounded;
    private float horizontalInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (groundCheck == null)
        {
            // Create ground check object if not assigned
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    void Update()
    {
        // Get input
        horizontalInput = Input.GetAxis("Horizontal");
        
        // Jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
        
        // Check if grounded
        CheckGrounded();
    }
    
    void FixedUpdate()
    {
        // Apply horizontal movement
        Vector3 movement = new Vector3(horizontalInput * moveSpeed, rb.velocity.y, 0);
        rb.velocity = movement;
    }
    
    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, 0);
    }
    
    void CheckGrounded()
    {
        // Cast a ray downward to check for ground
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayerMask);
        
        // Debug visualization
        Debug.DrawRay(groundCheck.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
    }
}