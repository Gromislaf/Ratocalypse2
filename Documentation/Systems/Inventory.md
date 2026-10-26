# System ekwipunku

**Skrypty:** `Assets/Ratocalypse/Scripts/Inventory/InventorySystem.cs`, `ItemData.cs`  

---

## INV-001 Plecak

**Opis:** Lista `bag` — 20 slotów na przedmioty.

**Parametry:**

| Parametr | Wartość |
|---|---|
| Pojemność | 20 slotów |
| Typ kolekcji | `List<ItemData>` |

**Warunki dodania:** `bag.Count < 20`. Jeśli pełny — `AddToBag()` zwraca false.

---

## INV-002 Sloty ekwipunku

**Opis:** Słownik `equipped` — 6 dedykowanych slotów na sylwetce.

| Slot | Przyjmuje |
|---|---|
| `MainHand` | Weapon (jednoręczna lub dwuręczna) |
| `OffHand` | Weapon jednoręczna lub Shield |
| `Helmet` | Helmet |
| `Chest` | Chest |
| `Legs` | Legs |
| `Boots` | Boots |

---

## INV-003 Reguły zakładania

| Sytuacja | Zachowanie |
|---|---|
| Slot zajęty | Poprzedni item wraca do plecaka |
| Broń dwuręczna w MainHand | OffHand automatycznie zwalniany — item wraca do plecaka |
| Coś w OffHand gdy MainHand ma dwuręczną | MainHand zwalniany przed założeniem |
| Plecak pełny przy zdejmowaniu | Operacja zablokowana — `Unequip()` zwraca false |

---

## INV-004 Przeliczanie bonusów

**Warunki aktywacji:** Po każdym `Equip()` lub `Unequip()`.

**Przebieg:**
```
stats.ResetBonuses()          // zeruje bonus* (NIE dotyka skillBonus*)
for each item in equipped.Values:
    stats.bonusDamage      += item.bonusDamage
    stats.bonusArmor       += item.bonusArmor
    stats.bonusCritChance  += item.bonusCritChance
    stats.bonusMoveSpeed   += item.bonusMoveSpeed
    stats.bonusMaxHp       += item.bonusMaxHp
    stats.bonusAttackSpeed += item.bonusAttackSpeed
```

---

## INV-005 Konsumable

**Opis:** Przedmioty `ItemType.Consumable` nie zajmują slotów ekwipunku — tylko plecak.

**Warunki użycia:** Kliknięcie w InventoryUI → `UseConsumable(item)`.

**Przebieg:**
1. `health.Heal(item.healAmount)` jeśli `healAmount > 0`
2. `health.RestoreStamina(item.staminaRestore)` jeśli `staminaRestore > 0`
3. `RemoveFromBag(item)`

---

## INV-006 Podnoszenie z ziemi (ItemPickup)

**Skrypt:** `Assets/Ratocalypse/Scripts/Interactable/ItemPickup.cs`

**Przebieg:**
1. Gracz klika obiekt na warstwie `Interactable`
2. PlayerController podchodzi (`interactRange = 2 m`)
3. Po dojściu wywołuje `IInteractable.Interact()`
4. `InventorySystem.AddToBag(itemData)` → `Destroy(gameObject)`

**Interakcje:** Publikuje `OnInventoryChanged` → QuestManager sprawdza `HasItemInBag(targetItem)`.
