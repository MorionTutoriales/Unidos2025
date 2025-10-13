using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentGun : MonoBehaviour
{
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected Vector3 offSet;
    [SerializeField] protected Vector2 cadenceLimitsRange;
    [SerializeField] protected Vector2 damageLimitsRange;
    [SerializeField] protected Vector2 timelifeLimitsRange;
    [SerializeField] protected float cadence = 0.7f;
    [SerializeField] protected float timeLife = 3f;
    protected float cadenceIncreaseFactor = 0;
    protected float damageIncreaseFactor = 0;
    protected float timelifeIncreaseFactor = 0;
    protected bool canShoot = true;

    private void Start()
    {
        GetcadenceIncreaseFactor();
        GetTimelifeIncreaseFactor();
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

    public void IncreaseTimelife()
    {
        cadence -= cadenceIncreaseFactor;
        if (cadence < cadenceLimitsRange.x)
        {
            cadence = cadenceLimitsRange.x;
        }
    }

    void GetTimelifeIncreaseFactor()
    {
        cadenceIncreaseFactor = (cadenceLimitsRange.y - cadenceLimitsRange.x) / GetComponent<UpgradeSystem>().upgradesCount;
        cadence = cadenceLimitsRange.y;
    }

    protected void DestroyBulletWithTime(GameObject bullet)
    {
        StartCoroutine(DestroyBullet(bullet));
    }

    IEnumerator DestroyBullet(GameObject bullet)
    {
        yield return new WaitForSeconds(timeLife);
        Destroy(bullet);
    }

    protected void Coldown()
    {
        canShoot = true;
    }

    virtual protected void Shoot() { }

    void Update()
    {
        if (Input.GetMouseButton(0) && canShoot)
        {
            Shoot();
        }
    }
}
