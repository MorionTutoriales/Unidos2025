using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform    jugador;
    public Salud        saludJugador;

    public static GameManager singleton;

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
}
