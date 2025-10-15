using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    public int upgradesCount = 5;
    ParentGun[] guns;

    private void Awake()
    {
        guns = GetComponents<ParentGun>();
    }

    public void CadenceUpgrade()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].IncreaseCadence();
        }
    }

    public void RangeUpgrade()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].IncreaseTimelife();
        }
    }

    public void DamageUpgrade()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].IncreaseDamage();
        }
    }
}
