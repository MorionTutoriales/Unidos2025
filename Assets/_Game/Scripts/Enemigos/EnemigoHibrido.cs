using UnityEngine;

public class EnemigoHibrido : EnemigoBase
{
    [Header("Configuracion de Enemigo Hibrido")]
    public float rangoVision = 3f; // Rango de vision del enemigo
    public float distanciaDetencion = 0.6f; // Distancia a la cual ataca
    public float tiempoEntreAtaques = 1.5f; // Intervalo entre ataques
    private float temporizadorAtaque = 0f;

    [Header("Movimiento")]
    public float radioPatrulla = 5f;         // Qué tan lejos del punto inicial puede moverse
    public float tiempoEsperaPatrulla = 2f;  // Tiempo que espera antes de elegir otro punto
    private Vector3 posicionInicial;
    private Vector3 destinoPatrulla;
    private float temporizadorPatrulla = 0f;

    [Header("Tipo de Atacante")]
    public bool aDistancia; // Para indicar si el enemigo es cuerpo a cuerpo o a distancia
    public GameObject proyectilPrefab; // Solo si es a distancia
    public Transform puntoDisparo; // Solo si es a distancia

    private Transform jugador;
    private Rigidbody rb;
    private bool persiguiendo = false;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning($"{gameObject.name} no tiene Rigidbody, se agregará automáticamente.");
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Guardamos la posición inicial
        posicionInicial = transform.position;

        // Elegimos el primer destino aleatorio
        ElegirNuevoDestinoPatrulla();
    }

    private void FixedUpdate()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        // Si aún no está persiguiendo y detecta al jugador
        if (!persiguiendo && distancia <= rangoVision)
        {
            persiguiendo = true;
        }

        if (persiguiendo)
        {
            ComportamientoPersecucion(distancia);
        }
        else
        {
            ComportamientoPatrulla();
        }
    }

    /// <summary>
    /// Metodo que relaciona el modo patrulla
    /// </summary>
    private void ComportamientoPatrulla()
    {
        float distanciaDestino = Vector3.Distance(transform.position, destinoPatrulla);

        if (distanciaDestino > 0.3f)
        {
            // Mover hacia el destino
            Vector3 direccion = (destinoPatrulla - transform.position).normalized;
            rb.linearVelocity = direccion * velocidad * 0.5f; // más lento que la persecución
        }
        else
        {
            // Detenerse y esperar antes de elegir nuevo destino
            rb.linearVelocity = Vector3.zero;
            temporizadorPatrulla -= Time.deltaTime;

            if (temporizadorPatrulla <= 0f)
            {
                ElegirNuevoDestinoPatrulla();
            }
        }
    }

    /// <summary>
    /// Metodo para entrar en modo Modo Persecución o atacar segun sea el caso
    /// </summary>
    /// <param name="distancia"> Distancia hacia el jugador</param>
    private void ComportamientoPersecucion(float distancia)
    {
        if (distancia > distanciaDetencion)
        {
            Vector3 direccion = (jugador.position - transform.position).normalized;
            rb.linearVelocity = direccion * velocidad;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            IntentarAtacar();
        }
    }

    /// <summary>
    /// Metodo para aleatoriamente elegir un destino de desplazamiento cerca a su posicion inicial
    /// </summary>
    private void ElegirNuevoDestinoPatrulla()
    {
        // Elegir punto aleatorio cerca de la posición inicial
        Vector2 puntoAleatorio = Random.insideUnitCircle * radioPatrulla;
        destinoPatrulla = new Vector3(posicionInicial.x + puntoAleatorio.x, transform.position.y, posicionInicial.z + puntoAleatorio.y);
        temporizadorPatrulla = tiempoEsperaPatrulla;
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

        // Radio de patrullaje en verde
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? posicionInicial : transform.position, radioPatrulla);
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
