using UnityEngine;

public class ObstacleBehavior : MonoBehaviour
{
   

    private void OnBecameInvisible()
    {
        print("Desaparezco");       
    }

    private void OnBecameVisible()
    {
        gameObject.SetActive(true);
        
    }
}
