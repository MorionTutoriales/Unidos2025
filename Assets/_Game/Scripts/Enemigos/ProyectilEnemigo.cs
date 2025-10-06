using UnityEngine;

public class ProyectilEnemigo : MonoBehaviour
{
    [Header("Configuraci�n del Proyectil")]
    public float velocidad = 1f; // Velocidad del proyectil
    public float damage = 5f; // Da�o del proyectil
    public float tiempoVida = 5f; // Tiempo de vida del proyectil en la scena

    private void Start()
    {
        Destroy(gameObject, tiempoVida); // Se destruye tras el tiempo de vida para limpiar memoria
    }

    private void Update()
    {
        // Movimiento hacia adelante en la direccion que fu� instanciado el proyectil
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si impacta con el jugador lo da�amos
        if (other.CompareTag("Player"))
        {
            GameManager.singleton.RestarVida(damage);
            Destroy(gameObject); // destruir la bala tras impactar
        }

        // Si impacta con el entorno u otro obst�culo
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstaculo"))
        {
            Destroy(gameObject);
        }
    }
}
