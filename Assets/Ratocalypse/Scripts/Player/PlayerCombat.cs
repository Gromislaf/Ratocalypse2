using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Animator animator;

    private PlayerController controller;
    private bool canAttack = true;
    private float fallbackTimer;

    private static readonly int HashAttack      = Animator.StringToHash("Attack");
    private static readonly int HashAttackSpeed = Animator.StringToHash("AttackSpeed");

    // ---- Lifecycle --------------------------------------------

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnItemEquipped>(OnItemEquipped);
        EventBus.Subscribe<OnItemUnequipped>(OnItemUnequipped);
        EventBus.Subscribe<OnPlayerLevelUp>(OnLevelUp);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnItemEquipped>(OnItemEquipped);
        EventBus.Unsubscribe<OnItemUnequipped>(OnItemUnequipped);
        EventBus.Unsubscribe<OnPlayerLevelUp>(OnLevelUp);
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused) return;

        TickFallback();

        if (canAttack && controller.IsInAttackRange && controller.CurrentTarget != null)
            TriggerAttack();
    }

    // ---- Atak -------------------------------------------------

    private void TriggerAttack()
    {
        canAttack = false;
        fallbackTimer = 1f / Mathf.Max(stats.TotalAttackSpeed, 0.1f);

        if (animator != null)
        {
            animator.SetFloat(HashAttackSpeed, stats.TotalAttackSpeed);
            animator.SetTrigger(HashAttack);
        }
        else
        {
            ApplyDamage();
        }
    }

    // ---- Animation Events (wywoływane przez Unity) ------------

    /// <summary>Wywołaj w Animation Event w klatce uderzenia.</summary>
    public void OnAttackHit()
    {
        ApplyDamage();
    }

    /// <summary>Wywołaj w Animation Event na końcu animacji ataku.</summary>
    public void OnAttackEnd()
    {
        animator?.ResetTrigger(HashAttack);
        canAttack = true;
        fallbackTimer = 0f;
    }

    // ---- Obrażenia --------------------------------------------

    private void ApplyDamage()
    {
        IDamageable target = controller.CurrentTarget;
        if (target == null || !target.IsAlive) return;

        bool isCritical = stats.RollCrit();
        float damage = stats.TotalDamage;
        if (isCritical) damage *= stats.critMultiplier;
        damage = Mathf.Round(damage);

        Vector3 knockback = Vector3.zero;
        if (target is Component targetComp)
            knockback = (targetComp.transform.position - transform.position).normalized;

        target.TakeDamage(damage, isCritical, knockback);

        if (isCritical && target is Component comp)
            EventBus.Publish(new OnCriticalHit
            {
                damage = damage,
                worldPosition = comp.transform.position
            });
    }

    // ---- Fallback timer (gdy brak animation events) -----------

    private void TickFallback()
    {
        if (canAttack || fallbackTimer <= 0f) return;
        fallbackTimer -= Time.deltaTime;
        if (fallbackTimer <= 0f)
        {
            animator?.ResetTrigger(HashAttack);
            canAttack = true;
        }
    }

    // ---- EventBus — odświeżenie po zmianie statystyk ----------

    private void OnItemEquipped(OnItemEquipped _)     => RefreshAttackSpeed();
    private void OnItemUnequipped(OnItemUnequipped _) => RefreshAttackSpeed();
    private void OnLevelUp(OnPlayerLevelUp _)         => RefreshAttackSpeed();

    private void RefreshAttackSpeed()
    {
        if (animator != null)
            animator.SetFloat(HashAttackSpeed, stats.TotalAttackSpeed);
    }
}
