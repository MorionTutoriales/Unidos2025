using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed;

    private void Start()
    {
        Invoke(nameof(DestroyItself), 3f);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.transform.CompareTag("Player") && !collision.transform.CompareTag("Bullet"))
        {
            DestroyItself();
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
