using UnityEngine;

public class EnemigoEstatico : EnemigoBase
{
    [Header("Configuracion de Enemigo Estático")]
    public float rangoVision = 10f; // Rango de deteccion
    public GameObject proyectilPrefab; // proyectil a instanciar
    public Transform puntoDisparo; // Punto desde donde sale el proyectil
    public float tiempoEntreDisparos = 2f; // Tiempo configurable entre disparos
    public Animator animator;

    private float temporizadorDisparo = 0f;
    private Transform jugador;

    private void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform; // Buscamos al jugador en la scena
    }

    private void Update()
    {
        if (jugador == null) return;

        // Reducir el temporizador cada frame
        temporizadorDisparo -= Time.deltaTime;

        // Si el jugador esta dentro del rango de visión y el temporizador permite disparar
        if (Vector3.Distance(transform.position, jugador.position) <= rangoVision)
        {
            transform.LookAt(GameManager.singleton.jugador.transform.position);
            if (temporizadorDisparo <= 0f)
            {
                Atacar(jugador.gameObject);
                temporizadorDisparo = tiempoEntreDisparos; // Reinicia el intervalo
            }
        }
    }
    public void Morir()
    {
        this.enabled = false;
        Destroy(gameObject, 5);
        animator.SetTrigger("morir");
    }

    public void Instanciar()
    {
        // Instanciamos el proyectil
        GameObject bala = Instantiate(proyectilPrefab, puntoDisparo.position, puntoDisparo.rotation);

        Rigidbody rb = bala.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = puntoDisparo.forward * 10f;
        }
    }

    /// <summary>
    /// Metodo sobreescrito para atacar el objetivo
    /// </summary>
    /// <param name="objetivo"> El jugador </param>
    public override void Atacar(GameObject objetivo)
    {
        // Disparamos al jugador
        if (proyectilPrefab != null && puntoDisparo != null)
        {
            // Hacemos que el punto de disparo mire al jugador
            transform.LookAt(objetivo.transform.position);
            puntoDisparo.LookAt(objetivo.transform.position);
            animator?.SetTrigger("atacar");
            
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja el rango de visión cuando el enemigo esté seleccionado en el editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);
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
