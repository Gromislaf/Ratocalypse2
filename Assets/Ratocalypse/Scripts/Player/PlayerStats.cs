// ============================================================
//  PlayerStats.cs
//  Ratpocalypse — Player/PlayerStats.cs
//
//  ScriptableObject przechowujący wszystkie statystyki gracza.
//  Dane oddzielone od logiki — wiele systemów może czytać
//  ten sam asset bez twardych referencji między sobą.
//
//  Tworzenie: Assets → Create → Ratpocalypse → Player Stats
// ============================================================

using UnityEngine;

[CreateAssetMenu(menuName = "Ratpocalypse/Player Stats", fileName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    // --------------------------------------------------------
    // Dane bazowe (ustawiane w Inspectorze, nie zmieniają się)
    // --------------------------------------------------------

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

    // --------------------------------------------------------
    // Dane runtime (zmieniane podczas gry — RESET przy nowej grze)
    // --------------------------------------------------------

    [System.NonSerialized] public float currentHp;
    [System.NonSerialized] public float currentStamina;
    [System.NonSerialized] public float currentXP;
    [System.NonSerialized] public int currentLevel = 1;
    [System.NonSerialized] public int availableSkillPoints = 0;

    // Bonusy z ekwipunku i umiejętności (addytywne)
    [System.NonSerialized] public float bonusDamage = 0f;
    [System.NonSerialized] public float bonusArmor = 0f;
    [System.NonSerialized] public float bonusCritChance = 0f;
    [System.NonSerialized] public float bonusMoveSpeed = 0f;
    [System.NonSerialized] public float bonusMaxHp = 0f;

    // --------------------------------------------------------
    // Właściwości obliczane (finalne wartości używane przez kod)
    // --------------------------------------------------------

    public float MaxHp => baseMaxHp + (hpPerLevel * (currentLevel - 1)) + bonusMaxHp;
    public float MaxStamina => baseMaxStamina;
    public float TotalDamage => baseDamage + (damagePerLevel * (currentLevel - 1)) + bonusDamage;
    public float TotalArmor => Mathf.Clamp01(baseArmor + bonusArmor);
    public float TotalCritChance => Mathf.Clamp01(baseCritChance + bonusCritChance);
    public float TotalMoveSpeed => moveSpeed + bonusMoveSpeed;

    /// <summary>XP potrzebne do następnego poziomu od zera.</summary>
    public float XPRequiredForNextLevel
    {
        get
        {
            if (currentLevel >= maxLevel) return float.MaxValue;
            return Mathf.Round(xpForLevel2 * Mathf.Pow(xpScalingMultiplier, currentLevel - 1));
        }
    }

    // --------------------------------------------------------
    // Metody
    // --------------------------------------------------------

    /// <summary>
    /// Inicjalizacja na początku nowej gry lub po wczytaniu save'a.
    /// Wywołaj z GameManager.StartNewGame() lub SaveSystem.Load().
    /// </summary>
    public void InitializeForNewGame()
    {
        currentLevel = 1;
        currentHp = MaxHp;
        currentStamina = MaxStamina;
        currentXP = 0f;
        availableSkillPoints = 0;
        ResetBonuses();
    }

    /// <summary>Zeruje bonusy — wywołaj przy każdorazowym przeliczeniu ekwipunku.</summary>
    public void ResetBonuses()
    {
        bonusDamage = 0f;
        bonusArmor = 0f;
        bonusCritChance = 0f;
        bonusMoveSpeed = 0f;
        bonusMaxHp = 0f;
    }

    /// <summary>
    /// Zadaj obrażenia graczowi. Uwzględnia pancerz.
    /// Zwraca faktyczną ilość zadanych obrażeń po redukcji.
    /// </summary>
    public float TakeDamage(float rawDamage)
    {
        float reduced = rawDamage * (1f - TotalArmor);
        reduced = Mathf.Max(reduced, 1f);   // minimum 1 obrażenie zawsze
        currentHp = Mathf.Max(currentHp - reduced, 0f);

        EventBus.Publish(new OnPlayerDamaged
        {
            amount = reduced,
            currentHp = currentHp,
            maxHp = MaxHp
        });

        if (currentHp <= 0f)
            EventBus.Publish(new OnPlayerDied());

        return reduced;
    }

    /// <summary>Ulecz gracza. Nie przekroczy MaxHp.</summary>
    public void Heal(float amount)
    {
        float actual = Mathf.Min(amount, MaxHp - currentHp);
        currentHp += actual;

        EventBus.Publish(new OnPlayerHealed
        {
            amount = actual,
            currentHp = currentHp,
            maxHp = MaxHp
        });
    }

    /// <summary>
    /// Używa staminy. Zwraca false jeśli za mało staminy.
    /// </summary>
    public bool UseStamina(float amount)
    {
        if (currentStamina < amount) return false;

        currentStamina = Mathf.Max(currentStamina - amount, 0f);
        EventBus.Publish(new OnPlayerStaminaChanged
        {
            current = currentStamina,
            max = MaxStamina
        });
        return true;
    }

    /// <summary>Regeneruje staminę (wywołuj co klatkę z delta time).</summary>
    public void RegenerateStamina(float deltaTime)
    {
        if (currentStamina >= MaxStamina) return;

        currentStamina = Mathf.Min(currentStamina + staminaRegenPerSecond * deltaTime, MaxStamina);
        EventBus.Publish(new OnPlayerStaminaChanged
        {
            current = currentStamina,
            max = MaxStamina
        });
    }

    /// <summary>
    /// Dodaj XP. Automatycznie sprawdza czy należy awansować.
    /// </summary>
    public void GainXP(float amount)
    {
        if (currentLevel >= maxLevel) return;

        currentXP += amount;
        EventBus.Publish(new OnPlayerXPGained
        {
            amount = amount,
            totalXP = currentXP,
            xpToNextLevel = XPRequiredForNextLevel
        });

        // Sprawdź czy można awansować (możliwy multi-level-up)
        while (currentXP >= XPRequiredForNextLevel && currentLevel < maxLevel)
        {
            currentXP -= XPRequiredForNextLevel;
            LevelUp();
        }
    }

    /// <summary>Prywatna metoda awansu. Wywoływana automatycznie przez GainXP.</summary>
    void LevelUp()
    {
        currentLevel++;
        availableSkillPoints++;

        // Przywróć HP proporcjonalnie (nie pełne — gracz nie powinien leczyć się przez level up)
        float hpPercent = currentHp / (MaxHp - hpPerLevel);
        currentHp = Mathf.Min(hpPercent * MaxHp, MaxHp);

        EventBus.Publish(new OnPlayerLevelUp
        {
            newLevel = currentLevel,
            skillPointsGained = 1
        });
    }

    /// <summary>
    /// Sprawdza czy atak jest krytyczny (losowanie).
    /// </summary>
    public bool RollCrit()
    {
        return Random.value < TotalCritChance;
    }

    /// <summary>
    /// Oblicza finalne obrażenia ataku (z krytykiem jeśli applicable).
    /// </summary>
    public float CalculateDamage(float weaponDamageBonus = 0f)
    {
        float damage = TotalDamage + weaponDamageBonus;
        if (RollCrit())
            damage *= critMultiplier;
        return Mathf.Round(damage);
    }
}