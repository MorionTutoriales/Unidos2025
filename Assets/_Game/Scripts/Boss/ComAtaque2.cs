using UnityEngine;

public class ComAtaque2 : ComportamientoBoss
{
    public Transform pivoteSupayFalsos;
    public float radio = 5, velRotacion = 2;

    public Vector2 posY;

    float tiempo;
    Vector3 pos1,pos2;
    Vector3 npos1,npos2;
    float t = 0;

    public GameObject spy1, spy2;
    public override void Actualizar()
    {
        tiempo += Time.deltaTime * velRotacion;
        pos1 = transform.position +
               new Vector3(
                   Mathf.Sin(tiempo)* radio, 
                   Mathf.Lerp(posY.x, posY.y, t), 
                   Mathf.Cos(tiempo)* radio);
        pos2 = transform.position -
               new Vector3(
                   Mathf.Sin(tiempo) * radio,
                   -Mathf.Lerp(posY.x, posY.y, t),
                   Mathf.Cos(tiempo) * radio);

        npos1 = transform.position +
               new Vector3(
                   Mathf.Sin(tiempo + 0.1f) * radio,
                   Mathf.Lerp(posY.x, posY.y, t),
                   Mathf.Cos(tiempo + 0.1f) * radio);
        npos2 = transform.position -
               new Vector3(
                   Mathf.Sin(tiempo + 0.1f) * radio,
                   -Mathf.Lerp(posY.x, posY.y, t),
                   Mathf.Cos(tiempo + 0.1f) * radio);
        t += Time.deltaTime*2f;
        t = Mathf.Clamp(t, 0, 1);
        agente.SetDestination(transform.position);

    }

    public override void UpdateObligao()
    {
        t-= Time.deltaTime;
        t = Mathf.Clamp(t, 0, 1);
        pos1.y = Mathf.Lerp(posY.x, posY.y, t);
        pos2.y = Mathf.Lerp(posY.x, posY.y, t);
        spy1.transform.position = pos1;
        spy2.transform.position = pos2;
        spy1.transform.forward = Vector3.Lerp(spy1.transform.forward, npos1 -pos1, 0.5f);
        spy2.transform.forward = Vector3.Lerp(spy1.transform.forward, pos2 - npos2, 0.5f);
    }

    public override void Inicializar()
    {
    }

    public override void Reiniciar()
    {
        animaciones.SetTrigger("baile2");
    }

    private void OnDrawGizmosSelected()
    {
        if (!dbujarGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos1, 0.5f);
        Gizmos.DrawWireSphere(pos2, 0.5f);
    }
}
