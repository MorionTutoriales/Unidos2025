using UnityEngine;

public class SupayDamager : MonoBehaviour
{
    public float frecuenciaDamage;
    public float damage;

    float tiempoDamage;

    private void OnTriggerStay(Collider other)
    {
        if (Time.time > tiempoDamage && other.CompareTag("Player"))
        {
            tiempoDamage = Time.time + frecuenciaDamage;
            GameManager.singleton.RestarVida(damage);
        }
    }
}
