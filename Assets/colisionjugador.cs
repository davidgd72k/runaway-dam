using JetBrains.Annotations;
using UnityEngine;

public class ColisionJugador : MonoBehaviour
{
    private void Start()
    {
        // Se suscribe al evento de empezar partida para “revivir” al jugador
        GameManager.instance.onPlay.AddListener(Revivir);
    }

    private void Revivir()
    {
        // Reactiva el jugador cuando empieza una nueva partida
        gameObject.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el jugador choca con un objeto con tag "obstacle", pierde
        if (collision.transform.CompareTag("obstacle"))
        {
            // Desactiva al jugador
            gameObject.SetActive(false);

            // Lanza el Game Over en el GameManager
            GameManager.instance.LaunchGameover();
        }
    }
}