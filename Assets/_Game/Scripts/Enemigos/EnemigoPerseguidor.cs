using UnityEngine;

public class EnemigoPerseguidor : EnemigoBase
{
    [Header("Configuracion de Persecucion")]
    public float distanciaAtaque = 1.0f; // Distancia minima para atacar al jugador
    public float tiempoEntreAtaques = 1.5f; // Intervalo entre ataques

    private float temporizadorAtaque = 0f; // Temporizador de ataques
    private Transform jugador;
    private Rigidbody rb;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player")?.transform; // Buscamos al jugador en la scena

        if (rb == null) Debug.LogWarning($"{gameObject.name} no tiene un Rigidbody asignado.");
        rb = GetComponent<Rigidbody>();    
    }

    private void FixedUpdate()
    {
        if (jugador == null || rb == null) return;
        
        float distancia = Vector3.Distance(transform.position, jugador.position); // Calculamos la distancia entre este enemigo y el jugador

        if (distancia > distanciaAtaque)
        {
            // Si la distancia es mayor nos movemos hacia el jugador
            MoverHaciaJugador();
        }
        else
        {
            // Sino nos denetemos y atacamos
            DetenerMovimiento();
            IntentarAtacar();
        }
    }

    /// <summary>
    /// Invocado para movernos directamente hacia el player
    /// </summary>
    private void MoverHaciaJugador()
    {
        Vector3 direccion = (jugador.position - transform.position).normalized;

        // Usamos la velocidad heredada desde EnemigoBase
        rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime);

        // Hacer que mire hacia el jugador
        transform.LookAt(jugador);
    }

    /// <summary>
    /// Metodo para detener el movimiento del enemigo
    /// </summary>
    private void DetenerMovimiento()
    {
        rb.linearVelocity = Vector3.zero;
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
        // Dañamos al jugador
        GameManager.singleton.RestarVida(damage);
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

        //    //destruir el proyectil después del impacto
        //    Destroy(other.gameObject);
        //}
    }
}
