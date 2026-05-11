using UnityEngine;
using TMPro;

public class uimanager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject startmenuUI;
    [SerializeField] private GameObject gameOverUI;

    private gameManager gm;

    // buscar componentes si no estan asignados
    private void Start()
    {
        gm = gameManager.instance ?? FindObjectOfType<gameManager>();
        scoreText = scoreText ?? FindObjectOfType<TextMeshProUGUI>();
        gm.onGameOver.AddListener(UIGameOver);
    }

    private void Update() => scoreText.text = gm?.scorebonito() ?? "0";

    public void SetScore(string score) => scoreText.text = score;

    private void UIGameOver()
    {
        gameOverUI.SetActive(true);
    }
    public void playButtonHandler()
    {
        gm.startgame();
    }
}
