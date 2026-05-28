using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    // Referencia al Rigidbody del jugador
    [SerializeField] private Rigidbody2D rb;

    // Parte visual del jugador (para escalar al agacharse)
    [SerializeField] private Transform gfx;

    // Fuerza del salto inicial
    [SerializeField] private float jumpForce = 10f;

    // Capa del suelo para detectar si está tocando el suelo
    [SerializeField] private LayerMask groundLayer;

    // Punto de comprobación del suelo (pies)
    [SerializeField] private Transform feetPos;

    // Distancia para detectar el suelo
    [SerializeField] private float groundDistance = 0.25f;

    // Tiempo máximo para mantener el salto
    [SerializeField] private float jumpTime = 0.2f;

    // Fuerza adicional mientras se mantiene el salto
    [SerializeField] private float jumpForceHold = 20f;

    // Multiplicador de caída más rápida
    [SerializeField] private float fallMultiplier = 2.5f;

    // Multiplicador para salto corto si sueltas el botón
    [SerializeField] private float lowJumpMultiplier = 2f;

    // Altura cuando el personaje se agacha
    [SerializeField] private float crouchHeight = 0.5f;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimer;

    private void Update()
    {
        // Comprueba si el jugador está tocando el suelo
        isGrounded = Physics2D.OverlapCircle(feetPos.position, groundDistance, groundLayer);

        // Ejecuta toda la lógica de movimiento
        Jump();
        HoldJump();
        ReleaseJump();
        CalculateJumpPhysics();
        Crouch();
    }

    private void Jump()
    {
        // Salto inicial si está en el suelo
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            jumpTimer = jumpTime;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void Crouch()
    {
        // Agacharse mientras está en el suelo
        if (isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, crouchHeight, gfx.localScale.z);
        }

        // Si está en el aire, vuelve al tamaño normal
        if (isJumping && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
        }

        // Al soltar la tecla, vuelve al tamaño normal
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
        }
    }

    private void CalculateJumpPhysics()
    {
        // Hace que la caída sea más rápida (mejor sensación de juego)
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (fallMultiplier - 1) *
                Time.deltaTime;
        }
        // Si suelta salto en el aire, reduce altura del salto
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (lowJumpMultiplier - 1) *
                Time.deltaTime;
        }
    }

    private void HoldJump()
    {
        // Mantener salto mientras haya tiempo disponible
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
    }

    private void ReleaseJump()
    {
        // Si suelta el botón de salto en el aire
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;

            // Reduce la velocidad hacia arriba si estaba subiendo
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