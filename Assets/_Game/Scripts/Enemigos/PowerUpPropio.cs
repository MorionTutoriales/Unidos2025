using Unity.VisualScripting;
using UnityEngine;

public class PowerUpPropio : MonoBehaviour
{
    public float progresoAdicional = 5;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.singleton.SumarProgreso(progresoAdicional);
            Destroy(gameObject);
        }
    }
}
