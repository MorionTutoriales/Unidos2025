using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public ParentGun shotgun;
    public ParentGun followergun;
    public ParentGun defaultgun;
    public GunType currentGunType = GunType.Default;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PickGun pickGun))
        {
            if (pickGun.gunType == GunType.Shotgun && !GameManager.singleton.shotgunPicked)
            {
                GameManager.singleton.shotgunPicked = true;
                shotgun.enabled = true;
            }
            else if (pickGun.gunType == GunType.FollowerGun && !GameManager.singleton.followergunPicked)
            {
                GameManager.singleton.followergunPicked = true;
                followergun.enabled = true;
            }
        }
    }

    public void SwitchGun(GunType gunType)
    {
        currentGunType = gunType;
        string gunName = currentGunType.ToString();
        defaultgun.enabled = gunName == "Default";
        shotgun.enabled = gunName == "Shotgun";
        followergun.enabled = gunName == "FollowerGun";
    }

    public bool IsGunActive(GunType gunType)
    {
        bool isActive = false;

        switch (gunType)
        {
            case GunType.Shotgun:
                if (GameManager.singleton.shotgunPicked)
                    isActive = true;
                break;
            case GunType.FollowerGun:
                if (GameManager.singleton.followergunPicked)
                    isActive = true;
                break;
            default:
                break;
        }
        return isActive;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchGun(GunType.Default);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && GameManager.singleton.shotgunPicked)
        {
            SwitchGun(GunType.Shotgun);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && GameManager.singleton.followergunPicked)
        {
            SwitchGun(GunType.FollowerGun);
        }
    }
}

public enum GunType
{
    Default,
    Shotgun,
    FollowerGun
}