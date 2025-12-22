using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;

    [Header("UI Slot 1")]
    public Image slot0_Background;
    public Image slot0_Icon;
    public TextMeshProUGUI slot0_Ammo;

    [Header("UI Slot 2")]
    public Image slot1_Background;
    public Image slot1_Icon;
    public TextMeshProUGUI slot1_Ammo;

    [Header("Colors")]
    public Color selectedColor = Color.green;
    public Color normalColor = new Color(0, 0, 0, 0.5f);

    void Update()
    {
        // update slot 0
        UpdateSlot(0, slot0_Background, slot0_Icon, slot0_Ammo);

        // update slot 1
        UpdateSlot(1, slot1_Background, slot1_Icon, slot1_Ammo);
    }

    void UpdateSlot(int index, Image bg, Image icon, TextMeshProUGUI text)
    {
        // if index in range
        if (index >= playerInventory.weaponSlots.Length) 
            return;

        WeaponRuntime weapon = playerInventory.weaponSlots[index];

        if (weapon != null && weapon.stats != null)
        {
            icon.enabled = true;
            icon.sprite = weapon.stats.icon;

            // add ammo count
            text.text = $"{weapon.currentAmmo} / {weapon.currentMagazines}";

            // if selected change color
            if (playerInventory.GetActiveSlotIndex() == index)
            {
                bg.color = selectedColor;
            }
            else
            {
                bg.color = normalColor;
            }
        }
        else
        {
            // empty slot
            icon.enabled = false;
            text.text = "";
            bg.color = normalColor;
        }
    }
}