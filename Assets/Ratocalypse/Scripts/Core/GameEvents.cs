// ============================================================
//  GameEvents.cs
//  Ratpocalypse � Core/GameEvents.cs
//
//  Wszystkie zdarzenia EventBus w jednym miejscu.
//  Ka�de zdarzenie to struct � zero alokacji na stercie.
//
//  Konwencja nazw: On + Kto + CoCzego
//  np. OnPlayerDamaged, OnEnemyDied, OnItemPickedUp
// ============================================================

// ---- GRACZ -------------------------------------------------

/// <summary>Gracz otrzyma� obra�enia.</summary>
public struct OnPlayerDamaged
{
    public float amount;        // ile obra�e�
    public float currentHp;     // HP po obra�eniach
    public float maxHp;
}

/// <summary>Gracz si� uleczy�.</summary>
public struct OnPlayerHealed
{
    public float amount;
    public float currentHp;
    public float maxHp;
}

/// <summary>Gracz zgin��.</summary>
public struct OnPlayerDied { }

/// <summary>Gracz u�y� staminy (uniku, ci�kiego ataku).</summary>
public struct OnPlayerStaminaChanged
{
    public float current;
    public float max;
}

/// <summary>Gracz zdoby� XP.</summary>
public struct OnPlayerXPGained
{
    public float amount;
    public float totalXP;
    public float xpToNextLevel;
}

/// <summary>Gracz awansowa� na wy�szy poziom.</summary>
public struct OnPlayerLevelUp
{
    public int newLevel;
    public int skillPointsGained;   // ile punkt�w do drzewka
}

/// <summary>Gracz zmieni� aktywn� bro�.</summary>
public struct OnWeaponChanged
{
    public WeaponType newWeapon;
}

// ---- WALKA -------------------------------------------------

/// <summary>Wr�g otrzyma� obra�enia. U�ywane do floating damage text.</summary>
public struct OnEnemyDamaged
{
    public UnityEngine.GameObject enemy;
    public float amount;
    public bool isCritical;
    public UnityEngine.Vector3 worldPosition;
}

/// <summary>Wr�g zgin��.</summary>
public struct OnEnemyDied
{
    public UnityEngine.GameObject enemy;
    public EnemyType enemyType;
    public UnityEngine.Vector3 worldPosition;
    public float xpReward;
}

/// <summary>Gracz trafi� krytycznie.</summary>
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

/// <summary>Gracz podnisl przedmiot — trafil do plecaka.</summary>
public struct OnItemPickedUp
{
    public ItemData item;
}

/// <summary>Gracz zalozyl przedmiot.</summary>
public struct OnItemEquipped
{
    public ItemData item;
    public EquipmentSlot slot;
}

/// <summary>Gracz zdjal przedmiot — wrocil do plecaka.</summary>
public struct OnItemUnequipped
{
    public ItemData item;
    public EquipmentSlot slot;
}

/// <summary>Zawartosc plecaka sie zmienila (dodano / usunieto item).</summary>
public struct OnInventoryChanged { }

// ---- MISJE -------------------------------------------------

/// <summary>Misja rozpocz�ta.</summary>
public struct OnQuestStarted
{
    public string questId;
    public string questName;
}

/// <summary>Post�p misji (np. zabito kolejnego szczura).</summary>
public struct OnQuestProgressUpdated
{
    public string questId;
    public int current;
    public int required;
}

/// <summary>Misja uko�czona.</summary>
public struct OnQuestCompleted
{
    public string questId;
    public float xpReward;
}

// ---- DRZEWKO UMIEJ�TNO�CI ----------------------------------

/// <summary>Gracz odblokowa� umiej�tno��.</summary>
public struct OnSkillUnlocked
{
    public string skillId;
    public SkillBranch branch;
    public int remainingPoints;
}

// ---- UI / MENU ---------------------------------------------

/// <summary>Ekwipunek otwarty / zamkni�ty.</summary>
public struct OnInventoryToggled
{
    public bool isOpen;
}

/// <summary>Drzewko umiejętności otwarte / zamknięte.</summary>
public struct OnSkillTreeToggled
{
    public bool isOpen;
}

/// <summary>Gra wstrzymana / wznowiona.</summary>
public struct OnGamePaused
{
    public bool isPaused;
}

/// <summary>Gracz dotar� do punktu zapisu.</summary>
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

/// <summary>Scena / obszar zosta� za�adowany.</summary>
public struct OnSceneLoaded
{
    public string sceneName;
}

// ============================================================
//  Enumeratory u�ywane przez zdarzenia
// ============================================================

public enum WeaponType
{
    Fists,          // go�e pi�ci (domy�lne)
    OneHanded,      // bro� jednor�czna (n�, siekiera)
    TwoHanded,      // bro� obur�czna (rura, m�ot)
    Ranged          // bro� dystansowa (uzi, pistolet)
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

public enum EquipmentSlot
{
    MainHand,
    OffHand,
    Helmet,
    Chest,
    Legs,
    Boots
}

public enum SkillBranch
{
    Warrior,
    Hunter,
    Survivor
}