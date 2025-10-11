using UnityEngine;

public class FollowGun : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] Vector3 offSet;
    [SerializeField] Vector2 cadenceLimitsRange;
    [SerializeField] float cadence = 0.7f;
    float cadenceIncreaseFactor = 0;
    bool canShoot = true;

    private void Start()
    {
        GetcadenceIncreaseFactor();
    }

    public void IncreaseCadence()
    {
        cadence -= cadenceIncreaseFactor;
        if (cadence < cadenceLimitsRange.x)
        {
            cadence = cadenceLimitsRange.x;
        }
    }

    void GetcadenceIncreaseFactor()
    {
        cadenceIncreaseFactor = (cadenceLimitsRange.y - cadenceLimitsRange.x) / GetComponent<UpgradeSystem>().upgradesCount;
        cadence = cadenceLimitsRange.y;
    }

    void Coldown()
    {
        canShoot = true;
    }

    void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Vector3 fixedDirection = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            Vector3 direction = fixedDirection - transform.position;
            GameObject actualBullet = Instantiate(bullet, transform.position + offSet, Quaternion.identity);
            actualBullet.transform.forward = direction;
            canShoot = false;
            Invoke(nameof(Coldown), cadence);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && canShoot)
        {
            Shoot();
        }
    }
}
