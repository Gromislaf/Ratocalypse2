# Katalog przedmiotów (ItemData)

**Skrypt:** `Assets/Ratocalypse/Scripts/Inventory/ItemData.cs` (ScriptableObject)  
**Assety:** `Assets/Ratocalypse/Data/Items/`  
Struktura zaimplementowana | Assety: do uzupełnienia

---

## Pola ItemData

| Pole | Typ | Opis |
|---|---|---|
| `itemId` | string | Trwały unikalny identyfikator (zob. GDD-ITM-001) |
| `itemName` | string | Wyświetlana nazwa |
| `icon` | Sprite | Ikona w UI |
| `description` | string | Opis przedmiotu |
| `itemType` | ItemType | Weapon / Shield / Helmet / Chest / Legs / Boots / Consumable |
| `weaponType` | WeaponType | Fists / OneHanded / TwoHanded / Ranged (tylko Weapon) |
| `isTwoHanded` | bool | Blokuje OffHand gdy true |
| `bonusDamage` | float | Dodatkowe obrażenia (flat) |
| `bonusArmor` | float | Procentowa redukcja obrażeń |
| `bonusCritChance` | float | Dodatkowa szansa na krit |
| `bonusMoveSpeed` | float | Dodatkowa prędkość ruchu |
| `bonusMaxHp` | float | Dodatkowe maksymalne HP |
| `bonusAttackSpeed` | float | Dodatkowa szybkość ataku |
| `healAmount` | float | HP przywracane przy użyciu (Consumable) |
| `staminaRestore` | float | Stamina przywracana przy użyciu (Consumable) |

---

---

## GDD-ITM-001 Trwały identyfikator przedmiotu (itemId)

**Status:** Zaimplementowana
**Wersja:** 0.1

**Opis funkcjonalny:** Pole `itemId` w `ItemData` (ScriptableObject) przechowuje trwały, unikalny identyfikator string. `SaveSystem` używa go zamiast `itemName` do serializacji i odtworzenia ekwipunku gracza między sesjami.

**Warunki uruchomienia:** Przy każdym wywołaniu `SaveSystem.Save()` i `SaveSystem.Load()`; walidacja obecności pola (`OnValidate`) działa w edytorze przy każdej modyfikacji assetu.

**Zasady działania:**
1. Każdy asset `ItemData` ma pole `itemId` (string) ustawiane ręcznie przez autora w Inspektorze.
2. Format: `item_<typ>_<nazwa>` (np. `item_sword_iron`, `item_helmet_leather`). Dozwolone znaki: małe litery a–z, cyfry 0–9, podkreślenie `_`.
3. `ItemData.OnValidate()` loguje `Debug.LogWarning`, gdy `itemId` jest puste lub null.
4. `SaveSystem.Save()` serializuje `itemId` (nie `itemName`) do `SaveData.bag` i `SaveData.equipped`. Item z pustym `itemId` jest pomijany z `Debug.LogWarning`.
5. `InventorySystem.RestoreFromSave()` wyszukuje ItemData w rejestrze po `itemId`. Nieznany `itemId` → item pomijany z `Debug.LogWarning`. Duplikat `itemId` w rejestrze → pierwsze dopasowanie wygrywa + `Debug.LogWarning`.

**Ograniczenia:**
- `itemId` nie może zmienić się po pierwszym zapisie gry — zmiana = utrata przedmiotu w istniejącym zapisie.
- Unikalność zapewnia autor (brak automatycznej walidacji między assetami).
- Stare zapisy przechowujące `itemName` zamiast `itemId` są nieważne i nie są migrowane.

**Parametry:**
| Parametr | Wartość | Jednostka | Zakres |
|---|---|---|---|
| Format | `item_<typ>_<nazwa>` | — | małe litery a–z, cyfry 0–9, podkreślenie |
| Długość `itemId` | maks. 64 | znaki | 1–64 |

**Wzory:** nie dotyczy

**Interakcje:**
- SAV-001, SAV-002 — `itemId` jest kluczem serializacji i deserializacji ekwipunku w `SaveData`
- `Systems/Inventory.md` — `InventorySystem.RestoreFromSave()` przyjmuje rejestr `ItemData[]` i wyszukuje po `itemId`

**Sytuacje wyjątkowe:**
- Pusty `itemId` przy zapisie → item pomijany, `Debug.LogWarning("ItemData '{itemName}' has empty itemId, skipping save")`
- Nieznany `itemId` przy wczytaniu → item pomijany, `Debug.LogWarning("Unknown itemId: {id}")`
- Duplikat `itemId` w rejestrze → pierwsze dopasowanie, `Debug.LogWarning("Duplicate itemId: {id}")`

**Informacja zwrotna:** Brak komunikatu dla gracza. Ostrzeżenia widoczne wyłącznie w konsoli Unity (dla autora/dewelopera).

**Implementacja:**
- `Assets/Ratocalypse/Scripts/Inventory/ItemData.cs` — nowe pole `public string itemId`; `OnValidate()` z ostrzeżeniem o pustym ID
- `Assets/Ratocalypse/Scripts/Inventory/InventorySystem.cs` — `GetBagIds()`, `GetEquippedIds()` zamiast metod `*Names`; `RestoreFromSave()` wyszukuje po `itemId`
- `Assets/Ratocalypse/Scripts/Core/SaveSystem.cs` — `Save()` wywołuje `GetBagIds()`/`GetEquippedIds()`; pomija itemy z pustym `itemId`

**Historia zmian:**
| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. |
| 0.1 | Zaimplementowana w zadaniu #4. Gałąź: feature/04-itemid. Testy: T1 ✅ T2 ✅ T3 ⏭ T4 ✅ T5 ⏭ (T3/T5 nieweryfikowalne — brak UI wczytywania w grze przed implementacją Load). |

---

## Przykładowe przedmioty (do uzupełnienia)

<!-- TODO: wypełnić po stworzeniu assetów ItemData w projekcie -->

| ID (nazwa pliku) | Nazwa | Typ | Kluczowe wartości |
|---|---|---|---|
| — | — | — | — |
