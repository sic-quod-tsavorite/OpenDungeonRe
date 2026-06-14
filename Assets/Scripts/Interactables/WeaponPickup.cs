using UnityEngine;

public class WeaponPickup : Interactable
{
    [Header("Weapon")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private GameObject weaponModel;

    public override void Interact()
    {
        InventorySystem inventory = FindAnyObjectByType<InventorySystem>();
        if (inventory == null)
        {
            Debug.LogWarning($"{gameObject.name}: No InventorySystem found on player.");
            return;
        }

        if (!inventory.AddWeapon(weaponData))
        {
            Debug.Log($"{gameObject.name}: Inventory full, cannot pick up {weaponData.weaponName}.");
            return;
        }

        Debug.Log($"Picked up {weaponData.weaponName}");

        if (weaponModel != null)
        {
            Destroy(weaponModel);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
