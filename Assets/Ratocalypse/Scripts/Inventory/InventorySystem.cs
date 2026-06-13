using System.Collections.Generic;
using UnityEngine;

// Umiec na graczu.
// Zarzadza plecakiem (20 slotow) i 6 slotami ekwipunku.
// Po kazdej zmianie ekwipunku przelicza bonusy w PlayerStats.
public class InventorySystem : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerHealthComponent health;
    [SerializeField] private int bagCapacity = 20;

    private readonly List<ItemData> bag = new();
    private readonly Dictionary<EquipmentSlot, ItemData> equipped = new();

    public IReadOnlyList<ItemData> Bag => bag;
    public int BagCapacity => bagCapacity;

    private void Awake()
    {
        if (stats == null)   Debug.LogError("[InventorySystem] PlayerStats nie przypisany!", this);
        if (health == null)  Debug.LogError("[InventorySystem] PlayerHealthComponent nie przypisany!", this);
    }

    // --- Plecak ---

    public bool AddToBag(ItemData item)
    {
        if (item == null) return false;
        if (bag.Count >= bagCapacity)
        {
            Debug.Log("[InventorySystem] Plecak pelny.");
            return false;
        }
        bag.Add(item);
        EventBus.Publish(new OnItemPickedUp { item = item });
        EventBus.Publish(new OnInventoryChanged());
        return true;
    }

    public bool RemoveFromBag(ItemData item)
    {
        if (!bag.Remove(item)) return false;
        EventBus.Publish(new OnInventoryChanged());
        return true;
    }

    // --- Ekwipunek ---

    public ItemData GetEquipped(EquipmentSlot slot) =>
        equipped.TryGetValue(slot, out var item) ? item : null;

    // Zaklada item z plecaka do wskazanego slotu.
    // Jesli slot zajety — poprzedni item wraca do plecaka.
    // Zwraca false gdy item nie pasuje do slotu lub plecak pelny przy zamianie.
    public bool Equip(ItemData item, EquipmentSlot targetSlot)
    {
        if (item == null) return false;
        if (!bag.Contains(item)) return false;
        if (!IsValidSlot(item, targetSlot)) return false;

        // Bron dwureczna w MainHand — OffHand wraca do plecaka
        if (item.isTwoHanded && targetSlot == EquipmentSlot.MainHand)
        {
            if (!UnequipToFreeSlot(EquipmentSlot.OffHand)) return false;
        }

        // Cos w OffHand gdy MainHand ma bron dwureczna — MainHand wraca do plecaka
        if (targetSlot == EquipmentSlot.OffHand)
        {
            var mainHand = GetEquipped(EquipmentSlot.MainHand);
            if (mainHand != null && mainHand.isTwoHanded)
            {
                if (!UnequipToFreeSlot(EquipmentSlot.MainHand)) return false;
            }
        }

        // Jesli slot zajety — wypchnij obecny item do plecaka
        if (!UnequipToFreeSlot(targetSlot)) return false;

        bag.Remove(item);
        equipped[targetSlot] = item;
        RecalculateBonuses();
        EventBus.Publish(new OnItemEquipped { item = item, slot = targetSlot });
        EventBus.Publish(new OnInventoryChanged());
        return true;
    }

    // Zdejmuje item ze slotu i wraca do plecaka.
    public bool Unequip(EquipmentSlot slot)
    {
        var item = GetEquipped(slot);
        if (item == null) return false;
        if (bag.Count >= bagCapacity)
        {
            Debug.Log("[InventorySystem] Plecak pelny — nie mozna zdjac przedmiotu.");
            return false;
        }
        equipped.Remove(slot);
        bag.Add(item);
        RecalculateBonuses();
        EventBus.Publish(new OnItemUnequipped { item = item, slot = slot });
        EventBus.Publish(new OnInventoryChanged());
        return true;
    }

    // Uzywa konsumabla z plecaka.
    public bool UseConsumable(ItemData item)
    {
        if (item == null || item.itemType != ItemType.Consumable) return false;
        if (!bag.Contains(item)) return false;

        if (item.healAmount > 0f)      health.Heal(item.healAmount);
        if (item.staminaRestore > 0f)  health.RestoreStamina(item.staminaRestore);

        bag.Remove(item);
        EventBus.Publish(new OnInventoryChanged());
        return true;
    }

    // --- Wewnetrzne ---

    private void RecalculateBonuses()
    {
        stats.ResetBonuses();
        foreach (var item in equipped.Values)
        {
            if (item == null) continue;
            stats.bonusDamage      += item.bonusDamage;
            stats.bonusArmor       += item.bonusArmor;
            stats.bonusCritChance  += item.bonusCritChance;
            stats.bonusMoveSpeed   += item.bonusMoveSpeed;
            stats.bonusMaxHp       += item.bonusMaxHp;
            stats.bonusAttackSpeed += item.bonusAttackSpeed;
        }
    }

    // Zdejmuje item ze slotu do plecaka (jesli cos tam jest). Zwraca false gdy plecak pelny.
    private bool UnequipToFreeSlot(EquipmentSlot slot)
    {
        var item = GetEquipped(slot);
        if (item == null) return true;
        if (bag.Count >= bagCapacity)
        {
            Debug.Log($"[InventorySystem] Plecak pelny — nie mozna zwolnic slotu {slot}.");
            return false;
        }
        equipped.Remove(slot);
        bag.Add(item);
        EventBus.Publish(new OnItemUnequipped { item = item, slot = slot });
        return true;
    }

    private bool IsValidSlot(ItemData item, EquipmentSlot slot)
    {
        return item.itemType switch
        {
            ItemType.Weapon     => slot == EquipmentSlot.MainHand ||
                                   (slot == EquipmentSlot.OffHand && !item.isTwoHanded),
            ItemType.Shield     => slot == EquipmentSlot.OffHand,
            ItemType.Helmet     => slot == EquipmentSlot.Helmet,
            ItemType.Chest      => slot == EquipmentSlot.Chest,
            ItemType.Legs       => slot == EquipmentSlot.Legs,
            ItemType.Boots      => slot == EquipmentSlot.Boots,
            _                   => false
        };
    }
}
