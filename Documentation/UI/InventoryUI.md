# Ekwipunek UI

**Skrypt:** `Assets/Ratocalypse/Scripts/UI/InventoryUI.cs`, `InventorySlotUI.cs`  

---

## Widoczność i toggle

| Cecha | Zachowanie |
|---|---|
| Klawisz toggle | `I` |
| Wzajemne zamykanie | Otwarcie Inventory (I) zamyka SkillTree (C) i odwrotnie |
| Elementy | `panel` + `background` (przyciemnione tło) — oba toggle razem |

---

## Układ

| Sekcja | Opis |
|---|---|
| BagGrid (20 slotów) | Wyświetla `inventory.Bag` |
| Sylwetka (6 slotów) | MainHand, OffHand, Helmet, Chest, Legs, Boots |

---

## Logika kliknięcia w plecaku

| Typ itemu | Akcja |
|---|---|
| `Consumable` | `inventory.UseConsumable(item)` — natychmiastowe użycie |
| Pozostałe typy | `inventory.EquipFromBag(item)` — UI deleguje dobór slotu do `InventorySystem` |

**Kliknięcie w slocie ekwipunku:** `Unequip(slot)` — item wraca do plecaka.

---

## Odświeżanie

Panel odświeża się (`Refresh()`) przy zdarzeniach `OnInventoryChanged`, `OnItemEquipped`, `OnItemUnequipped` — **wyłącznie gdy panel jest otwarty**. Gdy zamknięty — zdarzenia ignorowane.

---

## InventorySlotUI

Pojedynczy slot: `Image` z ikoną + `Button`. Konfigurowany przez `Setup(item, callback)`. Sam slot nie zna logiki — tylko wyświetla ikonę i wywołuje callback.
