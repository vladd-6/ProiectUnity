using UnityEngine;

// script for current weapon state
[System.Serializable]
public class WeaponRuntime
{
    public WeaponData stats;
    public int currentAmmo;  // current ammo count
    public int currentMagazines; // current magazines count

    public WeaponRuntime(WeaponData data)
    {
        stats = data;
        currentAmmo = data.maxAmmo;
        currentMagazines = data.magazines;
    }
}