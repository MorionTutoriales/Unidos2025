using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed;
    public float poder;

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.transform.CompareTag("Player") && !collision.transform.CompareTag("Bullet"))
        {
            DestroyItself();
            Salud s = collision.GetComponent<Salud>();
            if (s != null)
            {
                s.RestarVida(poder);
                Debug.Log($"Le pegue a algo con salud y le hice: {poder} de daño");
            }
        }
        if (collision.CompareTag("Supay"))
        {
            GameManager.singleton.saludSupay.RestarVida(poder);
        }
    }

    private void DestroyItself()
    {
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + speed * Time.fixedDeltaTime * transform.forward);
    }
}
