using UnityEngine;

public enum ItemType
{
    Weapon,
    Shield,
    Helmet,
    Chest,
    Legs,
    Boots,
    Consumable
}

[CreateAssetMenu(menuName = "Ratpocalypse/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    [Header("Identyfikacja")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType itemType;

    [Header("Bron (tylko Weapon)")]
    public WeaponType weaponType;
    public bool isTwoHanded;

    [Header("Bonusy statystyk")]
    public float bonusDamage;
    [Range(0f, 0.75f)] public float bonusArmor;
    [Range(0f, 1f)] public float bonusCritChance;
    public float bonusMoveSpeed;
    public float bonusMaxHp;
    public float bonusAttackSpeed;

    [Header("Konsumable (tylko Consumable)")]
    public float healAmount;
    public float staminaRestore;
}
