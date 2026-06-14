using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthSystem))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Idle, Taunt, Chase, Attack, Dead }

    [Header("Detection")]
    [SerializeField] private float aggroRange = 12f;
    [SerializeField] private float loseTargetRange = 18f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackStopTime = 0.8f;

    [Header("References")]
    [SerializeField] private Transform player;

    private State currentState = State.Idle;
    private NavMeshAgent agent;
    private Animator animator;
    private HealthSystem health;
    private float attackTimer;
    private float stateTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<HealthSystem>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
        attackTimer = 0f;
        currentState = State.Idle;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private void Update()
    {
        if (health.IsDead) return;
        if (player == null) return;

        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Taunt:
                UpdateTaunt();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Attack:
                UpdateAttack();
                break;
        }
    }

    private void UpdateIdle()
    {
        agent.isStopped = true;
        animator.Play("IdleNormal", 0, 0f);

        float dist = DistanceToPlayer();
        if (dist <= aggroRange)
        {
            currentState = State.Taunt;
            stateTimer = 0f;
        }
    }

    private void UpdateTaunt()
    {
        agent.isStopped = true;
        animator.Play("Taunting", 0, 0f);
        stateTimer += Time.deltaTime;

        if (stateTimer >= 1.5f)
        {
            currentState = State.Chase;
        }
    }

    private void UpdateChase()
    {
        float dist = DistanceToPlayer();

        if (dist > loseTargetRange)
        {
            currentState = State.Idle;
            agent.ResetPath();
            return;
        }

        if (dist <= attackRange)
        {
            currentState = State.Attack;
            agent.ResetPath();
            attackTimer = 0f;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.Play("Run", 0, 0f);
    }

    private void UpdateAttack()
    {
        agent.isStopped = true;
        agent.ResetPath();

        float dist = DistanceToPlayer();

        if (dist > attackRange * 1.5f)
        {
            currentState = State.Chase;
            return;
        }

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (attackTimer <= 0f)
        {
            PlayAttack();
            attackTimer = attackCooldown;
        }
    }

    private void PlayAttack()
    {
        int attackIndex = Random.Range(1, 4);
        animator.Play($"Attack0{attackIndex}", 0, 0f);
        Invoke(nameof(DealDamage), attackStopTime);
    }

    private void DealDamage()
    {
        if (health.IsDead) return;
        if (player == null) return;

        float dist = DistanceToPlayer();
        if (dist <= attackRange * 1.2f)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage, player.position);
                Debug.Log($"{gameObject.name} hit player for {attackDamage} damage!");
            }
        }
    }

    private void HandleDeath()
    {
        currentState = State.Dead;
        agent.isStopped = true;
        agent.ResetPath();
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
