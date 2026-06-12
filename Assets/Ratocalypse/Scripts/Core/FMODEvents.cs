// ============================================================
//  FMODEvents.cs
//  Ratpocalypse — Core/FMODEvents.cs
//
//  Wszystkie referencje do eventów FMOD w jednym miejscu.
//  ScriptableObject — ustawiasz ścieżki raz w Inspectorze,
//  a reszta kodu używa AudioManager.Instance.PlayOneShot(
//      FMODEvents.Instance.playerFootsteps, position).
//
//  Tworzenie: Assets → Create → Ratpocalypse → FMOD Events
// ============================================================

using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Ratpocalypse/FMOD Events", fileName = "FMODEvents")]
public class FMODEvents : ScriptableObject
{
    // --------------------------------------------------------
    // Singleton — ładowany z Resources
    // --------------------------------------------------------
    private static FMODEvents _instance;
    public static FMODEvents Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<FMODEvents>("FMODEvents");
            return _instance;
        }
    }

    // --------------------------------------------------------
    // Gracz — ruch
    // --------------------------------------------------------
    [Header("Gracz — ruch")]
    public EventReference playerFootsteps;
    public EventReference playerDodge;

    // --------------------------------------------------------
    // Gracz — walka
    // --------------------------------------------------------
    [Header("Gracz — walka")]
    public EventReference playerAttackFist;
    public EventReference playerAttackOneHanded;
    public EventReference playerAttackTwoHanded;
    public EventReference playerAttackRanged;
    public EventReference playerHit;
    public EventReference playerDeath;
    public EventReference playerCriticalHit;

    // --------------------------------------------------------
    // Wrogowie
    // --------------------------------------------------------
    [Header("Wrogowie")]
    public EventReference enemyAttack;
    public EventReference enemyHit;
    public EventReference enemyDeath;
    public EventReference enemyAlert;       // szczur zauważył gracza

    // --------------------------------------------------------
    // UI
    // --------------------------------------------------------
    [Header("UI")]
    public EventReference uiMenuOpen;
    public EventReference uiMenuClose;
    public EventReference uiItemPickup;
    public EventReference uiItemEquip;
    public EventReference uiLevelUp;
    public EventReference uiSkillUnlock;
    public EventReference uiCheckpoint;

    // --------------------------------------------------------
    // Muzyka
    // --------------------------------------------------------
    [Header("Muzyka")]
    public EventReference musicExploration;
    public EventReference musicCombat;
    public EventReference musicBoss;

    // --------------------------------------------------------
    // Ambient
    // --------------------------------------------------------
    [Header("Ambient")]
    public EventReference ambientCity;
    public EventReference ambientSewers;
    public EventReference ambientNest;

    // --------------------------------------------------------
    // Status efekty
    // --------------------------------------------------------
    [Header("Status efekty")]
    public EventReference effectPoison;
    public EventReference effectBleed;
    public EventReference effectStun;
}