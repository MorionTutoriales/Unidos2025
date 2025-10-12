using UnityEngine;
using UnityEngine.Events;

public class Accionador : MonoBehaviour
{
    public UnityEvent eventoActivar;


    public void Activar()
    {
        eventoActivar.Invoke();
    }
}
