using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBrain : MonoBehaviour
{
    public EstadoBossDirigido[] estadosSecuencia;

    public EstadosBoss estadoActual;
    public List<ComportamientoBoss> comportamientos;
    public GameObject slVida;

    public float distanciaDetectarJugador = 10;

    public bool activo;
    public bool vivo = true;

    private IEnumerator Start()
    {
        while (!activo)
        {
            yield return new WaitForSeconds(2);
            if ((Vector3.Distance(transform.position, GameManager.singleton.jugador.transform.position) < distanciaDetectarJugador))
            {
                Activar();
            }
        }
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
            activo = true;
            StartCoroutine(Estadizador());
            slVida.SetActive(true);
        }
        activo = true;

    }

    public void VictoriaBoss()
    {
        activo = false;
        CambiarEstado(EstadosBoss.Reposo);
    }
    public void RegistrarComportamiento(ComportamientoBoss boss)
    {
        comportamientos.Add(boss);
    }

    IEnumerator Estadizador()
    {
        yield return null;
        int i = 0;
        while (vivo && activo)
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

    public void Morir()
    {
        activo = false;
        CambiarEstado(EstadosBoss.Muerto);
        Invoke("CambiarEscena", 3);
    }

    void CambiarEscena()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Victoria");
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
    Ataque3,
    Muerto
}
