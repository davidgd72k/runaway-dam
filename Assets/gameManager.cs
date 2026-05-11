using UnityEngine;
using UnityEngine.Events;
public class gameManager : MonoBehaviour
{
    #region singleton
    public static gameManager instance;

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
            score += Time.deltaTime;
        }

    }
    public UnityEvent onPlay =new UnityEvent();
    public void startgame()
    {
        onPlay.Invoke();
        isPlaying = true;
    }
    public void gameover()
    {
        onGameOver.Invoke();
        score = 0;
        isPlaying = false;
    }
    public string scorebonito()
    {
        return Mathf.RoundToInt(score).ToString();
    }

}
