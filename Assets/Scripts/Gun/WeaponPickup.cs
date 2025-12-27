using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    public WeaponData weaponToGive; // weapon stats
    public float rotateSpeed = 50f;

    [HideInInspector]
    public int savedAmmo = -1; // -1 = new weapon, full ammo
    [HideInInspector]
    public int savedMagazines = -1;

    private float pickupActivationTime;

    private void Start()
    {
        // set pickup cooldown
        pickupActivationTime = Time.time + 1.0f;
    }

    void Update()
    {
        // add rotation
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if cooldown time expired
        if (Time.time < pickupActivationTime)
        {
            return;
        }

        // check if player
        if (other.CompareTag("Player"))
        {
            if (weaponToGive == null) return;

            // find inventory
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();

            if (inventory == null)
                inventory = other.GetComponentInParent<PlayerInventory>();

            if (inventory != null)
            {
                // add weapon in inventory
                if (inventory.AddWeapon(weaponToGive, savedAmmo, savedMagazines))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}