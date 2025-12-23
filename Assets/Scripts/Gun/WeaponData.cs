using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon System/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string gunName;
    public Sprite icon;
    public GameObject weaponPrefab;
    public GameObject pickupPrefab;

    [Header("Shooting")]
    public bool fullAuto;

    [Header("Stats")]
    public int damage;
    public float range;
    public float fireRate;

    [Header("Ammo")]
    public int maxAmmo;
    public int magazines;
    public float reloadTime;

    [Header("Recoil")]
    public float recoilForce;
    public float snappiness;
    public float returnSpeed;
}