using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Texto que muestra los puntos durante la partida
    /// </summary>
    [SerializeField] private TextMeshProUGUI scoreText;

    /// <summary>
    /// UI del men� inicial (pantalla de inicio)
    /// </summary>
    [SerializeField] private GameObject startmenuUI;

    /// <summary>
    /// UI que se muestra cuando el jugador pierde (Game Over)
    /// </summary>
    [SerializeField] private GameObject gameOverUI;

    /// <summary>
    /// Texto que muestra la puntuaci�n final en Game Over
    /// </summary>
    [SerializeField] TextMeshProUGUI gameOverScoreUI;

    /// <summary>
    /// Texto que muestra el r�cord guardado
    /// </summary>
    [SerializeField] TextMeshProUGUI gameOverHighscoreUI;

    /// <summary>
    /// Referencia al GameManager (gestiona el estado del juego)
    /// </summary>
    private GameManager gm;

    private void Start()
    {
        // Intentamos obtener el GameManager existente en la escena
        // primero usando el singleton, y si no existe, lo busca en la escena
        gm = GameManager.instance ?? FindAnyObjectByType<GameManager>();

        // Si el texto de score no est� asignado en el inspector,
        // intenta encontrar uno autom�ticamente en la escena
        scoreText = scoreText ?? FindAnyObjectByType<TextMeshProUGUI>();

        // Nos suscribimos al evento de Game Over para actualizar la UI
        gm.onGameOver.AddListener(UIGameOver);
    }

    private void Update()
    {
        // Actualiza el texto del score cada frame mientras el juego corre
        scoreText.text = ScoreUtils.RoundScoreToInt(gm.score) ?? "0";
    }

    /// <summary>
    /// Permite cambiar el score manualmente desde otros scripts si se necesita
    /// </summary>
    public void SetScore(string score) => scoreText.text = score;

    /// <summary>
    /// Se ejecuta cuando ocurre Game Over
    /// Activa la UI de derrota y muestra datos finales
    /// </summary>
    private void UIGameOver()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);

        if (gameOverScoreUI != null)
            gameOverScoreUI.text = "Puntos: " + ScoreUtils.RoundScoreToInt(gm.score);

        if (gameOverHighscoreUI != null)
            gameOverHighscoreUI.text = "Record: " + ScoreUtils.RoundScoreToInt(gm.data.highScore);
    }

    /// <summary>
    /// Bot�n de UI para iniciar la partida
    /// </summary>
    public void playButtonHandler()
    {
        gm.StartGame();
    }

    [SerializeField] private GameObject shopUI;

    // Abre la UI de la tienda y cierra las UIs principales si est�n abiertas
    public void shopButtonHandler()
    {
        // Asegurar que la instancia de skiltree no quede desactivada si est� dentro del startmenuUI
        var tree = skiltree.instance;
        if (tree == null)
        {
            // intentar lazy accessor si no hay instancia
            try { tree = skiltree.Instance; } catch { tree = null; }
        }

        if (tree != null && startmenuUI != null)
        {
            var treeGO = tree.gameObject;
            if (treeGO != null && treeGO.transform.IsChildOf(startmenuUI.transform))
            {
                // Separar del startmenu para que no se desactive al ocultar el men�
                treeGO.transform.SetParent(null);
                DontDestroyOnLoad(treeGO);
            }
        }

        // Asegurarse de que shopUI se abra aunque no estuviera referenciado en el Inspector
        if (shopUI == null)
        {
            // Intentar encontrar por nombre (activo)
            shopUI = GameObject.Find("ShopUI");
            if (shopUI == null)
            {
                // Buscar tambi�n objetos inactivos por coincidencia de nombre
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go.name.ToLower().Contains("shop"))
                    {
                        shopUI = go;
                        break;
                    }
                }
            }
        }

        if (shopUI != null)
        {
            shopUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UIManager.shopButtonHandler: shopUI no asignado y no se encontr� objeto con 'shop' en su nombre.");
        }

        // Cerrar otras UIs principales para evitar solapamiento (startmenu puede seguir visible si contiene skiltree)
        if (startmenuUI != null)
        {
            // Si skiltree estaba dentro, startmenuUI seguir� siendo desactivado porque el objeto fue reparentado
            startmenuUI.SetActive(false);
        }

        if (gameOverUI != null) gameOverUI.SetActive(false);
    }
}
