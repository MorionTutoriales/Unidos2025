using UnityEngine;

public class Shotgun : ParentGun
{
    [SerializeField] int bulletsPerShot = 5;
    [SerializeField] Vector2 spreadAngleRange = new Vector2(-10, 10);

    override protected void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 fixedDirection = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            Vector3 direction = fixedDirection - transform.position;
            for (int i = 0; i < bulletsPerShot; i++)
            {
                GameObject actualBullet = Instantiate(bullet, transform.position + offSet, Quaternion.identity);
                SetBullet(actualBullet);
                Vector3 spread = new Vector2(Random.Range(spreadAngleRange.x, spreadAngleRange.y), Random.Range(spreadAngleRange.x, spreadAngleRange.y));
                actualBullet.transform.forward = direction.normalized + spread;
            }
            canShoot = false;
            Invoke(nameof(Coldown), cadence);
        }
    }
}
