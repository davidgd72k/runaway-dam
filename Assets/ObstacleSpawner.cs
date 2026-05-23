using System.IO.IsolatedStorage;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    public float obstacleDeletingOffser = 10.0f;

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
    /// Devuelve el rectángulo visible por la cámara 2D en base a las coordenadas del mundo.
    /// </summary>
    /// <returns>
    /// Rectangulo del area visible de la cámara 2D.
    /// </returns>
    public Rect GetCameraVisibleRect()
    {
        // Calculo la altura visible de la cámara 2D (2 * cam.orthographicSize).
        Camera cam = Camera.main;
        // En vista ortografica, la cámara devuelve la mitad de su tamaño.
        float height = 2f * cam.orthographicSize;

        // Calculo el ancho visible de la cámara 2D (altura * su Aspect Ratio).
        float width = height * cam.aspect;

        // Obtengo la posición 3D de la cámara.
        Vector3 cameraPos = cam.transform.position;

        // Calculo el punto de origen (esquina superior-izquierda) de la zona visible de la cámara.
        float left = cameraPos.x - width / 2f;
        float top = cameraPos.y - height / 2f;

        return new Rect(left, top, width, height);
    }
}