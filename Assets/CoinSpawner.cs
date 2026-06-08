using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;

    [SerializeField] private float minSpawnTime = 1f;
    [SerializeField] private float maxSpawnTime = 3f;

    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 2f;

    [SerializeField] private float minSpeed = 10f;
    [SerializeField] private float maxSpeed = 30f;

    private bool isStartedInvoke = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isPlaying && !isStartedInvoke)
        {
            // Empieza invocando una moneda.
            Invoke(nameof(SpawnCoin), Random.Range(minSpawnTime, maxSpawnTime));
            isStartedInvoke = true;
        }

    }

    private void SpawnCoin()
    {
        // Fija la posición de la moneda y la crea.
        Vector3 spawnPos = new Vector3(
            transform.position.x,
            Random.Range(minY, maxY),
            0f
        );

        Coin coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity).GetComponent<Coin>();
        coin.speed = Random.Range(minSpeed, maxSpeed);

        // Vuelve a llamarse a sí mismo para repetir este proceso.
        Invoke(nameof(SpawnCoin), Random.Range(minSpawnTime, maxSpawnTime));
    }
}
