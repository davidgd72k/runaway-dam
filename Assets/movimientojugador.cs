using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    // Referencia al Rigidbody del jugador
    [SerializeField] private Rigidbody2D rb;

    // Parte visual del jugador (para escalar al agacharse)
    [SerializeField] private Transform gfx;

    // Fuerza del salto inicial
    [SerializeField] private float jumpForce = 10f;

    // Capa del suelo para detectar si est� tocando el suelo
    [SerializeField] private LayerMask groundLayer;

    // Punto de comprobaci�n del suelo (pies)
    [SerializeField] private Transform feetPos;

    // Distancia para detectar el suelo
    [SerializeField] private float groundDistance = 0.25f;

    // Tiempo m�ximo para mantener el salto
    [SerializeField] private float jumpTime = 0.2f;

    // Fuerza adicional mientras se mantiene el salto
    [SerializeField] private float jumpForceHold = 20f;

    // Multiplicador de ca�da m�s r�pida
    [SerializeField] private float fallMultiplier = 2.5f;

    // Multiplicador para salto corto si sueltas el bot�n
    [SerializeField] private float lowJumpMultiplier = 2f;

    // Altura cuando el personaje se agacha
    [SerializeField] private float crouchHeight = 0.5f;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimer;

    private void Update()
    {
        // Comprueba si el jugador est� tocando el suelo
        isGrounded = Physics2D.OverlapCircle(feetPos.position, groundDistance, groundLayer);

        // Ejecuta toda la l�gica de movimiento
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
        // Salto inicial si est� en el suelo
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
        // Solo puedes agacharte cuando est� en el suelo usando la tecla SHIFT.
        if (isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, crouchHeight, gfx.localScale.z);
        }

        // * Recuperas tu tama�o original cuando...
        // > Saltas, aunque estes manteniendo pulsando el bot�n de agachado.
        if (isJumping && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
        }

        // Al soltar la tecla, vuelve al tama�o normal
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
        // Hace que la ca�da sea m�s r�pida (mejor sensaci�n de juego)
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
        // Alarga el salto del personaje cuando mantienes pulsado el bot�n de salto.
        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            // Pero hasta cierto l�mite.
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
                // Pasado este l�mite: empiezas a caer.
                isJumping = false;
            }
        }
    }

    private void ReleaseJump()
    {
        // Si suelta el bot�n de salto en el aire
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