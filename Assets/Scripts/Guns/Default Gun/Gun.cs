using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : ParentGun
{
    override protected void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Vector3 fixedDirection = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            Vector3 direction = fixedDirection - transform.position;
            GameObject actualBullet = Instantiate(bullet, transform.position + offSet, Quaternion.identity);
            DestroyBulletWithTime(actualBullet);
            actualBullet.transform.forward = direction;
            canShoot = false;
            Invoke(nameof(Coldown), cadence);
        }
    }
}
