# System zapisu

**Skrypty:** `Assets/Ratocalypse/Scripts/Core/SaveSystem.cs`, `SaveData.cs`, `Assets/Ratocalypse/Scripts/Interactable/SavePoint.cs`  
**Format:** JSON (Newtonsoft.Json 3.2.2)  

---

## SaveData — co jest zapisywane

| Pole | Typ | Opis |
|---|---|---|
| `version` | string | Wersja formatu zapisu |
| `currentHp` | float | Aktualne HP gracza |
| `currentStamina` | float | Aktualna stamina |
| `currentXp` | float | Aktualne XP |
| `level` | int | Aktualny poziom |
| `availableSkillPoints` | int | Dostępne punkty umiejętności |
| `bag` | `List<string>` | ID przedmiotów w plecaku |
| `equipped` | `Dict<EquipmentSlot, string>` | ID wyposażonych przedmiotów |
| `unlockedSkills` | `List<string>` | ID odblokowanych umiejętności |
| `questState` | string | Aktualny stan QuestManager |
| `questHistory` | `List<string>` | ID ukończonych questów |
| `playerPosition` | Vector3 | Pozycja gracza w scenie |
| `savePointId` | string | ID ostatniego save pointu |

---

## Ścieżka pliku zapisu

```
Application.persistentDataPath + "/save.json"
// Windows: %userprofile%\AppData\LocalLow\FabWys\Ratocalypse 2,5\save.json
```

---

## SAV-001 Zapis przez SavePoint

**Skrypt:** `SavePoint.cs` — implementuje `IInteractable`, warstwa `Interactable`.

**Warunki aktywacji:** Gracz klika SavePoint → PlayerController podchodzi → `Interact()` → `SaveSystem.Save(savePointId)`.

**Przebieg:**
1. Zbiera dane ze wszystkich systemów (PlayerStats, InventorySystem, SkillTreeManager, QuestManager, PlayerController.transform)
2. Serializuje do JSON (Newtonsoft)
3. Zapisuje do pliku

**Informacja zwrotna:** Publikuje `OnCheckpointReached { savePointId }` → ⏳ UI save feedback (planowane).

---

## SAV-002 Wczytywanie

**Warunki aktywacji:** `SaveSystem.Load()` — wywoływane przez GameManager przy starcie gry jeśli plik istnieje.

**Przebieg:**
1. Deserializuje JSON → `SaveData`
2. Przywraca PlayerStats (HP, stamina, XP, level, skill points)
3. Przywraca InventorySystem (bag + equipped z ItemRegistry)
4. Przywraca SkillTreeManager — `RestoreFromSave(unlockedSkills)`
5. Przywraca QuestManager — stan i historię
6. Teleportuje gracza do `playerPosition`

---

## SAV-003 Usuwanie zapisu

`SaveSystem.DeleteSave()` — usuwa plik JSON. Wywołane przez GameManager przy "Nowa gra".

---

## Rejestr assetów

`SaveSystem` wymaga przypisania `itemRegistry[]` i `questRegistry[]` w Inspektorze — tablice wszystkich ItemData i QuestData w projekcie. Umożliwia wyszukiwanie assetów po ID podczas wczytywania.
