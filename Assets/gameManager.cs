using UnityEngine;
using UnityEngine.Events;
public class GameManager : MonoBehaviour
{
    #region singleton
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
    public UnityEvent onGameOver = new UnityEvent();

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
        isPlaying = true;
    }

    public void LaunchGameover()
    {
        onGameOver.Invoke();
        score = 0;
        isPlaying = false;
    }

    public string RoundScoreToInt()
    {
        return Mathf.RoundToInt(score).ToString();
    }

}
