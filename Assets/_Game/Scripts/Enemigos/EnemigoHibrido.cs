using UnityEngine;
using UnityEngine.AI;

public class EnemigoHibrido : EnemigoBase
{
    [Header("Configuracion de Enemigo Hibrido")]
    public float rangoVision = 3f;
    public float distanciaDetencion = 0.6f;
    public float tiempoEntreAtaques = 1.5f;
    private float temporizadorAtaque = 0f;
    public Animator animator;

    [Header("Movimiento")]
    public float radioPatrulla = 5f;
    public float tiempoEsperaPatrulla = 2f;
    private Vector3 posicionInicial;
    private Vector3 destinoPatrulla;
    private float temporizadorPatrulla = 0f;

    [Header("Tipo de Atacante")]
    public bool aDistancia;
    public GameObject proyectilPrefab;
    public Transform puntoDisparo;

    private Transform jugador;
    private NavMeshAgent agente;
    private bool persiguiendo = false;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
        agente = GetComponent<NavMeshAgent>();

        if (agente == null)
        {
            Debug.LogWarning($"{gameObject.name} no tiene NavMeshAgent, se agregará automáticamente.");
            agente = gameObject.AddComponent<NavMeshAgent>();
        }

        agente.speed = velocidad;
        agente.angularSpeed = 360f;
        agente.acceleration = 20f;
        agente.stoppingDistance = distanciaDetencion;
        agente.autoBraking = true;

        posicionInicial = transform.position;
        ElegirNuevoDestinoPatrulla();
    }

    private void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (!persiguiendo && distancia <= rangoVision)
            persiguiendo = true;

        if (persiguiendo)
            ComportamientoPersecucion(distancia);
        else
            ComportamientoPatrulla();
    }

    private void ComportamientoPatrulla()
    {
        if (Vector3.Distance(transform.position, destinoPatrulla) > 0.3f)
        {
            if (!agente.pathPending)
                agente.SetDestination(destinoPatrulla);
        }
        else
        {
            agente.isStopped = true;
            temporizadorPatrulla -= Time.deltaTime;

            if (temporizadorPatrulla <= 0f)
            {
                ElegirNuevoDestinoPatrulla();
                agente.isStopped = false;
            }
        }
    }

    private void ComportamientoPersecucion(float distancia)
    {
        if (distancia > distanciaDetencion)
        {
            agente.isStopped = false;
            agente.SetDestination(jugador.position);
        }
        else
        {
            agente.isStopped = true;
            IntentarAtacar();
        }
    }

    private void ElegirNuevoDestinoPatrulla()
    {
        Vector2 puntoAleatorio = Random.insideUnitCircle * radioPatrulla;
        destinoPatrulla = new Vector3(posicionInicial.x + puntoAleatorio.x, posicionInicial.y, posicionInicial.z + puntoAleatorio.y);
        temporizadorPatrulla = tiempoEsperaPatrulla;
    }

    private void IntentarAtacar()
    {
        temporizadorAtaque -= Time.deltaTime;

        if (temporizadorAtaque <= 0f)
        {
            Atacar(jugador.gameObject);
            temporizadorAtaque = tiempoEntreAtaques;
        }
    }
    void CausarDamage()
    {
        GameManager.singleton.RestarVida(damage);
    }
    public override void Atacar(GameObject objetivo)
    {
        if (!aDistancia)
        {
            animator?.SetTrigger("atacar");
            Invoke("CausarDamage", 0.3f);
        }
        else
        {
            if (proyectilPrefab != null && puntoDisparo != null)
            {
                puntoDisparo.LookAt(objetivo.transform.position);
                GameObject bala = Instantiate(proyectilPrefab, puntoDisparo.position, puntoDisparo.rotation);

                Rigidbody rb = bala.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = puntoDisparo.forward * 10f;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDetencion);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? posicionInicial : transform.position, radioPatrulla);
    }
}
