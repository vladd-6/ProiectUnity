using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    public WeaponData weaponToGive; // weapon stats
    public float rotateSpeed = 50f;

    void Update()
    {
        // add rotation
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if player
        if (other.CompareTag("Player"))
        {
            // find inventory
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();

            if (inventory == null)
                inventory = other.GetComponentInParent<PlayerInventory>();

            if (inventory != null)
            {
                // add weapon in inventory
                bool wasPickedUp = inventory.AddWeapon(weaponToGive);

                if (wasPickedUp)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}