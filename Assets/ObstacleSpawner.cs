using System.IO.IsolatedStorage;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
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
    public float obstacleDeletingOffser = 10.0f;

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
        CheckObstacleCameraVisibility();
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

    private void CheckObstacleCameraVisibility()
    {
        for (int i = 0; i < obstacleParent.childCount; i++)
        {
            Transform obj = obstacleParent.GetChild(i);
            Rect camRect = GetCameraVisibleRect();
            
            if (obj.position.x < (camRect.x - obstacleDeletingOffser))
            {
                print("BORRAME");
                Destroy(obj.gameObject);
                print("Borrado");
            }

        }
    }

    private Vector3 DebugViewportRect(Rect viewport)
    {
        Debug.Log($"Viewport X: {viewport.x}, Y: {viewport.y}");
        Debug.Log($"Viewport Width: {viewport.width}, Height: {viewport.height}");

        
        return Vector3.zero;
    }

    /// <summary>
    /// Devuelve el rect�ngulo visible por la c�mara 2D en base a las coordenadas del mundo.
    /// </summary>
    /// <returns>
    /// Rectangulo del area visible de la c�mara 2D.
    /// </returns>
    public Rect GetCameraVisibleRect()
    {
        // Calculo la altura visible de la c�mara 2D (2 * cam.orthographicSize).
        Camera cam = Camera.main;
        // En vista ortografica, la c�mara devuelve la mitad de su tama�o.
        float height = 2f * cam.orthographicSize;

        // Calculo el ancho visible de la c�mara 2D (altura * su Aspect Ratio).
        float width = height * cam.aspect;

        // Obtengo la posici�n 3D de la c�mara.
        Vector3 cameraPos = cam.transform.position;

        // Calculo el punto de origen (esquina superior-izquierda) de la zona visible de la c�mara.
        float left = cameraPos.x - width / 2f;
        float top = cameraPos.y - height / 2f;

        return new Rect(left, top, width, height);
    }
}