# Plan: #4 — itemId w ItemData i zmiana systemu zapisu ekwipunku

**Specyfikacja:** GDD-ITM-001 (`Documentation/Assets/Items.md`)
**Rozmiar:** M · **Zasady:** Z2.2–3 · **Gałąź:** `feature/04-itemid`

---

## Zmiany w kodzie

| Plik | Zmiana |
|---|---|
| `Assets/Ratocalypse/Scripts/Inventory/ItemData.cs` | Nowe pole `[SerializeField] public string itemId`; `OnValidate()` z `Debug.LogWarning` gdy puste |
| `Assets/Ratocalypse/Scripts/Inventory/InventorySystem.cs` | Rename `GetBagNames` → `GetBagIds`, `GetEquippedNames` → `GetEquippedIds`; lookup po `itemId`; warningi na puste/nieznane/duplikat |
| `Assets/Ratocalypse/Scripts/Core/SaveSystem.cs` | 2 linie: `GetBagIds()`/`GetEquippedIds()` zamiast `*Names` |
| `Assets/Ratocalypse/Scripts/Core/SaveData.cs` | Tylko komentarz: "nazwy itemów" → "ID itemów" |

## Kolejność kroków

1. `ItemData.cs` — dodać pole `itemId` i `OnValidate()`
2. `InventorySystem.cs` — rename metod + zmiana lookup + warningi
3. `SaveSystem.cs` — zmienić 2 wywołania
4. `SaveData.cs` — zaktualizować komentarz

## Decyzje techniczne

- Pola `bag`/`equipped` w `SaveData` nie zmieniane nazwowo — typy pasują, tylko semantyka; unika problemów z deserializacją
- `[SerializeField]` na `itemId` — spójność z wzorcem reszty SO
- `Debug.LogWarning` (nie Error) — nie blokuje gry
- Duplikat: `System.Array.FindAll` → jeśli `Length > 1` → warning, użyty `result[0]`
- Etap 3b (Integrator): odpada — brak assetów ItemData do uzupełnienia

## Przypadki testowe

| # | Warunek | Akcja | Oczekiwany wynik | Poziom |
|---|---|---|---|---|
| T1 | Asset ItemData bez `itemId` | Otwórz SO w Inspektorze | Warning: "ItemData 'X' has empty itemId." | 2 |
| T2 | Normalny zapis z itemId | Ustaw itemId na teście, wejdź do SavePoint | JSON zawiera `itemId` zamiast `itemName` | 2 |
| T3 | Wczytanie zapisu | Uruchom grę, Load() | Ekwipunek odtworzony | 1 |
| T4 | Item z pustym itemId w torbie, zapis | Zapisz grę | Warning "skipping save"; item pominięty w JSON | 2 |
| T5 | Stary zapis (itemName zamiast ID) | Podmień save.json, wczytaj | Warning "Unknown itemId"; gra nie crashuje | 2 |

---

## Dziennik bramek

| Data | Etap | Zdarzenie / decyzja |
|---|---|---|
| 2026-11-01 | 0 | Rozmiar M zatwierdzony |
| 2026-11-01 | 1+2 | Spec GDD-ITM-001 i plan zatwierdzony; etap 3b odpada (brak assetów ItemData) |
| 2026-11-01 | 4 | Inspektor: 🔴 [SerializeField] public (naprawione) + drobne kropki w komunikatach (naprawione) |
| 2026-11-01 | 4 | Testy: T1 ✅ T2 ✅ T3 ⏭ T4 ✅ T5 ⏭ (T3/T5 wymagają Load() — zadanie #31) |
| 2026-11-01 | 6+7 | Commit `3741ef4` → merge do main |
