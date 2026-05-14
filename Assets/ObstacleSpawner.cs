using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    // TODO: ¿Hay alguna forma de no acumular en memoria todos los obstaculos que surgen?
    /// <summary>
    /// Prefabs de los obstaculos a aparecer.
    /// </summary>
    [SerializeField] private GameObject[] obstaclePrefab;
    /// <summary>
    /// Contiene todos los obstaculos que surgen en el juego.
    /// </summary>
    [SerializeField] private Transform obstacleParent;

    /// <summary>
    /// Ratio de aparición.
    /// </summary>
    public float spawnRate = 2f;
    /// <summary>
    /// Velocidad del obstaculo.
    /// </summary>
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
        GameManager.instance.onGameOver.AddListener(ClearObstacle);
        GameManager.instance.onPlay.AddListener(ResetValues);

        ResetValues();
    }

    private void Update()
    {
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
        GameObject obstacleToSpawn =
            obstaclePrefab[Random.Range(0, obstaclePrefab.Length)];

        GameObject newObstacle =
            Instantiate(obstacleToSpawn, transform.position, Quaternion.identity);

        newObstacle.transform.SetParent(obstacleParent);

        Rigidbody2D rb = newObstacle.GetComponent<Rigidbody2D>();

        rb.linearVelocity = Vector2.left * currentObstacleSpeed;
    }

    private void ActualizarVelocidadObstaculos()
    {
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
        currentSpawnRate =
            spawnRate / Mathf.Pow(tiempoVivo, (float)(1.5 * dificultadspwn));

        currentSpawnRate =
            Mathf.Clamp(currentSpawnRate, minSpawnRate, spawnRate);

        currentObstacleSpeed =
            obstacleSpeed * Mathf.Pow(tiempoVivo, dificultadvelo);
    }

    private void ResetValues()
    {
        tiempoVivo = 1f;

        currentSpawnRate = spawnRate;
        currentObstacleSpeed = obstacleSpeed;

        spawnTimer = 0f;
    }

    private void ClearObstacle()
    {
        foreach (Transform child in obstacleParent)
        {
            Destroy(child.gameObject);
        }
    }
}