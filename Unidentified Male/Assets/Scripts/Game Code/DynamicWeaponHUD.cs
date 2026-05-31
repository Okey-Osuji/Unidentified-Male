using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DynamicWeaponHUD : MonoBehaviour
{
    public static DynamicWeaponHUD Instance;

    [Header("UI Slot Background Images")]
    [SerializeField] private Image fistsImage;
    [SerializeField] private Image slot1Image;
    [SerializeField] private Image slot2Image;
    [SerializeField] private Image slot3Image;

    [Header("UI Slot Text Components")]
    [SerializeField] private TextMeshProUGUI slot1Text;
    [SerializeField] private TextMeshProUGUI slot2Text;
    [SerializeField] private TextMeshProUGUI slot3Text;

    [Header("Styling Colors")]
    [SerializeField] private Color equippedColor = Color.white;
    [SerializeField] private Color unequippedColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    /// Refreshes the bottom-right HUD layout based on current weapons carried.
    /// <param name="unlockedWeapons">The active weapon list from PlayerCombat</param>
    /// <param name="activeWeapon">The Weapon component currently in the player's hand</param>
    public void RefreshHUD(List<GameObject> unlockedWeapons, Weapon activeWeapon)
    {
        // Resets all backgrounds to dim state
        if (fistsImage != null) fistsImage.color = unequippedColor;
        if (slot1Image != null) slot1Image.color = unequippedColor;
        if (slot2Image != null) slot2Image.color = unequippedColor;
        if (slot3Image != null) slot3Image.color = unequippedColor;

        // Highlights fists if active weapon is null
        if (activeWeapon == null)
        {
            if (fistsImage != null) fistsImage.color = equippedColor;
        }

        // Updates Slot 1
        UpdateSlot(0, unlockedWeapons, slot1Image, slot1Text, activeWeapon);
        // Updates Slot 2
        UpdateSlot(1, unlockedWeapons, slot2Image, slot2Text, activeWeapon);
        // Updates Slot 3
        UpdateSlot(2, unlockedWeapons, slot3Image, slot3Text, activeWeapon);
    }

    private void UpdateSlot(int index, List<GameObject> weapons, Image slotImage, TextMeshProUGUI slotText, Weapon activeWeapon)
    {
        if (slotImage == null) return;

        // Targets the entire slot panel GameObject that holds the image
        GameObject slotPanel = slotImage.gameObject;

        if (index < weapons.Count && weapons[index] != null)
        {
            // Turns the whole panel ON because a weapon exists for this slot
            slotPanel.SetActive(true);

            Weapon slotWep = weapons[index].GetComponent<Weapon>();

            if (slotWep != null)
            {
                // Sets the weapon text name
                if (slotText != null) slotText.text = slotWep.weaponName.ToUpper();

                // Assigns the weapon sprite to the slot image if it exists
                if (slotWep.weaponIcon != null)
                {
                    slotImage.sprite = slotWep.weaponIcon;
                }

                // Highlights the slot panel if this weapon matches what's in your hand
                if (activeWeapon != null && slotWep == activeWeapon)
                {
                    slotImage.color = equippedColor;
                }
            }
        }
        else
        {
            // Turns the whole panel OFF because there's no weapon in this inventory index
            slotPanel.SetActive(false);
        }
    }
}