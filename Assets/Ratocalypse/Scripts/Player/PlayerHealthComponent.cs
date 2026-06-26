using UnityEngine;

// Warstwa logiki i zdarzeń dla HP/Staminy/XP gracza.
// PlayerStats trzyma dane i obliczenia — ten komponent je modyfikuje i publikuje do EventBus.
// Implementuje IDamageable i IHealable, dzięki czemu wrogowie nie wiedzą z czym walczą.
public class PlayerHealthComponent : MonoBehaviour, IDamageable, IHealable
{
    [SerializeField] private PlayerStats stats;

    public bool IsAlive => stats != null && stats.currentHp > 0f;

    /// <summary>Iframes — ustawiane przez PlayerController podczas uniku.</summary>
    public bool IsInvincible { get; set; } = false;

    private void Awake()
    {
        if (stats == null)
            Debug.LogError("[PlayerHealthComponent] PlayerStats nie przypisany!", this);
    }

    private void Update()
    {
        if (stats.RegenerateStamina(Time.deltaTime))
            EventBus.Publish(new OnPlayerStaminaChanged { current = stats.currentStamina, max = stats.MaxStamina });

        if (stats.TotalHpRegen > 0f && stats.currentHp > 0f && stats.currentHp < stats.MaxHp)
        {
            float healed = stats.Heal(stats.TotalHpRegen * Time.deltaTime);
            if (healed > 0f)
                EventBus.Publish(new OnPlayerHealed { amount = healed, currentHp = stats.currentHp, maxHp = stats.MaxHp });
        }
    }

    // IDamageable — wywoływane przez wrogów przez interfejs; knockback obsłuży PlayerController
    void IDamageable.TakeDamage(float damage, bool isCritical, Vector3 knockbackDirection)
    {
        TakeDamage(damage);
    }

    public float TakeDamage(float rawDamage)
    {
        if (IsInvincible) return 0f;
        float actual = stats.TakeDamage(rawDamage);
        EventBus.Publish(new OnPlayerDamaged { amount = actual, currentHp = stats.currentHp, maxHp = stats.MaxHp });
        if (stats.currentHp <= 0f)
            EventBus.Publish(new OnPlayerDied());
        return actual;
    }

    // IHealable
    public void Heal(float amount)
    {
        float actual = stats.Heal(amount);
        if (actual <= 0f) return;
        EventBus.Publish(new OnPlayerHealed { amount = actual, currentHp = stats.currentHp, maxHp = stats.MaxHp });
    }

    public bool UseStamina(float amount)
    {
        bool success = stats.UseStamina(amount);
        if (success)
            EventBus.Publish(new OnPlayerStaminaChanged { current = stats.currentStamina, max = stats.MaxStamina });
        return success;
    }

    public void RestoreStamina(float amount)
    {
        if (amount <= 0f) return;
        stats.currentStamina = Mathf.Min(stats.currentStamina + amount, stats.MaxStamina);
        EventBus.Publish(new OnPlayerStaminaChanged { current = stats.currentStamina, max = stats.MaxStamina });
    }

    // Zwraca true jeśli gracz awansował — np. do efektu wizualnego level-up.
    public bool GainXP(float amount)
    {
        if (stats.currentLevel >= stats.maxLevel) return false;
        int levelsGained = stats.GainXP(amount);
        EventBus.Publish(new OnPlayerXPGained
        {
            amount = amount,
            totalXP = stats.currentXP,
            xpToNextLevel = stats.XPRequiredForNextLevel
        });
        if (levelsGained > 0)
        {
            EventBus.Publish(new OnPlayerLevelUp
            {
                newLevel = stats.currentLevel,
                skillPointsGained = levelsGained
            });
        }
        return levelsGained > 0;
    }
}
