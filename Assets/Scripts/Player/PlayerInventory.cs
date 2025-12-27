using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public GunSystem gunSystem;

    // weapon inventory
    [HideInInspector]
    public WeaponRuntime[] weaponSlots = new WeaponRuntime[2];

    [Header("Start settings")]
    public WeaponData startingWeaponData; // starting weapon (optional)

    private int activeSlotIndex = 0;

    void Start()
    {
        weaponSlots[0] = null;
        weaponSlots[1] = null;

        if (startingWeaponData != null)
        {
            // equip starting weapon
            weaponSlots[0] = new WeaponRuntime(startingWeaponData);
            EquipSlot(0);
        }
    }

    void Update()
    {
        // key 1
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipSlot(0);

        // key 2
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipSlot(1);
    }

    public bool AddWeapon(WeaponData data, int specificAmmo = -1, int specificMags = -1)
    {
        if (data == null) return false;

        // verify duplicated weapons
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null && weaponSlots[i].stats == data)
            {
                Debug.Log("Weapon already equipped!");
                return false;
            }
        }

        // find empty spot
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {   
                // equip the new gun
                weaponSlots[i] = new WeaponRuntime(data);

                // if old weapon (!=-1), set old values for ammo
                if (specificAmmo != -1) weaponSlots[i].currentAmmo = specificAmmo;
                if (specificMags != -1) weaponSlots[i].currentMagazines = specificMags;

                EquipSlot(i);
                return true;
            }
        }

        // drop and swap logic
        // select old weapon
        WeaponRuntime oldWeapon = weaponSlots[activeSlotIndex];

        if (oldWeapon != null && oldWeapon.stats.pickupPrefab != null)
        {
            // drop the old weapon in front of the player
            Vector3 dropPosition = transform.position + (transform.forward * 0.5f) + (Vector3.up * 0.01f);
            GameObject droppedObject = Instantiate(oldWeapon.stats.pickupPrefab, dropPosition, Quaternion.identity);

            WeaponPickup pickupScript = droppedObject.GetComponent<WeaponPickup>();
            if (pickupScript != null)
            {
                // set current ammo for the dropped weapon
                pickupScript.savedAmmo = oldWeapon.currentAmmo;
                pickupScript.savedMagazines = oldWeapon.currentMagazines;
            }
        }

        // if full, add over existing weapon
        weaponSlots[activeSlotIndex] = new WeaponRuntime(data);

        if (specificAmmo != -1) weaponSlots[activeSlotIndex].currentAmmo = specificAmmo;
        if (specificMags != -1) weaponSlots[activeSlotIndex].currentMagazines = specificMags;

        EquipSlot(activeSlotIndex);
        return true;
    }

    void EquipSlot(int index)
    {
        // extra checks
        if (index < 0 || index >= weaponSlots.Length) return;
        if (weaponSlots[index] == null) return;
        if (weaponSlots[index].stats == null) return;

        activeSlotIndex = index;
        gunSystem.Equip(weaponSlots[index]);
    }

    public int GetActiveSlotIndex()
    {
        return activeSlotIndex;
    }
}