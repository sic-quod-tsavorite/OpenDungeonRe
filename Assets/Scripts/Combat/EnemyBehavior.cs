using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class EnemyBehavior : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 2f;

    private HealthSystem health;
    private Animator animator;
    private float previousHealth;
    private bool isDead;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();
        animator = GetComponent<Animator>();
        previousHealth = health.MaxHealth;
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
        health.OnHealthChanged += HandleHealthChanged;
        isDead = false;
        previousHealth = health.MaxHealth;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
        health.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (current < previousHealth && !isDead)
        {
            PlayGetHit();
        }
        previousHealth = current;
    }

    private void PlayGetHit()
    {
        if (animator != null)
        {
            animator.Play("GetHit", 0, 0f);
        }
    }

    private void HandleDeath()
    {
        isDead = true;
        if (animator != null)
        {
            animator.Play("Die", 0, 0f);
        }
        Invoke(nameof(Deactivate), deathDelay);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
