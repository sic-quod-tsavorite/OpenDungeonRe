using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Camera playerCamera;

    [Header("Settings")]
    [SerializeField] private float defaultAttackRange = 3f;

    private InventorySystem inventory;
    private GameObject currentWeaponInstance;
    private float attackCooldownTimer;

    private Quaternion weaponRestRotation;
    private bool isSwinging;
    private float swingTimer;
    private float swingAngle;
    private float swingSpeed;

    private void Awake()
    {
        inventory = GetComponent<InventorySystem>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (weaponHolder != null)
        {
            weaponRestRotation = weaponHolder.localRotation;
        }
    }

    private void OnEnable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnAttack += HandleAttack;
        }
        else
        {
            StartCoroutine(WaitForInputManager());
        }

        if (inventory != null)
        {
            inventory.OnWeaponChanged += OnWeaponChanged;
        }
    }

    private System.Collections.IEnumerator WaitForInputManager()
    {
        while (PlayerInputManager.Instance == null)
            yield return null;

        PlayerInputManager.Instance.OnAttack += HandleAttack;
    }

    private void OnDisable()
    {
        if (PlayerInputManager.Instance != null)
        {
            PlayerInputManager.Instance.OnAttack -= HandleAttack;
        }

        if (inventory != null)
        {
            inventory.OnWeaponChanged -= OnWeaponChanged;
        }

        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
            currentWeaponInstance = null;
        }
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (isSwinging)
        {
            UpdateSwingAnimation();
        }
    }

    private void HandleAttack()
    {
        if (attackCooldownTimer > 0f) return;

        WeaponData weapon = inventory != null ? inventory.CurrentWeapon : null;

        if (weapon == null) return;

        attackCooldownTimer = 1f / weapon.attackRate;

        PerformAttack(weapon);
        StartSwing(weapon.swingAngle, weapon.swingSpeed);
    }

    private void PerformAttack(WeaponData weapon)
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        float range = weapon.attackRange > 0f ? weapon.attackRange : defaultAttackRange;

        if (Physics.Raycast(ray, out RaycastHit hit, range, weapon.hitLayers))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weapon.damage, hit.point);
                Debug.Log($"Hit {hit.collider.gameObject.name} for {weapon.damage} damage");
            }
        }
    }

    private void StartSwing(float angle, float speed)
    {
        swingAngle = angle;
        swingSpeed = speed;
        swingTimer = 0f;
        isSwinging = true;
    }

    private void UpdateSwingAnimation()
    {
        if (weaponHolder == null)
        {
            isSwinging = false;
            return;
        }

        swingTimer += Time.deltaTime * swingSpeed;
        float progress = swingTimer;

        if (progress >= 1f)
        {
            weaponHolder.localRotation = weaponRestRotation;
            isSwinging = false;
            return;
        }

        float swingCurve;
        if (progress < 0.5f)
        {
            swingCurve = progress * 2f;
        }
        else
        {
            swingCurve = 1f - (progress - 0.5f) * 2f;
        }

        Quaternion swingOffset = Quaternion.Euler(-swingAngle * swingCurve, 0f, 0f);
        weaponHolder.localRotation = weaponRestRotation * swingOffset;
    }

    private void OnWeaponChanged(WeaponData newWeapon)
    {
        isSwinging = false;

        if (weaponHolder != null)
        {
            weaponHolder.localRotation = weaponRestRotation;
        }

        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
            currentWeaponInstance = null;
        }

        if (newWeapon != null && newWeapon.weaponPrefab != null && weaponHolder != null)
        {
            currentWeaponInstance = Instantiate(newWeapon.weaponPrefab, weaponHolder);
            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;
        }
    }
}
