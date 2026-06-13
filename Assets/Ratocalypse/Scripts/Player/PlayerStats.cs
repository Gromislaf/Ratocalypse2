using UnityEngine;

[CreateAssetMenu(menuName = "Ratpocalypse/Player Stats", fileName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Życie")]
    [Tooltip("Maksymalne HP na poziomie 1")]
    public float baseMaxHp = 100f;
    [Tooltip("O ile rośnie maxHP za każdy level")]
    public float hpPerLevel = 15f;
    [Tooltip("Regeneracja HP na sekundę (0 = brak)")]
    public float hpRegenPerSecond = 0f;

    [Header("Stamina")]
    [Tooltip("Maksymalna stamina")]
    public float baseMaxStamina = 100f;
    [Tooltip("Regeneracja staminy na sekundę")]
    public float staminaRegenPerSecond = 20f;
    [Tooltip("Koszt staminy na unik")]
    public float dodgeCost = 25f;

    [Header("Obrażenia")]
    [Tooltip("Bazowe obrażenia bez broni")]
    public float baseDamage = 10f;
    [Tooltip("O ile rosną obrażenia za każdy level")]
    public float damagePerLevel = 2f;

    [Header("Szybkość")]
    [Tooltip("Prędkość poruszania się (NavMeshAgent.speed)")]
    public float moveSpeed = 5f;
    [Tooltip("Prędkość uniku")]
    public float dodgeSpeed = 12f;
    [Tooltip("Czas trwania uniku w sekundach")]
    public float dodgeDuration = 0.3f;

    [Header("Szybkość ataku")]
    [Tooltip("Mnożnik animacji ataku (1 = normalna, 1.5 = 50% szybciej)")]
    public float baseAttackSpeed = 1f;
    [Tooltip("Wzrost szybkości ataku za każdy level")]
    public float attackSpeedPerLevel = 0.02f;

    [Header("Obrona")]
    [Tooltip("Procentowa redukcja obrażeń (0–1)")]
    [Range(0f, 0.75f)]
    public float baseArmor = 0f;

    [Header("Krytyki")]
    [Tooltip("Szansa na trafienie krytyczne (0–1)")]
    [Range(0f, 1f)]
    public float baseCritChance = 0.05f;
    [Tooltip("Mnożnik obrażeń krytycznych")]
    public float critMultiplier = 2f;

    [Header("Progresja — XP")]
    [Tooltip("XP potrzebne do osiągnięcia level 2")]
    public float xpForLevel2 = 100f;
    [Tooltip("Mnożnik XP per level (level 3 = xpForLevel2 * multiplier)")]
    public float xpScalingMultiplier = 1.4f;
    [Tooltip("Maksymalny poziom gracza")]
    public int maxLevel = 20;

    // ---- Runtime (reset przy każdej nowej grze) ----
    [System.NonSerialized] public float currentHp;
    [System.NonSerialized] public float currentStamina;
    [System.NonSerialized] public float currentXP;
    [System.NonSerialized] public int currentLevel = 1;
    [System.NonSerialized] public int availableSkillPoints = 0;

    // Bonusy z ekwipunku i umiejętności (addytywne, reset przy przeliczeniu ekwipunku)
    [System.NonSerialized] public float bonusDamage = 0f;
    [System.NonSerialized] public float bonusArmor = 0f;
    [System.NonSerialized] public float bonusCritChance = 0f;
    [System.NonSerialized] public float bonusMoveSpeed = 0f;
    [System.NonSerialized] public float bonusMaxHp = 0f;
    [System.NonSerialized] public float bonusAttackSpeed = 0f;

    // ---- Właściwości obliczane ----
    public float MaxHp           => baseMaxHp + (hpPerLevel * (currentLevel - 1)) + bonusMaxHp;
    public float MaxStamina      => baseMaxStamina;
    public float TotalDamage     => baseDamage + (damagePerLevel * (currentLevel - 1)) + bonusDamage;
    public float TotalArmor      => Mathf.Clamp01(baseArmor + bonusArmor);
    public float TotalCritChance => Mathf.Clamp01(baseCritChance + bonusCritChance);
    public float TotalMoveSpeed  => moveSpeed + bonusMoveSpeed;
    public float TotalAttackSpeed => baseAttackSpeed + (attackSpeedPerLevel * (currentLevel - 1)) + bonusAttackSpeed;

    public float XPRequiredForNextLevel
    {
        get
        {
            if (currentLevel >= maxLevel) return float.MaxValue;
            return Mathf.Round(xpForLevel2 * Mathf.Pow(xpScalingMultiplier, currentLevel - 1));
        }
    }

    // Resetuje runtime w Editorze między sesjami Play, żeby nie zaczynać z poprzednim stanem.
    private void OnEnable()
    {
        InitializeForNewGame();
    }

    public void InitializeForNewGame()
    {
        currentLevel = 1;
        currentHp = MaxHp;
        currentStamina = MaxStamina;
        currentXP = 0f;
        availableSkillPoints = 0;
        ResetBonuses();
    }

    public void ResetBonuses()
    {
        bonusDamage = 0f;
        bonusArmor = 0f;
        bonusCritChance = 0f;
        bonusMoveSpeed = 0f;
        bonusMaxHp = 0f;
        bonusAttackSpeed = 0f;
    }

    // Poniższe metody modyfikują tylko dane — zdarzenia EventBus publikuje PlayerHealthComponent.

    public float TakeDamage(float rawDamage)
    {
        float reduced = rawDamage * (1f - TotalArmor);
        reduced = Mathf.Max(reduced, 1f);
        currentHp = Mathf.Max(currentHp - reduced, 0f);
        return reduced;
    }

    public float Heal(float amount)
    {
        float actual = Mathf.Min(amount, MaxHp - currentHp);
        currentHp += actual;
        return actual;
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount) return false;
        currentStamina = Mathf.Max(currentStamina - amount, 0f);
        return true;
    }

    // Zwraca true gdy stamina wzrosła — PlayerHealthComponent publikuje event tylko wtedy.
    public bool RegenerateStamina(float deltaTime)
    {
        if (currentStamina >= MaxStamina) return false;
        currentStamina = Mathf.Min(currentStamina + staminaRegenPerSecond * deltaTime, MaxStamina);
        return true;
    }

    // Zwraca liczbę zdobytych poziomów (zazwyczaj 0 lub 1).
    public int GainXP(float amount)
    {
        if (currentLevel >= maxLevel) return 0;
        currentXP += amount;
        int levelsGained = 0;
        while (currentXP >= XPRequiredForNextLevel && currentLevel < maxLevel)
        {
            currentXP -= XPRequiredForNextLevel;
            LevelUp();
            levelsGained++;
        }
        return levelsGained;
    }

    private void LevelUp()
    {
        float oldMaxHp = MaxHp;
        currentLevel++;
        availableSkillPoints++;
        // Skaluje HP proporcjonalnie — gracz nie leczy się przez level up, ale nie traci HP
        currentHp = Mathf.Min((currentHp / oldMaxHp) * MaxHp, MaxHp);
    }

    public bool RollCrit()
    {
        return Random.value < TotalCritChance;
    }

    public float CalculateDamage(float weaponDamageBonus = 0f)
    {
        float damage = TotalDamage + weaponDamageBonus;
        if (RollCrit())
            damage *= critMultiplier;
        return Mathf.Round(damage);
    }
}
