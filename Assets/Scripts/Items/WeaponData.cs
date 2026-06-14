using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName = "New Weapon";
    public GameObject weaponPrefab;

    [Header("Combat Stats")]
    public float damage = 10f;
    public float attackRange = 3f;
    public float attackRate = 1f;
    public LayerMask hitLayers = ~0;

    [Header("Swing Animation")]
    public float swingAngle = 60f;
    public float swingSpeed = 8f;
}
