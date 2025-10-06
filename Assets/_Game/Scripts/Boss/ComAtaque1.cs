using NUnit.Framework.Interfaces;
using UnityEngine;

public class ComAtaque1 : ComportamientoBoss
{
    public float distancia;

    Vector3 p1, p2;

    public float velocidad, t, velRotacion;
    bool a1;

    public override void Actualizar()
    {
        t +=  Time.deltaTime*velocidad;
        transform.Rotate(Vector3.up*Time.deltaTime* velRotacion * (a1?1:-1));
        transform.position  = Vector3.Lerp(p1, p2, t);
        if (t>1) {
            RecalcularPuntos();
            t=0;
            a1 = !a1;
        }
    }

    public override void Inicializar()
    {

    }

    private void OnDrawGizmosSelected()
    {
        if(!dbujarGizmos)return;
        Gizmos.DrawWireSphere(p1 + Vector3.up, 0.2f);
        Gizmos.DrawWireSphere(p2 + Vector3.up, 0.2f);
    }

    void RecalcularPuntos()
    {

        Vector3 d = GameManager.singleton.jugador.position - new Vector3(transform.position.x, transform.position.y, transform.position.z);
        d.y = 0;
        p1 = transform.position;
        p2 = GameManager.singleton.jugador.position + d.normalized * distancia;

    }

    public override void Reiniciar()
    {
        RecalcularPuntos();
        t = 0;
        animaciones.SetTrigger("baile1");
        agente.SetDestination(transform.position);
    }
}
