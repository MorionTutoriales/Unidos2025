using UnityEngine;
using UnityEngine.AI;

public class ComSeguir : ComportamientoBoss
{
    public override void Actualizar()
    {

        agente.SetDestination(GameManager.singleton.jugador.position);
    }

    public override void Inicializar()
    {
    }

    public override void Reiniciar()
    {
        animaciones.SetTrigger("caminar");
    }
}
