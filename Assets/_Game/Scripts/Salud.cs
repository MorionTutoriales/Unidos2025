using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Salud : MonoBehaviour
{
    public float vidaMaxima;
    public float vidaActual;

    public UnityEvent eventoMuerte;
    public bool vivo = true;

    public Slider sliderVida;

    private void Start()
    {
        vidaActual = vidaMaxima;
        if (sliderVida != null)
        {
            sliderVida.value = vidaActual/vidaMaxima;
        }
    }
    public void RestarVida(float vida)
    {
        if (!vivo) return;
        vidaActual -= vida;

        if (vidaActual <= 0)
        {
            eventoMuerte.Invoke();
            vivo = false;
        }
        if (vidaActual>vidaMaxima)
        {
            vidaActual = vidaMaxima;
        }
        if (sliderVida != null)
        {
            sliderVida.value = vidaActual/vidaMaxima;
        }
    }

    public void SumarVida(float vida)
    {
        RestarVida(-vida);
    }

}
