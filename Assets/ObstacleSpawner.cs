using UnityEngine;
using static obstacleName;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefab;
    [SerializeField] private Transform obstacleParent;

    public float spawnRate = 2f;
    public float obstacleSpeed = 3f;

    [Range(0, 2)] public float dificultadspwn = 1f;
    [Range(0, 2)] public float dificultadvelo = 0.5f;

    public float minSpawnRate = 0.3f;

    private float currentSpawnRate;
    private float currentObstacleSpeed;

    [SerializeField] private float proyectilSpeedMultiplier = 1.3f;

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

        ObstacleData prefabData =
            obstacleToSpawn.GetComponent<ObstacleData>();

        if (prefabData == null)
            return;

        Vector3 spawnPos = transform.position;

        // altura aleatoria según tipo
        switch (prefabData.obstacleType)
        {
            case ObstacleType.Proyectil:
                spawnPos.y += Random.Range(-2f, 2f);
                break;

            case ObstacleType.Pelota:
                spawnPos.y += Random.Range(1f, 4f); // empieza más arriba
                break;
        }

        GameObject newObstacle =
            Instantiate(obstacleToSpawn, spawnPos, Quaternion.identity);

        newObstacle.transform.SetParent(obstacleParent);

        Rigidbody2D rb = newObstacle.GetComponent<Rigidbody2D>();
        ObstacleData data = newObstacle.GetComponent<ObstacleData>();

        if (rb == null || data == null)
            return;

        switch (data.obstacleType)
        {
            case ObstacleType.Proyectil:

                rb.gravityScale = 0f;
                rb.linearVelocity =
                    Vector2.left *
                    (currentObstacleSpeed * proyectilSpeedMultiplier);

                break;

            case ObstacleType.Pelota:

                // CLAVE: física completa
                rb.gravityScale = 1f;
                rb.linearVelocity =
                    Vector2.left * currentObstacleSpeed;

                break;

            case ObstacleType.Spike:
            case ObstacleType.SpikeRow:

                rb.gravityScale = 0f;
                rb.linearVelocity =
                    Vector2.left * currentObstacleSpeed;

                break;
        }
        Debug.Log("SPAWN: " + data.obstacleType);
    }

    private void ActualizarVelocidadObstaculos()
    {
        foreach (Transform child in obstacleParent)
        {
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
            ObstacleData data = child.GetComponent<ObstacleData>();

            if (rb == null || data == null)
                continue;

            switch (data.obstacleType)
            {
                case ObstacleType.Proyectil:

                    rb.linearVelocity =
                        Vector2.left *
                        (currentObstacleSpeed * proyectilSpeedMultiplier);

                    break;

                case ObstacleType.Pelota:

                    // SOLO mantener velocidad horizontal
                    rb.linearVelocity =
                        new Vector2(-currentObstacleSpeed, rb.linearVelocity.y);

                    break;

                case ObstacleType.Spike:
                case ObstacleType.SpikeRow:

                    rb.linearVelocity =
                        Vector2.left * currentObstacleSpeed;

                    break;
            }
        }
    }

    private void CalcularDificultad()
    {
        currentSpawnRate =
            spawnRate / Mathf.Pow(tiempoVivo, 1.5f * dificultadspwn);

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