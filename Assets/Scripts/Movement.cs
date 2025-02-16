using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float jumpForce = 3.0f;
    public float jumpHoldForce = 0.1f;
    public float moveSpeed = 6;
    public float dashForce = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.3f;
    public float climbSpeed = 3f;
    public float coyoteTime = 0.15f;

    private bool isGrounded;
    private bool canDash = true;
    private bool hasDashed = false;
    private bool isDashing = false;
    private bool isClimbing = false;
    private bool isTouchingWall = false;
    private float lastGroundedTime;

    private Rigidbody2D rb;
    private float jumpTime = 0f;
    private float lastDashTime = -100f;
    private Vector2 lastDirection = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing) return;

        float xMovement = Input.GetAxisRaw("Horizontal");
        float yMovement = Input.GetAxisRaw("Vertical");

        if (xMovement != 0)
        {
            lastDirection = new Vector2(xMovement, 0).normalized;
        }

        if (!isClimbing)
        {
            rb.linearVelocity = new Vector2(xMovement * moveSpeed, rb.linearVelocity.y);
        }

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.C) && (isGrounded || Time.time - lastGroundedTime <= coyoteTime))
        {
            isGrounded = false;
            jumpTime = Time.time + 0.2f;
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }

        if (Input.GetKey(KeyCode.C) && !isGrounded && Time.time < jumpTime)
        {
            rb.AddForce(new Vector2(0, jumpHoldForce), ForceMode2D.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.X) && canDash && !hasDashed)
        {
            Vector2 dashDirection = GetDashDirection(xMovement, yMovement);
            StartCoroutine(Dash(dashDirection));
        }

        if (isTouchingWall && Input.GetKey(KeyCode.Z))
        {
            isClimbing = true;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(0, yMovement * climbSpeed);
        }
        else if (isClimbing && !Input.GetKey(KeyCode.Z))
        {
            isClimbing = false;
            rb.gravityScale = 1;
        }
    }

    private Vector2 GetDashDirection(float x, float y)
    {
        Vector2 dashDirection = new Vector2(x, y).normalized;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = lastDirection;
        }
        return dashDirection;
    }

    private IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        canDash = false;
        if (!isGrounded) hasDashed = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = direction * dashForce;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        lastDashTime = Time.time;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            hasDashed = false;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
            isClimbing = false;
            rb.gravityScale = 1;
        }
    }
}
