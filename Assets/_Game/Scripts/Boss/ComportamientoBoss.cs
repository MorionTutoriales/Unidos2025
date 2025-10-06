using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BossBrain))]

public abstract class ComportamientoBoss : MonoBehaviour
{
    protected NavMeshAgent agente;
    protected Animator  animaciones;
    BossBrain           brain;
    public EstadosBoss  estadoEnemigo;
    public bool         dbujarGizmos;
    public float        modificadorDaño;

    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        brain = GetComponent<BossBrain>();
        animaciones = GetComponentInChildren<Animator>();
        brain.RegistrarComportamiento(this);
        Inicializar();
    }
    private void Update()
    {
        UpdateObligao();
    }

    public virtual void UpdateObligao()
    {

    }

    public void ActivarAgente()
    {
        agente.enabled = true;
    }

    public abstract void Inicializar();
    public abstract void Actualizar();
    public abstract void Reiniciar();
}
