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

    public bool AddWeapon(WeaponData data)
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
            Vector3 dropPosition = transform.position + (transform.forward * 5f);
            Instantiate(oldWeapon.stats.pickupPrefab, dropPosition, Quaternion.identity);
        }

        // if full, add over existing weapon
        weaponSlots[activeSlotIndex] = new WeaponRuntime(data);
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