using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerHealthComponent playerHealth;
    [SerializeField] private Animator animator;

    [Header("Warstwy raycastu")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Walka")]
    [Tooltip("Dystans, w którym zatrzymujemy się przy wrogu i oddajemy mu atak PlayerCombat")]
    [SerializeField] private float attackRange = 1.5f;

    private NavMeshAgent agent;
    private Camera mainCamera;

    private IDamageable currentTarget;
    private Transform currentTargetTransform;
    private bool isAttackingTarget;
    private bool isDodging;

    // Animator parameter hashes — obliczane raz, bez GC przy każdej klatce
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashDodge = Animator.StringToHash("Dodge");

    // ---- Lifecycle --------------------------------------------

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;

        agent.speed = stats.TotalMoveSpeed;
        agent.updateRotation = false; // obrót robimy sami przez Slerp
        agent.angularSpeed = 0f;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnPlayerLevelUp>(OnLevelUp);
        EventBus.Subscribe<OnItemEquipped>(OnItemEquipped);
        EventBus.Subscribe<OnItemUnequipped>(OnItemUnequipped);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnPlayerLevelUp>(OnLevelUp);
        EventBus.Unsubscribe<OnItemEquipped>(OnItemEquipped);
        EventBus.Unsubscribe<OnItemUnequipped>(OnItemUnequipped);
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused) return;

        HandleMovementInput();
        HandleDodgeInput();
        PursueTarget();
        FaceMovementDirection();
        UpdateAnimator();
    }

    // ---- Input ------------------------------------------------

    private void HandleMovementInput()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (IsPointerOverUI()) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Priorytet: wróg > ziemia
        if (Physics.Raycast(ray, out RaycastHit enemyHit, 100f, enemyLayer))
        {
            SetTarget(enemyHit.collider.GetComponent<IDamageable>(), enemyHit.transform);
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit groundHit, 100f, groundLayer))
        {
            ClearTarget();
            MoveToPoint(groundHit.point);
        }
    }

    private void HandleDodgeInput()
    {
        if (isDodging) return;

        bool shiftHeld = Keyboard.current.leftShiftKey.isPressed
                      || Keyboard.current.rightShiftKey.isPressed;
        if (!shiftHeld || !Mouse.current.rightButton.wasPressedThisFrame) return;
        if (!playerHealth.UseStamina(stats.dodgeCost)) return;

        // Unikamy w kierunku ruchu — jeśli stoimy, to do przodu
        Vector3 dir = agent.velocity.sqrMagnitude > 0.1f
            ? agent.velocity.normalized
            : transform.forward;

        StartCoroutine(DodgeCoroutine(dir));
    }

    // ---- Ruch -------------------------------------------------

    private void MoveToPoint(Vector3 destination)
    {
        isAttackingTarget = false;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    private void SetTarget(IDamageable target, Transform targetTransform)
    {
        currentTarget = target;
        currentTargetTransform = targetTransform;
        isAttackingTarget = true;
        agent.isStopped = false;
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentTargetTransform = null;
        isAttackingTarget = false;
    }

    private void PursueTarget()
    {
        if (!isAttackingTarget || currentTargetTransform == null) return;

        if (currentTarget is { IsAlive: false })
        {
            ClearTarget();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTargetTransform.position);
        if (dist <= attackRange)
        {
            agent.isStopped = true;
            FacePosition(currentTargetTransform.position);
            // PlayerCombat odczyta CurrentTarget przez property i wykona atak
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(currentTargetTransform.position);
        }
    }

    // ---- Unik -------------------------------------------------

    private System.Collections.IEnumerator DodgeCoroutine(Vector3 direction)
    {
        isDodging = true;
        playerHealth.IsInvincible = true; // iframes
        animator.SetTrigger(HashDodge);

        // Odłączamy NavMesh od Transform na czas uniku — przesuwamy transform bezpośrednio
        agent.isStopped = true;
        agent.updatePosition = false;
        ClearTarget();

        float elapsed = 0f;
        while (elapsed < stats.dodgeDuration)
        {
            transform.position += direction * stats.dodgeSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Warp do najbliższego punktu na NavMesh — zabezpieczenie gdy unik skończył się poza siatką
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
            agent.Warp(transform.position);

        agent.updatePosition = true;
        agent.isStopped = false;
        playerHealth.IsInvincible = false;
        isDodging = false;
    }

    // ---- Obrót ------------------------------------------------

    private void FaceMovementDirection()
    {
        if (isDodging || isAttackingTarget) return;
        if (agent.velocity.sqrMagnitude < 0.01f) return;
        FacePosition(transform.position + agent.velocity);
    }

    private void FacePosition(Vector3 worldPosition)
    {
        Vector3 dir = (worldPosition - transform.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 15f);
    }

    // ---- Animator ---------------------------------------------

    private void UpdateAnimator()
    {
        if (animator == null) return;
        float speed = agent.velocity.magnitude / Mathf.Max(stats.TotalMoveSpeed, 0.01f);
        animator.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);
    }

    // ---- EventBus — odświeżenie prędkości po zmianie statystyk ----

    private void OnLevelUp(OnPlayerLevelUp _)        => agent.speed = stats.TotalMoveSpeed;
    private void OnItemEquipped(OnItemEquipped _)     => agent.speed = stats.TotalMoveSpeed;
    private void OnItemUnequipped(OnItemUnequipped _) => agent.speed = stats.TotalMoveSpeed;

    // ---- Helpers ----------------------------------------------

    private bool IsPointerOverUI() =>
        EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

    // ---- API dla PlayerCombat ---------------------------------

    /// <summary>Aktualny cel ataku — null jeśli brak lub martwy.</summary>
    public IDamageable CurrentTarget => currentTarget;

    /// <summary>Czy gracz jest w zasięgu ataku aktualnego celu.</summary>
    public bool IsInAttackRange => isAttackingTarget
        && currentTargetTransform != null
        && Vector3.Distance(transform.position, currentTargetTransform.position) <= attackRange;

    /// <summary>Czy gracz jest w trakcie uniku (iframes aktywne).</summary>
    public bool IsDodging => isDodging;
}
