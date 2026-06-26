using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class EnemyAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Alert, Chase, Attack, Stunned, Dead }

    [SerializeField] private EnemyData data;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform[] waypoints;

    private NavMeshAgent agent;
    private Collider col;
    private Transform playerTransform;
    private IDamageable playerDamageable;

    private State state = State.Patrol;
    private float currentHp;

    private float alertTimer;
    private float attackCooldownTimer;
    private float stunTimer;

    private Vector3 patrolOrigin;
    private Vector3 currentPatrolTarget;
    private int waypointIndex;
    private bool hasPatrolTarget;

    private static readonly int HashSpeed  = Animator.StringToHash("Speed");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashStun   = Animator.StringToHash("Stun");
    private static readonly int HashDead   = Animator.StringToHash("Dead");

    // ---- IDamageable ------------------------------------------

    public bool IsAlive => state != State.Dead;

    void IDamageable.TakeDamage(float damage, bool isCritical, Vector3 knockbackDirection)
    {
        if (state == State.Dead) return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0f);

        EventBus.Publish(new OnEnemyDamaged
        {
            enemy = gameObject,
            amount = damage,
            isCritical = isCritical,
            worldPosition = transform.position
        });

        if (currentHp <= 0f)
        {
            Die();
            return;
        }

        // Uderzony w Patrol/Alert — natychmiastowa pogoń
        if (state == State.Patrol || state == State.Alert)
            EnterState(State.Chase);
    }

    // ---- Lifecycle --------------------------------------------

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        col   = GetComponent<Collider>();

        if (data == null)
        {
            Debug.LogError("[EnemyAI] EnemyData nie przypisany!", this);
            return;
        }

        currentHp    = data.maxHp;
        patrolOrigin = transform.position;

        agent.speed = data.patrolSpeed;

        var playerObj = FindFirstObjectByType<PlayerController>();
        if (playerObj != null)
        {
            playerTransform  = playerObj.transform;
            playerDamageable = playerObj.GetComponent<IDamageable>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        switch (state)
        {
            case State.Patrol:  UpdatePatrol();  break;
            case State.Alert:   UpdateAlert();   break;
            case State.Chase:   UpdateChase();   break;
            case State.Attack:  UpdateAttack();  break;
            case State.Stunned: UpdateStunned(); break;
        }

        attackCooldownTimer -= Time.deltaTime;
        UpdateAnimator();
    }

    // ---- Stany ------------------------------------------------

    private void UpdatePatrol()
    {
        if (PlayerInRange(data.detectRange))
        {
            EnterState(State.Alert);
            return;
        }

        if (waypoints != null && waypoints.Length > 0)
        {
            PatrolWaypoints();
        }
        else
        {
            PatrolRandom();
        }
    }

    private void UpdateAlert()
    {
        agent.isStopped = true;
        FacePlayer();

        alertTimer -= Time.deltaTime;
        if (alertTimer <= 0f)
            EnterState(State.Chase);
    }

    private void UpdateChase()
    {
        if (playerTransform == null) { EnterState(State.Patrol); return; }

        float dist = DistanceToPlayer();

        if (dist <= data.attackRange)
        {
            EnterState(State.Attack);
            return;
        }

        if (dist > data.losePlayerRange)
        {
            EnterState(State.Patrol);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(playerTransform.position);
    }

    private void UpdateAttack()
    {
        if (playerTransform == null) { EnterState(State.Patrol); return; }

        float dist = DistanceToPlayer();

        if (dist > data.attackRange * 1.2f)
        {
            EnterState(State.Chase);
            return;
        }

        agent.isStopped = true;
        FacePlayer();

        if (attackCooldownTimer <= 0f)
        {
            attackCooldownTimer = data.attackCooldown;
            animator?.SetTrigger(HashAttack);
        }
    }

    private void UpdateStunned()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
            EnterState(State.Chase);
    }

    // ---- Wejście do stanu -------------------------------------

    private void EnterState(State next)
    {
        state = next;

        switch (next)
        {
            case State.Alert:
                alertTimer = data.alertDelay;
                agent.isStopped = true;
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.speed = data.moveSpeed;
                break;

            case State.Patrol:
                agent.isStopped = false;
                agent.speed = data.patrolSpeed;
                hasPatrolTarget = false;
                break;

            case State.Attack:
                agent.isStopped = true;
                break;
        }
    }

    private void Die()
    {
        state = State.Dead;
        agent.isStopped = true;
        agent.enabled = false;
        if (col != null) col.enabled = false;

        animator?.SetTrigger(HashDead);

        EventBus.Publish(new OnEnemyDied
        {
            enemy       = gameObject,
            enemyType   = data.enemyType,
            worldPosition = transform.position,
            xpReward    = data.xpReward
        });

        Destroy(gameObject, 3f);
    }

    // ---- Patrol -----------------------------------------------

    private void PatrolWaypoints()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[waypointIndex];
        if (target == null) { waypointIndex = 0; return; }

        agent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) < 0.6f)
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    private void PatrolRandom()
    {
        if (hasPatrolTarget && agent.remainingDistance > 0.5f) return;

        // Wybierz nowy losowy punkt w promieniu patrolRadius
        for (int i = 0; i < 8; i++)
        {
            Vector2 rand2D = Random.insideUnitCircle * data.patrolRadius;
            Vector3 candidate = patrolOrigin + new Vector3(rand2D.x, 0f, rand2D.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                currentPatrolTarget = hit.position;
                agent.SetDestination(currentPatrolTarget);
                hasPatrolTarget = true;
                return;
            }
        }
    }

    // ---- Pomocnicze -------------------------------------------

    private bool PlayerInRange(float range) =>
        playerTransform != null && DistanceToPlayer() <= range;

    private float DistanceToPlayer() =>
        playerTransform != null
            ? Vector3.Distance(transform.position, playerTransform.position)
            : float.MaxValue;

    private void FacePlayer()
    {
        if (playerTransform == null) return;
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 10f);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        float speed = agent.enabled ? agent.velocity.magnitude : 0f;
        animator.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);
    }

    // ---- Animation Event — wywoływane przez animator ----------

    /// <summary>Wywołaj w Animation Event w momencie uderzenia.</summary>
    public void OnAttackHit()
    {
        if (state != State.Attack || playerDamageable == null) return;
        if (!PlayerInRange(data.attackRange * 1.3f)) return;

        Vector3 knockback = playerTransform != null
            ? (playerTransform.position - transform.position).normalized
            : Vector3.zero;

        playerDamageable.TakeDamage(data.attackDamage, false, knockback);
    }

    /// <summary>Wywołaj w Animation Event gdy animacja stuna się kończy.</summary>
    public void ApplyStun(float duration)
    {
        if (state == State.Dead) return;
        stunTimer = duration > 0f ? duration : data.stunDuration;
        agent.isStopped = true;
        state = State.Stunned;
        animator?.SetTrigger(HashStun);
    }
}
