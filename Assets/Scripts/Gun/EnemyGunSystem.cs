using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class EnemyGunSystem : MonoBehaviour
{
    // pistol + ammo stats, TODO: read them from a file for a specific gun
    [Header("Gun Stats")]
    public int damage = 10;
    public float range = 50f;
    public float fireRate = 0.25f;

    [Header("Ammo Settings")]
    public int maxAmmo = 12;
    public int magazines = 5;
    public float reloadTime = 1.5f;

    // variables for ammo logic
    private int currentAmmo;
    private int currentMagazines;
    private bool isReloading = false;

    // parameters for recoil
    [Header("Recoil Settings")]
    [SerializeField] private Transform weaponModel;
    [SerializeField] private float recoilForce = 10f;
    [SerializeField] private float snappiness = 15f;
    [SerializeField] private float returnSpeed = 10f;

    private Vector3 reloadRotation = new Vector3(30f, 0f, 0f); // gun rotation at reload
    private Vector3 reloadPosition = new Vector3(0f, -0.2f, 0f); // gun translation at reload
    private float reloadAnimSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform fireOrigin; // muzzle or barrel
    [SerializeField] private ParticleSystem muzzleFlashParticles;

    private float nextTimeToFire = 0f;

    // gun initial position (for shooting animation)
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Vector3 currentRecoilRotation;
    private Vector3 targetRecoilRotation;

    public Transform FireOrigin => fireOrigin;

    void Start()
    {
        currentAmmo = maxAmmo;
        currentMagazines = magazines;

        // set initial gun position
        if (weaponModel != null)
        {
            initialPosition = weaponModel.localPosition;
            initialRotation = weaponModel.localRotation;
        }
    }

    
    void Update()
    {
        /*HandleAnimations();*/
    }

    public bool TryShoot(Vector3 direction)
    {
        if (isReloading)
            return false;
        if(Time.time < nextTimeToFire)
            return false;   
        if(currentAmmo <= 0)
        {
            if (currentMagazines > 0)
            {
                StartCoroutine(Reload());
            }
            return false;
        }

        nextTimeToFire = Time.time + fireRate;
        Shoot(direction);
        return true;
    }

    void Shoot(Vector3 direction)
    {
        currentAmmo--;
        // add recoil
        targetRecoilRotation += new Vector3(recoilForce, 0, 0);

        // trigger muzzle flash
        if (muzzleFlashParticles != null)
        {
            muzzleFlashParticles.Stop();
            muzzleFlashParticles.Play();
        }

        RaycastHit hit;
        if (Physics.Raycast(fireOrigin.position, direction, out hit, range))
        {
            HealthController health = hit.collider.GetComponentInParent<HealthController>();
            if (health != null)
                health.ReceiveDamage(damage, hit.point);
        }

    }

    void HandleAnimations()
    {
        if (weaponModel == null)
            return;

        // compute recoil
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilRotation = Vector3.Slerp(currentRecoilRotation, targetRecoilRotation, snappiness * Time.deltaTime);

        // compute final rotation and position (depending on reloading state)
        Vector3 finalPositionTarget = isReloading ? initialPosition : initialPosition + reloadPosition;
        Quaternion finalRotationTarget = isReloading ? initialRotation * Quaternion.Euler(reloadRotation) : initialRotation * Quaternion.Euler(-currentRecoilRotation.x, 0, 0);

        // apply smooth transition (lerp) from current position to target position
        weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, finalPositionTarget, reloadAnimSpeed * Time.deltaTime);
        weaponModel.localRotation = Quaternion.Slerp(weaponModel.localRotation, finalRotationTarget, reloadAnimSpeed * Time.deltaTime);
    }

    IEnumerator Reload()
    {
        isReloading = true;

        // wait for gun to reload
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        currentMagazines--;
        isReloading = false; // finish reloading cycle
    }
}
