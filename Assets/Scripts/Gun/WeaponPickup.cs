using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Set?ri")]
    public WeaponData weaponToGive; // Trage fi?ierul (ex: PistolStats) aici
    public float rotateSpeed = 50f;

    void Update()
    {
        // Efect vizual: se învârte arma
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verific?m dac? cel care a intrat este Player-ul
        // Asigur?-te c? Player-ul are tag-ul "Player"
        if (other.CompareTag("Player"))
        {
            // C?ut?m inventarul pe obiectul care a intrat sau pe p?rin?ii lui
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();

            // Dac? nu e direct pe player, poate e pe un p?rinte (uneori colliderul e pe un copil)
            if (inventory == null)
                inventory = other.GetComponentInParent<PlayerInventory>();

            if (inventory != null)
            {
                // Încerc?m s? ad?ug?m arma
                bool wasPickedUp = inventory.AddWeapon(weaponToGive);

                if (wasPickedUp)
                {
                    // Distrugem obiectul de pe jos
                    Destroy(gameObject);
                }
            }
        }
    }
}