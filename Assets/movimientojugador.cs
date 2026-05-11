using UnityEngine;

public class movimientojugador : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform gfx;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform feetPos;
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private float jumpTime = 0.2f;
    [SerializeField] private float jumpForceHold = 20f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float crouchHeight = 0.5f;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimer;

    private void Update()
    {
        #region jump
        isGrounded = Physics2D.OverlapCircle(feetPos.position, groundDistance, groundLayer);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            jumpTimer = jumpTime;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimer > 0)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y + jumpForceHold * Time.deltaTime
                );

                jumpTimer -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;

            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * 0.5f
                );
            }
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        #endregion jump

        #region crouch
        if(isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, crouchHeight, gfx.localScale.z);
        }
        if (isJumping && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
        }
        #endregion crouch
    }
}