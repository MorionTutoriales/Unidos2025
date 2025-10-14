using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Transform    jugador;
    public Salud        saludJugador;
    public Salud        saludSupay;

    public static GameManager singleton;

    public float progreso;
    public float progresoMaximo;
    public Slider slProgreso;
    
    public bool enPausa = false;
    public Image imProgreso;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    public void RestarVida(float c)
    {
        saludJugador.RestarVida(c);
        if (FOVShaker.Instance!=null)
        {
            FOVShaker.Instance.Shake(amplitude: 4f, frequency: 15f, duration: 0.2f);
        }
    }

    public void SumarVida(float c)
    {
        saludJugador.SumarVida(c);
    }

    public void SumarProgreso(float c)
    {
        progreso += c;
        
        if (progreso >= progresoMaximo)
        {
            PauseMenuUI.singleton.OpenMenu();
            progreso = 0;
        }
    }

    private void Update()
    {
        slProgreso.value = Mathf.Lerp(slProgreso.value, progreso / progresoMaximo, Time.deltaTime*5);
    }

}
