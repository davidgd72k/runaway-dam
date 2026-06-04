using NUnit.Framework.Interfaces;
using System.Collections;
using TMPro;
using UnityEngine.UI;
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
    public int coins = 0;

    public float gameplayVelocity = 5f;
    public float velocityMultiplier = 1f;

    public float dashingTime = 2f;
    public float cooldownTime = 10f;
    public float mejorascd = 0f;

    [Header("UI References")]
    // Asignar en el Inspector: GameObject que contiene un componente Button
    public GameObject botonMejora;
    private Button botonMejoraButton;

    // Contador sencillo de mejoras controlado por el botón (0..5)
    [Header("Simple Upgrades")]
    public int mejorascdCount = 0;

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

        // Suscribirse a cambios en el skill tree para sincronizar variables derivadas.
        // Usar el accessor Instance para forzar creación/búsqueda si no existe aún.
        var tree = skiltree.instance ?? (skiltree.Instance);
        if (tree != null)
        {
            // Registrar listener
            tree.onDerivedStatsChanged.AddListener(ApplySkillDerivedStats);
            // Sincronizar inicialmente
            ApplySkillDerivedStats();
            // Marcar que ya nos hemos suscrito para evitar suscripciones duplicadas
            subscribedToSkiltree = true;
        }

        // Configurar listener del boton de mejora si se ha asignado en el Inspector
        if (botonMejora != null)
        {
            botonMejoraButton = botonMejora.GetComponent<Button>();
            if (botonMejoraButton != null)
            {
                // Asegurar suscripción única: remover antes de añadir
                botonMejoraButton.onClick.RemoveListener(OnBotonMejoraPressed);
                botonMejoraButton.onClick.AddListener(OnBotonMejoraPressed);
            }
        }
    }

    public void Update()
    {
        // Mientras el juego esté activo, el score aumenta con el tiempo.
        if (isPlaying && currentLife > 0)
        {
            // El score ahora representa la cantidad de monedas recogidas.
            // Asignar el score al número de monedas para mantener consistencia.
            score = coins;

            if (Input.GetKeyDown(KeyCode.LeftControl) && (invencible == false) && currentDashState == DashState.Available && canUseShield)
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
    // Intento de suscripción perezosa: si no hay instancia de skiltree al Start, suscribirse cuando aparezca
    private bool subscribedToSkiltree = false;
    private void LateUpdate()
    {
        if (!subscribedToSkiltree)
        {
            var tree = skiltree.instance ?? (skiltree.Instance);
            if (tree != null)
            {
                tree.onDerivedStatsChanged.AddListener(ApplySkillDerivedStats);
                ApplySkillDerivedStats();
                subscribedToSkiltree = true;
            }
        }
    }
    // Lee skiltree y aplica las variables derivadas al GameManager
    private void ApplySkillDerivedStats()
    {
        var tree = skiltree.instance;
        if (tree == null) return;

        Debug.Log($"[GameManager] ApplySkillDerivedStats called: shieldCooldownUpgrades={tree.shieldCooldownUpgrades}");

        // Simplemente leer los valores ya calculados en skiltree
        canUseShield = tree.shieldUnlocked;
        canDoubleJump = tree.doubleJumpUnlocked;

        // Copiar contadores de mejoras al GameManager
        extraLife = tree.extraLifeUpgrades;

        // Calcular reducción total del cooldown a partir de las mejoras
        var reduction = (float)tree.shieldCooldownUpgrades * cooldownReductionPerUpgrade;
        // Almacenar reducción para uso informativo (segundos reducidos)
        mejorascd = Mathf.Clamp(reduction, 0f, cooldownTime - 0.1f);
        // Calcular tiempo efectivo de espera entre dashes: cooldownTime - reduction
        shieldCooldownWait = Mathf.Max(0.1f, cooldownTime - mejorascd);
    }

    // Método público para forzar la sincronización desde otros scripts
    public void RefreshSkillDerivedStats()
    {
        ApplySkillDerivedStats();
    }

    private void OnDestroy()
    {
        var tree = skiltree.instance;
        if (tree != null)
        {
            tree.onDerivedStatsChanged.RemoveListener(ApplySkillDerivedStats);
        }

        if (botonMejoraButton != null)
        {
            botonMejoraButton.onClick.RemoveListener(OnBotonMejoraPressed);
        }
    }

    // Handler del boton: incrementa un int simple hasta 5 (sin dependencias)
    private void OnBotonMejoraPressed()
    {
        mejorascd += 5;
        Debug.Log($"[GameManager] BotonMejora pressed: mejorascdCount={mejorascdCount}");
    }

    // Evento que se dispara al empezar a jugar
    public UnityEvent onPlay = new UnityEvent();

    public void StartGame()
    {
        // Avisamos a otros scripts de que el juego empieza
        onPlay.Invoke();

        // Reiniciamos score
        score = 0;
        coins = 0;
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
        Debug.Log("escudo.");
        currentDashState = DashState.Using;
        spriteRender.color = Color.yellow;
        invencible = true;
        // TODO: que el tiempo sea en una variable.
        yield return new WaitForSeconds(dashingTime);
        Debug.Log("Se acaba el escudo");
        spriteRender.color = Color.gray;
        currentDashState = DashState.Waiting;
        invencible = false;
    }

    IEnumerator CooldownDashing()
    {
        Debug.Log("Cooldown dashing.");
        // Ajustar con el valor bruto de mejoras desde skiltree si está disponible
        var tree = skiltree.instance;
        var waitTime = Mathf.Max(0.1f, shieldCooldownWait - mejorascd);
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Dash recuperado");
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

    // Permisos/estados derivados del skill tree
    public bool canUseShield = false;
    public bool canDoubleJump = false;

    public float cooldownReductionPerUpgrade = 1f;
    // Tiempo efectivo de espera entre escudos después de aplicar mejoras
    private float shieldCooldownWait = 1f;
}