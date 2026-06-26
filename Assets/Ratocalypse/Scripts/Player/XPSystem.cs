using UnityEngine;

/// <summary>
/// Sluch OnEnemyDied i OnQuestCompleted → wywoluje GainXP na PlayerHealthComponent.
/// Dodaj na tym samym GO co PlayerHealthComponent.
/// </summary>
[RequireComponent(typeof(PlayerHealthComponent))]
public class XPSystem : MonoBehaviour
{
    private PlayerHealthComponent playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealthComponent>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<OnEnemyDied>(OnEnemyDied);
        EventBus.Subscribe<OnQuestCompleted>(OnQuestCompleted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnEnemyDied>(OnEnemyDied);
        EventBus.Unsubscribe<OnQuestCompleted>(OnQuestCompleted);
    }

    private void OnEnemyDied(OnEnemyDied e)
    {
        playerHealth.GainXP(e.xpReward);
    }

    private void OnQuestCompleted(OnQuestCompleted e)
    {
        playerHealth.GainXP(e.xpReward);
    }
}
