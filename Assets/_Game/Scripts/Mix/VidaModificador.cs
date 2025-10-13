using UnityEngine;

public class VidaModificador : MonoBehaviour
{
    public bool destruir;
    public float cuanto;
    public GameObject preInstanciar;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.singleton.saludJugador.SumarVida(cuanto);
            if(destruir) Destroy(gameObject);
            if (preInstanciar != null) Instantiate(preInstanciar, transform.position, transform.rotation);
        }
    }
}
