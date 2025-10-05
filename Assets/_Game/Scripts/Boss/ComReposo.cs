using UnityEngine;

public class ComReposo : ComportamientoBoss
{
    public override void Actualizar()
    {
        agente.SetDestination(transform.position);
    }

    public override void Inicializar()
    {
    }

    public override void Reiniciar()
    {
    }
}
