# Plan: #10 — Przeniesienie reguły typ→slot z `InventoryUI` do `InventorySystem`

Ścieżka: refaktor · Rozmiar: S · Gałąź: `feature/10-typ-slot-rule`

## Pliki

- `Assets/Ratocalypse/Scripts/Inventory/InventorySystem.cs`
- `Assets/Ratocalypse/Scripts/UI/InventoryUI.cs`

## Zmiany

### `InventorySystem.cs`

Dodać metodę publiczną `EquipFromBag(ItemData item)` (przed `GetBagIds`, ~linia 169):

```csharp
public bool EquipFromBag(ItemData item)
{
    EquipmentSlot? slot = GetDefaultSlot(item);
    if (!slot.HasValue) return false;
    return Equip(item, slot.Value);
}
```

Przenieść `GetDefaultSlot` z `InventoryUI` jako `private static`:

```csharp
private static EquipmentSlot? GetDefaultSlot(ItemData item)
{
    return item.itemType switch
    {
        ItemType.Weapon  => EquipmentSlot.MainHand,
        ItemType.Shield  => EquipmentSlot.OffHand,
        ItemType.Helmet  => EquipmentSlot.Helmet,
        ItemType.Chest   => EquipmentSlot.Chest,
        ItemType.Legs    => EquipmentSlot.Legs,
        ItemType.Boots   => EquipmentSlot.Boots,
        _                => null
    };
}
```

### `InventoryUI.cs`

Zmienić `OnBagSlotClicked` (linia 106–120) — usunąć logikę typ→slot, wywołać `EquipFromBag`:

```csharp
private void OnBagSlotClicked(int index)
{
    if (index >= inventory.Bag.Count) return;
    ItemData item = inventory.Bag[index];

    if (item.itemType == ItemType.Consumable)
    {
        inventory.UseConsumable(item);
        return;
    }

    inventory.EquipFromBag(item);
}
```

Usunąć metodę `GetDefaultSlot` (linia 122–134) z `InventoryUI`.

## Przypadki testowe

Poziom 1 — widoczne w grze.

| # | Warunek | Akcja | Oczekiwany wynik |
|---|---|---|---|
| T1 | Miecz (Weapon) w plecaku, MainHand pusty | Kliknij miecz | Miecz w MainHand, znika z plecaka |
| T2 | Hełm (Helmet) w plecaku, slot pusty | Kliknij hełm | Hełm w slocie Helmet |
| T3 | MainHand zajęty, klik nowa broń | Kliknij nową broń | Stara wraca do plecaka, nowa w MainHand |
| T4 | Mikstura (Consumable) w plecaku | Kliknij miksturę | HP rośnie, brak ekwipowania |
| T5 | Item w slocie ekwipunku | Kliknij slot | Item wraca do plecaka |

## Decyzje techniczne

- `EquipFromBag` zamiast `EquipDefault` — precyzyjniej oddaje źródło itemu.
- `GetDefaultSlot` jako `private static` — nie potrzebna poza klasą.
- Sprawdzenie `Consumable` zostaje w UI — to rozgałęzienie intencji, nie reguła gry.

## Wpływ na dokumentację

- `Documentation/UI/InventoryUI.md` §Logika kliknięcia: zmienić opis akcji → `EquipFromBag(item)`.
- `Documentation/Systems/Inventory.md` §INV-003: dodać opis `EquipFromBag`.

## Dziennik bramek

| Data | Etap | Zdarzenie |
|---|---|---|
| 2026-11-03 | 0+2 | Rozmiar S zatwierdzony; plan zatwierdzony przez autora |
