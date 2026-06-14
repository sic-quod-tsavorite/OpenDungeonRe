using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private int maxSlots = 4;

    private List<WeaponData> weapons = new List<WeaponData>();
    private int currentSlotIndex = -1;

    public int CurrentSlotIndex => currentSlotIndex;
    public int MaxSlots => maxSlots;
    public WeaponData CurrentWeapon => currentSlotIndex >= 0 && currentSlotIndex < weapons.Count
        ? weapons[currentSlotIndex]
        : null;

    public event System.Action<WeaponData> OnWeaponChanged;

    private void OnEnable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnPrevious += CyclePrevious;
            PlayerInputManager.Instance.OnNext += CycleNext;
        }
        else
        {
            StartCoroutine(WaitForInputManager());
        }
    }

    private System.Collections.IEnumerator WaitForInputManager()
    {
        while (PlayerInputManager.Instance == null)
            yield return null;

        PlayerInputManager.Instance.OnPrevious += CyclePrevious;
        PlayerInputManager.Instance.OnNext += CycleNext;
    }

    private void OnDisable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnPrevious -= CyclePrevious;
            PlayerInputManager.Instance.OnNext -= CycleNext;
        }
    }

    public bool AddWeapon(WeaponData weapon)
    {
        if (weapon == null) return false;
        if (weapons.Count >= maxSlots) return false;

        weapons.Add(weapon);

        if (weapons.Count == 1)
        {
            EquipSlot(0);
        }

        return true;
    }

    public bool RemoveWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weapons.Count) return false;

        weapons.RemoveAt(slotIndex);

        if (weapons.Count == 0)
        {
            currentSlotIndex = -1;
            OnWeaponChanged?.Invoke(null);
        }
        else if (currentSlotIndex >= weapons.Count)
        {
            EquipSlot(weapons.Count - 1);
        }
        else if (currentSlotIndex > slotIndex)
        {
            currentSlotIndex--;
        }

        return true;
    }

    public void EquipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weapons.Count) return;

        currentSlotIndex = slotIndex;
        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    private void CyclePrevious()
    {
        if (weapons.Count == 0) return;

        int newIndex = currentSlotIndex - 1;
        if (newIndex < 0) newIndex = weapons.Count - 1;
        EquipSlot(newIndex);
    }

    private void CycleNext()
    {
        if (weapons.Count == 0) return;

        int newIndex = currentSlotIndex + 1;
        if (newIndex >= weapons.Count) newIndex = 0;
        EquipSlot(newIndex);
    }
}
