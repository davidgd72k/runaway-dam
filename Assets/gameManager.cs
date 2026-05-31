using NUnit.Framework.Interfaces;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public const int BASE_LIFE = 1;
    // Singleton: asegura que solo exista un GameManager en toda la partida
    public static GameManager instance;

    #region GameManager singleton
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

    // Score actual de la partida
    public float score = 0f;

    public int extraLife = 2;
    private int currentLife = 1;
    public int CurrentLife { get { return currentLife; } }
    private bool isPlayerDamagable = true;

    // Indica si el juego está activo o no
    public bool isPlaying = false;

    // Datos guardados (como el high score)
    public Data data;

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
    }

    public void Update()
    {
        // Mientras el juego esté activo, el score aumenta con el tiempo
        if (isPlaying && currentLife > 0)
        {
            score += Time.deltaTime;
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
        StartCoroutine(Invulneratibity());
    }

    IEnumerator Invulneratibity()
    {
        isPlayerDamagable = false;
        // TODO: meter en una variable el tiempo de invencibilidad.
        yield return new WaitForSeconds(1);
        isPlayerDamagable = true;
    }
}