using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Events;
public class GameManager : MonoBehaviour
{
    #region GameManager singleton.
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public float score = 0f;
    public bool isPlaying = false;
    public Data data;

    public UnityEvent onGameOver = new UnityEvent();

    private void Start()
    {
        // Cargamos el record guardado en disco.
        string loadedData = SaveSystem.Load("save");

        if (loadedData != null)
        {
            data = JsonUtility.FromJson<Data>(loadedData);
        }
        else
        {
            // ¿No hay record guardado?; pues se crea uno nuevo.
            data = new Data();
        }
    }

    public void Update()
    {
        if (isPlaying)
        {
            // Cada segundo que pasa es un punto, entendido.
            score += Time.deltaTime;
        }

    }

    public UnityEvent onPlay = new UnityEvent();

    public void StartGame()
    {
        onPlay.Invoke();
        score = 0;
        isPlaying = true;
    }

    public void LaunchGameover()
    {
        // ¡Lograste hacer un nuevo record!
        if (data.highScore < score)
        {
            data.highScore = score;

            // Guardamos record en el disco.
            string saveData = JsonUtility.ToJson(data);
            SaveSystem.Save("save", saveData);
        }
        isPlaying = false;

        onGameOver.Invoke();
    }
}
