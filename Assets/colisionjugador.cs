using JetBrains.Annotations;
using UnityEngine;

public class ColisionJugador : MonoBehaviour
{
    private void Start()
    {
        GameManager.instance.onPlay.AddListener(Revivir);
    }
    
    private void Revivir()
    {
        gameObject.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Chocar con un obstaculo activa el game-over.
        if (collision.transform.CompareTag("obstacle"))
        {
            gameObject.SetActive(false);
            GameManager.instance.LaunchGameover();
        }
    }
}
