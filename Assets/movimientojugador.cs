using UnityEngine;

public class MovimientoJugador : MonoBehaviour
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
        // Obtenemos la hitbox que permite que el personaje no traspasé el suelo.
        isGrounded = Physics2D.OverlapCircle(feetPos.position, groundDistance, groundLayer);

        Jump();
        HoldJump();
        ReleaseJump();
        Crouch();
        CalculateJumpPhysics();
    }

    /// <summary>
    /// Controla el funcionamiento del salto del personaje.
    /// </summary>
    private void Jump()
    {
        // El personaje solo puede saltar cuando está tocando el suelo.
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            jumpTimer = jumpTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    /// <summary>
    /// Controla el funcionamiento del agacharse del personaje.
    /// </summary>
    private void Crouch()
    {
        // Solo puedes agacharte cuando está en el suelo usando la tecla SHIFT.
        if (isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, crouchHeight, gfx.localScale.z);
        }

        // * Recuperas tu tamaño original cuando...
        // > Saltas, aunque estes manteniendo pulsando el botón de agachado.
        if (isJumping && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
        }

        // > Sueltas la tecla de agachado.
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CalculateJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void HoldJump()
    {
        // Alarga el salto del personaje cuando mantienes pulsado el botón de salto.
        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            // Pero hasta cierto límite.
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
                // Pasado este límite: empiezas a caer.
                isJumping = false;
            }
        }
    }

    private void ReleaseJump()
    {
        // Al soltar la tecla de salto, dejas de elevarte.
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;

            // Aplica una fuerza hacia el suelo.
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * 0.5f
                );
            }
        }
    }
}