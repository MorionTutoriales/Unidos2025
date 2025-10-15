using UnityEngine;

public class RuletaRusa : MonoBehaviour
{
    public GameObject[] listadoObjetos;

    void Start()
    {
        int i = Random.Range(0, listadoObjetos.Length-1);
        if (listadoObjetos[i] != null)
        {
            Instantiate(listadoObjetos[i],transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

}
