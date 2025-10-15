using UnityEngine;

public class PickGun : MonoBehaviour
{
    public GunType gunType;

    private void Start()
    {
        if (GameManager.singleton.jugador.GetComponent<GunController>().IsGunActive(gunType))
        {
            Debug.Log("Already picked " + gunType.ToString());
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GunController gunController))
        {
            gunController.SwitchGun(gunType);
            Debug.Log("Picked " + gunType.ToString());
            Destroy(gameObject);
        }
    }
}
