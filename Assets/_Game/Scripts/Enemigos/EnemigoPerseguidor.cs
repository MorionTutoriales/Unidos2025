using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemigoPerseguidor : EnemigoBase
{
    [Header("Configuración de Persecución")]
    public float distanciaAtaque = 1.0f;      // Distancia mínima para atacar al jugador
    public float tiempoEntreAtaques = 1.5f;   // Intervalo entre ataques
    public Animator animator;

    private float temporizadorAtaque = 0f;    // Temporizador de ataques
    private Transform jugador;
    private NavMeshAgent agente;
    public float bloqueo;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Intentamos obtener o agregar el NavMeshAgent
        agente = GetComponent<NavMeshAgent>();
        if (agente == null)
        {
            Debug.LogWarning($"{gameObject.name} no tiene NavMeshAgent, se agregará automáticamente.");
            agente = gameObject.AddComponent<NavMeshAgent>();
        }

        // Configuramos el agente según los valores del enemigo base
        agente.speed = velocidad;
        agente.angularSpeed = 360f;
        agente.acceleration = 20f;
        agente.stoppingDistance = distanciaAtaque;
        agente.autoBraking = true;
        agente.updateRotation = true; // para que mire hacia donde se mueve

        float distancia = Vector3.Distance(transform.position, jugador.position);
        bloqueo = Time.time + distancia*2;
        StartCoroutine(ReDistanciador());
    }

    IEnumerator ReDistanciador()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);
        while (distancia > 10)
        {
            distancia = Vector3.Distance(transform.position, jugador.position);
            yield return new WaitForSeconds(1);
        }
        bloqueo = 0;
    }

    private void Update()
    {
        
        if (jugador == null || Time.time < bloqueo) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > distanciaAtaque+0.2f)
        {
            // Si está lejos, perseguimos al jugador
            MoverHaciaJugador();
        }
        else
        {
            // Si está dentro del rango de ataque
            DetenerMovimiento();
            IntentarAtacar();
        }
    }

    /// <summary>
    /// Mueve el enemigo hacia el jugador usando el NavMeshAgent.
    /// </summary>
    private void MoverHaciaJugador()
    {
        if (!agente.pathPending)
        {
            transform.LookAt(GameManager.singleton.jugador.transform, Vector3.up);
            agente.isStopped = false;
            agente.SetDestination(jugador.position);
        }
    }

    /// <summary>
    /// Detiene el movimiento del enemigo.
    /// </summary>
    private void DetenerMovimiento()
    {
        if (agente.enabled)
        {
            agente.isStopped = true;
            agente.ResetPath();
        }
    }

    public void MorirTotal()
    {
        DetenerMovimiento();
        this.enabled = false;
        Destroy(gameObject, 5);
        animator.SetTrigger("morir");
    }

    /// <summary>
    /// Invocado para intentar atacar al jugador.
    /// </summary>
    private void IntentarAtacar()
    {
        temporizadorAtaque -= Time.deltaTime;

        if (temporizadorAtaque <= 0f)
        {
            animator?.SetTrigger("atacar");
            //Atacar(jugador.gameObject);
            temporizadorAtaque = tiempoEntreAtaques;
        }
    }

    /// <summary>
    /// Método sobreescrito para atacar al objetivo.
    /// </summary>
    public override void Atacar(GameObject objetivo)
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia < distanciaAtaque + 0.2f)
        {
            // Si está lejos, perseguimos al jugador
            GameManager.singleton.RestarVida(damage);
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }

    private void OnTriggerEnter(Collider other)
    {
        //// Si quieres reactivar la detección de proyectiles, puedes usar el código anterior aquí
    }
}
