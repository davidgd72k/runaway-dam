using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Texto que muestra los puntos
    /// </summary>
    [SerializeField] private TextMeshProUGUI scoreText;
    /// <summary>
    /// Interfaz del menú al iniciar la partida.
    /// </summary>
    [SerializeField] private GameObject startmenuUI;
    /// <summary>
    /// Interfaz del menú de gameover.
    /// </summary>
    [SerializeField] private GameObject gameOverUI;

    private GameManager gm;

    private void Start()
    {
        // Busca los componentes por si los mismos no estan asignados.
        gm = GameManager.instance ?? FindAnyObjectByType<GameManager>();
        scoreText = scoreText ?? FindAnyObjectByType<TextMeshProUGUI>();
        gm.onGameOver.AddListener(UIGameOver);
    }

    private void Update()
    {
        scoreText.text = gm?.RoundScoreToInt() ?? "0";
    }

    public void SetScore(string score) => scoreText.text = score;

    private void UIGameOver()
    {
        gameOverUI.SetActive(true);
    }

    public void playButtonHandler()
    {
        gm.StartGame();
    }
}
