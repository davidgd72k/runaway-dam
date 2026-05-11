using JetBrains.Annotations;
using UnityEngine;

public class colisionjugador : MonoBehaviour
{
    private void Start()
    {
        gameManager.instance.onPlay.AddListener(Revivir);
    }
    private void Revivir()
    {
        gameObject.SetActive(true);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.transform.CompareTag("obstacle"))
        {
            gameObject.SetActive(false);
            gameManager.instance.gameover();
        }
    }
}
