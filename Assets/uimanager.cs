using UnityEngine;
using TMPro;
using System.Text;
using System;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Texto que muestra los puntos durante la partida
    /// </summary>
    [SerializeField] private TextMeshProUGUI scoreText;
    
    /// <summary>
    /// Texto que muestra las vidas actual del jugador.
    /// </summary>
    [SerializeField] private TextMeshProUGUI lifeText;

    /// <summary>
    /// Texto que contiene la velocidad actual de la partida.
    /// </summary>
    [SerializeField] private TextMeshProUGUI speedText;

    [SerializeField] private TextMeshProUGUI skillShopText;

    /// <summary>
    /// UI del menú inicial (pantalla de inicio)
    /// </summary>
    [SerializeField] private GameObject startmenuUI;

    /// <summary>
    /// UI que se muestra cuando el jugador pierde (Game Over)
    /// </summary>
    [SerializeField] private GameObject gameOverUI;

    /// <summary>
    /// Texto que muestra la puntuación final en Game Over
    /// </summary>

    /// <summary>
    /// Texto que muestra el record guardado
    /// </summary>

    private StringBuilder sbScore;
    private StringBuilder sbLife;
    private StringBuilder sbSpeed;
    private StringBuilder sbSkillPoints;



    /// <summary>
    /// Referencia al GameManager (gestiona el estado del juego)
    /// </summary>
    private GameManager gm;

    private void Start()
    {
        sbScore = new StringBuilder();
        sbLife = new StringBuilder();
        sbSpeed = new StringBuilder();
        sbSkillPoints = new StringBuilder();

        // Intentamos obtener el GameManager existente en la escena
        // primero usando el singleton, y si no existe, lo busca en la escena
        gm = GameManager.instance ?? FindAnyObjectByType<GameManager>();

        // Si el texto de score no est� asignado en el inspector,
        // intenta encontrar uno autom�ticamente en la escena
        scoreText = scoreText ?? FindAnyObjectByType<TextMeshProUGUI>();
        lifeText = lifeText ?? FindAnyObjectByType<TextMeshProUGUI>();
        skillShopText = skillShopText ?? FindAnyObjectByType<TextMeshProUGUI>();

        // Nos suscribimos al evento de Game Over para actualizar la UI
        gm.onGameOver.AddListener(UIGameOver);
    }

    private void Update()
    {
        // Evito llenar la memoria de strings únicos.
        // String con los puntos actuales del jugador.
        sbScore.Clear();
        sbScore.Append(string.Format("{0}", ScoreUtils.RoundScoreToInt(gm.score) ?? "0"));

        // String con la vida actual del jugador.
        sbLife.Clear();
        sbLife.Append(string.Format("{0}", Convert.ToString(GameManager.instance.CurrentLife) ?? "???"));

        sbSpeed.Clear();
        sbSpeed.Append(string.Format("x{0}", Convert.ToString(GameManager.instance.velocityMultiplier) ?? "???"));

        sbSkillPoints.Clear();
        // Mostrar skill points como los puntos previos + el score actual
        double baseSP = skiltree.instance != null ? skiltree.instance.SkillPoints : 0.0;
        double scoreValue = gm != null ? gm.score : 0.0;
        var totalSP = (int)Math.Floor(baseSP + scoreValue);
        sbSkillPoints.Append(string.Format("Skill points: {0}", totalSP));

        // Actualizo los textos por pantalla.
        scoreText.text = sbScore.ToString();
        lifeText.text = sbLife.ToString();
        speedText.text = sbSpeed.ToString();
        skillShopText.text = sbSkillPoints.ToString();
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
