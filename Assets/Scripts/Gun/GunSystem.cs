using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GunSystem : MonoBehaviour
{
    public Transform weaponHolderParent; // where the weapon is placed

    private WeaponRuntime activeWeapon; // current weapon
    private Transform currentWeaponModel;
    private ParticleSystem muzzleFlashParticles;

    // UI
    [SerializeField] private Camera fpsCamera;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Slider ammoSlider;

    // vars for shooting logic
    private bool isReloading = false;
    private float nextTimeToFire = 0f;

    // recoil and animations
    private Vector3 currentRecoilRotation;
    private Vector3 targetRecoilRotation;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 reloadRotation = new Vector3(30f, 0f, 0f);
    private Vector3 reloadPosition = new Vector3(0f, -0.2f, 0f);
    private float reloadAnimSpeed = 5f;

    public void Equip(WeaponRuntime runtime)
    {
        activeWeapon = runtime; // saved number of bullets

        // reset time to fire from previous gun
        nextTimeToFire = 0f;

        // clear current weapon
        if (weaponHolderParent.childCount > 0)
        {
            foreach (Transform child in weaponHolderParent) Destroy(child.gameObject);
        }

        // draw picked weapon
        if (activeWeapon.stats.weaponPrefab != null)
        {
            GameObject newGun = Instantiate(activeWeapon.stats.weaponPrefab, weaponHolderParent);
            currentWeaponModel = newGun.transform;
            currentWeaponModel.localPosition = Vector3.zero;
            currentWeaponModel.localRotation = Quaternion.identity;

            initialPosition = currentWeaponModel.localPosition;
            initialRotation = currentWeaponModel.localRotation;

            muzzleFlashParticles = newGun.GetComponentInChildren<ParticleSystem>();
        }

        // UI
        isReloading = false;
        if (ammoSlider != null)
        {
            ammoSlider.maxValue = activeWeapon.stats.maxAmmo;
        }
        UpdateAmmoUI();
    }

    void Update()
    {
        if (activeWeapon == null) return;

        HandleAnimations();

        if (isReloading) return;

        // auto / single
        bool triggerPulled = activeWeapon.stats.fullAuto ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        // can reload only if current magazine not full and enough magazines remaining
        if (Input.GetKeyDown(KeyCode.R) && activeWeapon.currentAmmo < activeWeapon.stats.maxAmmo && activeWeapon.currentMagazines > 0)
        {
            StartCoroutine(Reload());
        }
        else if (triggerPulled && Time.time >= nextTimeToFire && activeWeapon.currentAmmo > 0)
        {
            nextTimeToFire = Time.time + activeWeapon.stats.fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        // update ammo count
        activeWeapon.currentAmmo--;
        UpdateAmmoUI();

        // add recoil
        targetRecoilRotation += new Vector3(activeWeapon.stats.recoilForce, 0, 0);

        if (muzzleFlashParticles != null) { muzzleFlashParticles.Stop(); muzzleFlashParticles.Play(); }

        // Raycast
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.SphereCast(fpsCamera.transform.position, 0.1f, fpsCamera.transform.forward, out hit, activeWeapon.stats.range, layerMask))
        {
            HealthController turretHealth = hit.collider.GetComponentInParent<HealthController>();
            if (turretHealth != null) turretHealth.ReceiveDamage(activeWeapon.stats.damage, hit.point);

            DroneHealth droneHealth = hit.collider.GetComponentInParent<DroneHealth>();
            if (droneHealth != null) droneHealth.ReceiveDamage(activeWeapon.stats.damage, hit.point);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(activeWeapon.stats.reloadTime);

        activeWeapon.currentAmmo = activeWeapon.stats.maxAmmo;
        activeWeapon.currentMagazines--;

        isReloading = false;
        UpdateAmmoUI();
    }

    void HandleAnimations()
    {
        if (currentWeaponModel == null) return;

        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, activeWeapon.stats.returnSpeed * Time.deltaTime);
        currentRecoilRotation = Vector3.Slerp(currentRecoilRotation, targetRecoilRotation, activeWeapon.stats.snappiness * Time.deltaTime);

        Vector3 finalPos = isReloading ? initialPosition + reloadPosition : initialPosition;
        Quaternion recoilRot = Quaternion.Euler(-currentRecoilRotation.x, 0, 0);
        Quaternion finalRot = isReloading ? initialRotation * Quaternion.Euler(reloadRotation) : initialRotation * recoilRot;

        currentWeaponModel.localPosition = Vector3.Lerp(currentWeaponModel.localPosition, finalPos, reloadAnimSpeed * Time.deltaTime);
        currentWeaponModel.localRotation = Quaternion.Slerp(currentWeaponModel.localRotation, finalRot, reloadAnimSpeed * Time.deltaTime);
    }

    void UpdateAmmoUI()
    {
        if (activeWeapon == null) return;
        if (ammoText != null) ammoText.text = activeWeapon.currentAmmo + " / " + activeWeapon.currentMagazines;
        if (ammoSlider != null) ammoSlider.value = activeWeapon.currentAmmo;
    }
}