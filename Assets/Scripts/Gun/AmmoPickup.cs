using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Settings")]
    public int magazinesToAdd = 1; // how many magazines each pickup brings
    public float rotateSpeed = 50f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null) inventory = other.GetComponentInParent<PlayerInventory>();

            if (inventory != null)
            {
                if (inventory.AddAmmoToCurrentWeapon(magazinesToAdd))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}