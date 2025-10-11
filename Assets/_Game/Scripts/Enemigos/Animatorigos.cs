using System.Collections;
using UnityEngine;

public class Animatorigos : MonoBehaviour
{
    public Animator animator;

    public float velocidad;

    IEnumerator Velocidometro()
    {
        while (true)
        {
            Vector3 pos1 = transform.position;
            yield return new WaitForSeconds(0.5f);
            velocidad = (transform.position - pos1).sqrMagnitude;
            animator.SetFloat("velocidad", velocidad);
        }
    }

    // Update is called once per frame
    void Start()
    {
        StartCoroutine(Velocidometro());
    }
}
