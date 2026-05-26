using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    // Prefabs de obstáculos que pueden aparecer
    [SerializeField] private GameObject[] obstaclePrefab;

    // Objeto padre donde se guardan los obstáculos en la jerarquía
    [SerializeField] private Transform obstacleParent;

    // Tiempo entre spawns
    public float spawnRate = 2f;

    // Velocidad base de los obstáculos
    public float obstacleSpeed = 3f;

    [Range(0, 2)] public float dificultadspwn = 1f;
    [Range(0, 2)] public float dificultadvelo = 0.5f;

    public float minSpawnRate = 0.3f;

    private float currentSpawnRate;
    private float currentObstacleSpeed;

    private float spawnTimer;
    private float tiempoVivo = 1f;

    private void Start()
    {
        // Eventos del GameManager
        GameManager.instance.onGameOver.AddListener(ClearObstacle);
        GameManager.instance.onPlay.AddListener(ResetValues);

        ResetValues();
    }

    private void Update()
    {
        // Solo funciona si el juego está en marcha
        if (!GameManager.instance.isPlaying)
            return;

        tiempoVivo += Time.deltaTime;

        CalcularDificultad();
        SpawnLoop();
        ActualizarVelocidadObstaculos();
    }

    private void SpawnLoop()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnRate)
        {
            Spawn();
            spawnTimer = 0f;
        }
    }

    private void Spawn()
    {
        // Selecciona un prefab aleatorio
        GameObject obstacleToSpawn =
            obstaclePrefab[Random.Range(0, obstaclePrefab.Length)];

        // Instancia el obstáculo en la escena
        GameObject newObstacle =
            Instantiate(obstacleToSpawn, transform.position, Quaternion.identity);

        // Lo asigna al padre
        newObstacle.transform.SetParent(obstacleParent);

        // Le da velocidad inicial
        Rigidbody2D rb = newObstacle.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.left * currentObstacleSpeed;
    }

    private void ActualizarVelocidadObstaculos()
    {
        // Recorre todos los obstáculos activos
        foreach (Transform child in obstacleParent)
        {
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.left * currentObstacleSpeed;
            }
        }
    }

    private void CalcularDificultad()
    {
        // Aumenta la dificultad con el tiempo
        currentSpawnRate =
            spawnRate / Mathf.Pow(tiempoVivo, (float)(1.5 * dificultadspwn));

        currentSpawnRate =
            Mathf.Clamp(currentSpawnRate, minSpawnRate, spawnRate);

        currentObstacleSpeed =
            obstacleSpeed * Mathf.Pow(tiempoVivo, dificultadvelo);
    }

    private void ResetValues()
    {
        // Reinicia valores al empezar partida
        tiempoVivo = 1f;
        currentSpawnRate = spawnRate;
        currentObstacleSpeed = obstacleSpeed;
        spawnTimer = 0f;
    }

    private void ClearObstacle()
    {
        // Borra todos los obstáculos al terminar la partida
        foreach (Transform child in obstacleParent)
        {
            Destroy(child.gameObject);
        }
    }
}