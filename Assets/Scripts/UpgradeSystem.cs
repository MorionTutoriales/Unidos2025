using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    public int upgradesCount = 5;
    Gun gun;

    private void Awake()
    {
        gun = GetComponent<Gun>();
    }

    public void UpgradeGun()
    {
        gun.IncreaseCadence();
    }
}
