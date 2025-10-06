using UnityEngine;

public class ComMuerto : ComportamientoBoss
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
        animaciones.SetTrigger("morir");
    }
}
