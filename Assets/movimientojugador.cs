using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    private Animator animator; //Para campturar el componente Animator del Jugador
    // Referencia al Rigidbody del jugador
    [SerializeField] private Rigidbody2D rb;

    // Parte visual del jugador (para escalar al agacharse)
    [SerializeField] private Transform gfx;

    // Fuerza del salto inicial
    [SerializeField] private float jumpForce = 10f;

    // Capa del suelo para detectar si estï¿½ tocando el suelo
    [SerializeField] private LayerMask groundLayer;

    // Punto de comprobaciï¿½n del suelo (pies)
    [SerializeField] private Transform feetPos;

    // Distancia para detectar el suelo
    [SerializeField] private float groundDistance = 0.25f;

    // Tiempo mï¿½ximo para mantener el salto
    [SerializeField] private float jumpTime = 0.2f;

    // Fuerza adicional mientras se mantiene el salto
    [SerializeField] private float jumpForceHold = 20f;

    // Multiplicador de caï¿½da mï¿½s rï¿½pida
    [SerializeField] private float fallMultiplier = 2.5f;

    // Multiplicador para salto corto si sueltas el botï¿½n
    [SerializeField] private float lowJumpMultiplier = 2f;

    // Altura cuando el personaje se agacha
    [SerializeField] private float crouchHeight = 0.5f;

    private bool isGrounded;
    private bool isCrouching;
    private bool isJumping;
    private float jumpTimer;

    void Start()
    {
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        // Comprueba si el jugador estï¿½ tocando el suelo
        isGrounded = Physics2D.OverlapCircle(feetPos.position, groundDistance, groundLayer);

        // Ejecuta toda la lï¿½gica de movimiento
        Jump();
        HoldJump();
        ReleaseJump();
        Crouch();
        CalculateJumpPhysics();

        // Si el personaje no está en el suelo, reproducir animación de salto
        if (!isGrounded)
        {
            animator.SetBool("isjumping", true);
            // Asegurarse de que la animación de andar no se reproduzca en el aire
            }
        else
        {
            animator.SetBool("isjumping", false);
        }

        if(isCrouching)
        {
            animator.SetBool("iscrouching", true); //Si se está moviendo, reproduzco la animación
        }
        else
        {
            animator.SetBool("iscrouching", false); //Si no, la paro
        }
    }

    /// <summary>
    /// Controla el funcionamiento del salto del personaje.
    /// </summary>
    private void Jump()
    {
        // Salto inicial si estï¿½ en el suelo
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
        // Solo puedes agacharte cuando estï¿½ en el suelo usando la tecla SHIFT.
        if (isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, crouchHeight, gfx.localScale.z);
            isCrouching = true;
        }

        // * Recuperas tu tamaï¿½o original cuando...
        // > Saltas, aunque estes manteniendo pulsando el botï¿½n de agachado.
        if (isJumping && Input.GetKey(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
            isCrouching = false;

        }

        // Al soltar la tecla, vuelve al tamaï¿½o normal
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            gfx.localScale = new Vector3(gfx.localScale.x, 1f, gfx.localScale.z);
            isCrouching = false;

        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CalculateJumpPhysics()
    {
        // Hace que la caï¿½da sea mï¿½s rï¿½pida (mejor sensaciï¿½n de juego)
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
        // Alarga el salto del personaje cuando mantienes pulsado el botï¿½n de salto.
        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            // Pero hasta cierto lï¿½mite.
            if (jumpTimer > 0)
            {
                isJumping = true;
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y + jumpForceHold * Time.deltaTime
                );

                jumpTimer -= Time.deltaTime;
            }
            else
            {
                // Pasado este lï¿½mite: empiezas a caer.
                isJumping = false;
            }
        }
    }

    private void ReleaseJump()
    {
        // Si suelta el botï¿½n de salto en el aire
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