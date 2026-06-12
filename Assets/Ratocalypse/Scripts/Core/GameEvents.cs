// ============================================================
//  GameEvents.cs
//  Ratpocalypse — Core/GameEvents.cs
//
//  Wszystkie zdarzenia EventBus w jednym miejscu.
//  Ka¿de zdarzenie to struct — zero alokacji na stercie.
//
//  Konwencja nazw: On + Kto + CoCzego
//  np. OnPlayerDamaged, OnEnemyDied, OnItemPickedUp
// ============================================================

// ---- GRACZ -------------------------------------------------

/// <summary>Gracz otrzyma³ obra¿enia.</summary>
public struct OnPlayerDamaged
{
    public float amount;        // ile obra¿eñ
    public float currentHp;     // HP po obra¿eniach
    public float maxHp;
}

/// <summary>Gracz siê uleczy³.</summary>
public struct OnPlayerHealed
{
    public float amount;
    public float currentHp;
    public float maxHp;
}

/// <summary>Gracz zgin¹³.</summary>
public struct OnPlayerDied { }

/// <summary>Gracz u¿y³ staminy (uniku, ciê¿kiego ataku).</summary>
public struct OnPlayerStaminaChanged
{
    public float current;
    public float max;
}

/// <summary>Gracz zdoby³ XP.</summary>
public struct OnPlayerXPGained
{
    public float amount;
    public float totalXP;
    public float xpToNextLevel;
}

/// <summary>Gracz awansowa³ na wy¿szy poziom.</summary>
public struct OnPlayerLevelUp
{
    public int newLevel;
    public int skillPointsGained;   // ile punktów do drzewka
}

/// <summary>Gracz zmieni³ aktywn¹ broñ.</summary>
public struct OnWeaponChanged
{
    public WeaponType newWeapon;
}

// ---- WALKA -------------------------------------------------

/// <summary>Wróg otrzyma³ obra¿enia. U¿ywane do floating damage text.</summary>
public struct OnEnemyDamaged
{
    public UnityEngine.GameObject enemy;
    public float amount;
    public bool isCritical;
    public UnityEngine.Vector3 worldPosition;
}

/// <summary>Wróg zgin¹³.</summary>
public struct OnEnemyDied
{
    public UnityEngine.GameObject enemy;
    public EnemyType enemyType;
    public UnityEngine.Vector3 worldPosition;
    public float xpReward;
}

/// <summary>Gracz trafi³ krytycznie.</summary>
public struct OnCriticalHit
{
    public float damage;
    public UnityEngine.Vector3 worldPosition;
}

/// <summary>Efekt statusu zaaplikowany na cel.</summary>
public struct OnStatusEffectApplied
{
    public UnityEngine.GameObject target;
    public StatusEffectType effectType;
    public float duration;
}

// ---- EKWIPUNEK / LOOT --------------------------------------

/// <summary>Gracz podniós³ przedmiot.</summary>
public struct OnItemPickedUp
{
    public string itemId;       // tymczasowo string — zast¹pione przez ItemData w kroku 8
}

/// <summary>Gracz za³o¿y³ przedmiot.</summary>
public struct OnItemEquipped
{
    public string itemId;
    public EquipSlot slot;
}

/// <summary>Gracz zdj¹³ przedmiot.</summary>
public struct OnItemUnequipped
{
    public string itemId;
    public EquipSlot slot;
}

// ---- MISJE -------------------------------------------------

/// <summary>Misja rozpoczêta.</summary>
public struct OnQuestStarted
{
    public string questId;
    public string questName;
}

/// <summary>Postêp misji (np. zabito kolejnego szczura).</summary>
public struct OnQuestProgressUpdated
{
    public string questId;
    public int current;
    public int required;
}

/// <summary>Misja ukoñczona.</summary>
public struct OnQuestCompleted
{
    public string questId;
    public float xpReward;
}

// ---- DRZEWKO UMIEJÊTNOŒCI ----------------------------------

/// <summary>Gracz odblokowa³ umiejêtnoœæ.</summary>
public struct OnSkillUnlocked
{
    public string skillId;
    public SkillBranch branch;
    public int remainingPoints;
}

// ---- UI / MENU ---------------------------------------------

/// <summary>Ekwipunek otwarty / zamkniêty.</summary>
public struct OnInventoryToggled
{
    public bool isOpen;
}

/// <summary>Gra wstrzymana / wznowiona.</summary>
public struct OnGamePaused
{
    public bool isPaused;
}

/// <summary>Gracz dotar³ do punktu zapisu.</summary>
public struct OnCheckpointReached
{
    public string checkpointId;
}

/// <summary>Gra zapisana.</summary>
public struct OnGameSaved
{
    public string checkpointId;
}

// ---- SCENA -------------------------------------------------

/// <summary>Scena / obszar zosta³ za³adowany.</summary>
public struct OnSceneLoaded
{
    public string sceneName;
}

// ============================================================
//  Enumeratory u¿ywane przez zdarzenia
// ============================================================

public enum WeaponType
{
    Fists,          // go³e piêœci (domyœlne)
    OneHanded,      // broñ jednorêczna (nó¿, siekiera)
    TwoHanded,      // broñ oburêczna (rura, m³ot)
    Ranged          // broñ dystansowa (uzi, pistolet)
}

public enum EnemyType
{
    CommonRat,
    Bloater,
    Spitter,
    Leaper,
    AlphaRat,
    RatKing
}

public enum StatusEffectType
{
    Poison,
    Bleed,
    Stun,
    Slow
}

public enum EquipSlot
{
    Weapon,
    Armor,
    Accessory1,
    Accessory2
}

public enum SkillBranch
{
    Warrior,
    Hunter,
    Survivor
}