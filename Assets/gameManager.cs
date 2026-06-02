using NUnit.Framework.Interfaces;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public enum DashState
    {
        Available,
        Using,
        Waiting
    }

    public const int BASE_LIFE = 1;

    #region GameManager singleton
    // Singleton: asegura que solo exista un GameManager en toda la partida
    public static GameManager instance;

    private void Awake()
    {
        // Si no existe instancia, esta se convierte en la principal
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // no se destruye al cambiar de escena
        }
        else
        {
            Destroy(gameObject); // evita duplicados
        }
    }
    #endregion

    [SerializeField] private GameObject playerCharacter;
    private SpriteRenderer spriteRender;

    // Score actual de la partida
    public float score = 0f;

    public float gameplayVelocity = 5f;
    public float velocityMultiplier = 1f;

    public float dashingTime = 5f;
    public float cooldownTime = 5f;

    public int extraLife = 2;
    private int currentLife = 1;

    public bool invencible = false;
    public int CurrentLife { get { return currentLife; } }
    private bool isPlayerDamagable = true;

    // Indica si el juego está activo o no
    public bool isPlaying = false;

    // Datos guardados (como el high score)
    public Data data;

    public DashState currentDashState = DashState.Available;
    public DashState lastDashState = DashState.Available;

    // Evento que se dispara cuando el jugador pierde
    public UnityEvent onGameOver = new UnityEvent();

    private void Start()
    {
        // Cargamos el archivo guardado del disco
        string loadedData = SaveSystem.Load("save");

        if (loadedData != null)
        {
            // Convertimos el JSON en objeto Data
            data = JsonUtility.FromJson<Data>(loadedData);
        }
        else
        {
            // Si no hay guardado, creamos uno nuevo
            data = new Data();
        }

        spriteRender = playerCharacter.GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        // Mientras el juego esté activo, el score aumenta con el tiempo.
        if (isPlaying && currentLife > 0)
        {
            score += gameplayVelocity * velocityMultiplier * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.LeftControl) && (invencible == false) && currentDashState == DashState.Available)
            {
                currentDashState = DashState.Using;
            }

            if (currentDashState != lastDashState)
            {
                ChangeDashState(currentDashState);
                lastDashState = currentDashState;
            }
        }

        
    }

    // Evento que se dispara al empezar a jugar
    public UnityEvent onPlay = new UnityEvent();

    public void StartGame()
    {
        // Avisamos a otros scripts de que el juego empieza
        onPlay.Invoke();

        // Reiniciamos score
        score = 0;
        currentLife = BASE_LIFE + extraLife;

        // Activamos estado de juego
        isPlaying = true;
    }

    public void LaunchGameover()
    {
        // Si el score actual supera el récord, lo guardamos
        if (data.highScore < score)
        {
            data.highScore = score;

            // Guardamos en disco
            string saveData = JsonUtility.ToJson(data);
            SaveSystem.Save("save", saveData);
        }

        // El juego se detiene
        isPlaying = false;

        // Avisamos a otros scripts de que terminó la partida
        onGameOver.Invoke();
    }

    public void DamagePlayer()
    {
        // Estas con i-frames activados.
        if (!isPlayerDamagable)
            return;

        currentLife--;
        Debug.Log("Daño recibido. Vida actual: " + currentLife);

        // Activo los i-frames.
        StartCoroutine(DamageInvulneratibity());
    }

    IEnumerator DamageInvulneratibity()
    {
        isPlayerDamagable = false;
        // TODO: meter en una variable el tiempo de invencibilidad.
        yield return new WaitForSeconds(1);

        isPlayerDamagable = true;
    }

    IEnumerator EnableDashing()
    {
        Debug.Log("Dasheando.");
        currentDashState = DashState.Using;
        spriteRender.color = Color.yellow;
        invencible = true;
        // TODO: que el tiempo sea en una variable.
        yield return new WaitForSeconds(dashingTime);
        Debug.Log("Se acaba el dashing");
        spriteRender.color = Color.gray;
        currentDashState = DashState.Waiting;
        invencible = false;
    }

    IEnumerator CooldownDashing()
    {
        //Debug.Log("Cooldown dashing.");

        // TODO: que el tiempo sea en una variable.
        var cooldownMult = PlayerAbilities.instance.improveCooldownDash;
        yield return new WaitForSeconds(cooldownTime * cooldownMult);
        //Debug.Log("Dash recuperado");
        spriteRender.color = Color.white;
        currentDashState = DashState.Available;
    }

    public void ChangeDashState(DashState newDashState)
    {
        currentDashState = newDashState;

        switch (currentDashState)
        {
            case DashState.Available:
                // Hago algo.
                break;
            case DashState.Using:
                StartCoroutine(EnableDashing());
                break;
            case DashState.Waiting:
                StartCoroutine(CooldownDashing());
                break;
        }
    }
}