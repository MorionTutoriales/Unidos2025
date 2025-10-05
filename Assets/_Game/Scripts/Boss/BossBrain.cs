using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBrain : MonoBehaviour
{
    public EstadoBossDirigido[] estadosSecuencia;

    public EstadosBoss estadoActual;
    public List<ComportamientoBoss> comportamientos;

    public bool activo;
    public bool vivo = true;

    private void Start()
    {
        Activar();
    }

    void Update()
    {
        foreach (ComportamientoBoss comportamiento in comportamientos)
        {
            if (comportamiento.estadoEnemigo == estadoActual)
            {
                comportamiento.Actualizar();
            }
        }
        //if (!activo)
        //{
        //    Activar
        //}
    }

    void Activar()
    {
        if (!activo)
        {
            StartCoroutine(Estadizador());
        }
        activo = true;
    }

    public void RegistrarComportamiento(ComportamientoBoss boss)
    {
        comportamientos.Add(boss);
    }

    IEnumerator Estadizador()
    {
        int i = 0;
        while (vivo)
        {
            CambiarEstado(estadosSecuencia[i].estado);
            yield return new WaitForSeconds(estadosSecuencia[i].tiempo);
            i++;
            if (i>= estadosSecuencia.Length)
            {
                i= 0;
            }
        }
    }

    void CambiarEstado(EstadosBoss e)
    {
        comportamientos[0].ActivarAgente();
        estadoActual = e;
        foreach (ComportamientoBoss comportamiento in comportamientos)
        {
            if (comportamiento.estadoEnemigo == estadoActual)
            {
                comportamiento.Reiniciar();
            }
        }
    }
    
}

[System.Serializable]
public class EstadoBossDirigido
{
    public EstadosBoss estado;
    public float tiempo;
}
public enum EstadosBoss
{
    Reposo,
    Seguir,
    Ataque1,
    Ataque2,
    Ataque3
}
