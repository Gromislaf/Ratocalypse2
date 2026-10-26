# Drzewko umiejętności UI

**Skrypty:** `Assets/Ratocalypse/Scripts/UI/SkillTreeUI.cs`, `SkillNodeUI.cs`  

---

## Widoczność i toggle

| Cecha | Zachowanie |
|---|---|
| Klawisz toggle | `C` |
| Wzajemne zamykanie | Otwarcie SkillTree (C) zamyka Inventory (I) |

---

## Układ

3 kolumny (Warrior / Hunter / Survivor) × 4 tiery = 12 węzłów (SkillNodeUI).

---

## SkillNodeUI — stany przycisku

| Stan | Warunek | Wygląd |
|---|---|---|
| Dostępny | `CanUnlock()` == true | Aktywny przycisk |
| Zablokowany prereq | Tier N-1 nie odblokowany | Nieaktywny |
| Brak punktów | `availableSkillPoints < 1` | Nieaktywny |
| Odblokowany | `IsUnlocked()` == true | Podświetlony/zaznaczony |

---

## Licznik punktów

Wyświetla `stats.availableSkillPoints`. Aktualizuje się przy `OnPlayerLevelUp` i `OnSkillUnlocked`.
