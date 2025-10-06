using UnityEngine;

public class EnemigoHibrido : EnemigoBase
{
    [Header("Configuracion de Enemigo Hibrido")]
    public float rangoVision = 3f; // Rango de vision del enemigo
    public float distanciaDetencion = 0.6f; // Distancia a la cual ataca
    public float tiempoEntreAtaques = 1.5f; // Intervalo entre ataques
    private float temporizadorAtaque = 0f;

    [Header("Tipo de Atacante")]
    public bool aDistancia; // Para indicar si el enemigo es cuerpo a cuerpo o a distancia
    public GameObject proyectilPrefab; // Solo si es a distancia
    public Transform puntoDisparo; // Solo si es a distancia

    private Transform jugador;
    private Rigidbody rb;
    private bool persiguiendo = false;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform; // Buscamos al jugador en la scena

        if (rb == null)
        {
            Debug.LogWarning($"{gameObject.name} no tiene un Rigidbody asignado.");
        }
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // evita que se caiga o gire raro  
    }

    private void FixedUpdate()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position); // Calculamos la distancia

        // Si detectamos
        if (!persiguiendo && distancia <= rangoVision)
        {
            persiguiendo = true;
        }

        if (persiguiendo)
        {
            // Si la distancia es mayor a la de detención, moverse hacia el jugador
            if (distancia > distanciaDetencion)
            {
                Vector3 direccion = (jugador.position - transform.position).normalized;
                rb.linearVelocity = direccion * velocidad;
            }
            else
            {
                // Sino nos detenemos y e intentamos atacar
                rb.linearVelocity = Vector3.zero;
                IntentarAtacar();
            }
        }
        else
        {
            rb.linearVelocity = Vector3.zero; // no moverse si no está persiguiendo
        }
    }

    /// <summary>
    /// Invocado para internar atacar al enemigo
    /// </summary>
    private void IntentarAtacar()
    {
        // Controla la frecuencia de ataque para no hacerlo en cada frame
        temporizadorAtaque -= Time.deltaTime;

        if (temporizadorAtaque <= 0f)
        {
            Atacar(jugador.gameObject);
            temporizadorAtaque = tiempoEntreAtaques;
        }
    }

    /// <summary>
    /// Metodo sobreescrito para atacar el objetivo
    /// </summary>
    /// <param name="objetivo"> El jugador </param>
    public override void Atacar(GameObject objetivo)
    {
        // Dañamos al jugador cuerpo a cuerpo
        if (!aDistancia)
        {
            GameManager.singleton.RestarVida(damage);
        }
        else
        {
            // Dañamos al jugador disparandole
            if (proyectilPrefab != null && puntoDisparo != null)
            {
                // Hacemos que el punto de disparo mire al jugador
                puntoDisparo.LookAt(objetivo.transform.position);

                // Instanciamos el proyectil
                GameObject bala = Instantiate(proyectilPrefab, puntoDisparo.position, puntoDisparo.rotation);

                Rigidbody rb = bala.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = puntoDisparo.forward * 10f;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el rango de visión en rojo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        // Dibuja el rango de detención en amarillo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDetencion);
    }

    private void OnTriggerEnter(Collider other)
    {
        ////Verificamos si el objeto que nos golpea es un proyectil del player
        //if (other.CompareTag("Proyectil"))
        //{
        //    // Obtenemos el componente del proyectil del jugador
        //    Proyectil proyectil = other.GetComponent<Proyectil>();
        //    if (proyectil != null)
        //    {
        //        RecibirDamage(proyectil.damage);
        //    }

        //    //destruir el proyectil despu�s del impacto
        //    Destroy(other.gameObject);
        //}
    }
}
