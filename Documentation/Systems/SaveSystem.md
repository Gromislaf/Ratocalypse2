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

**Status:** Zaimplementowana
**Wersja:** 0.2

**Skrypt:** `SavePoint.cs` — implementuje `IInteractable`, warstwa `Interactable`.

**Warunki aktywacji:** Gracz klika SavePoint → PlayerController podchodzi → `SavePoint.Interact()`.

**Przebieg:**
1. `SavePoint.Interact()` publikuje `OnCheckpointReached { checkpointId }` (powiadomienie przed zapisem — do logowania i UI)
2. `SavePoint.Interact()` wywołuje `SaveSystem.Instance.Save(savePointId)`
3. `SaveSystem.Save()` zbiera dane ze wszystkich systemów (PlayerStats, InventorySystem, SkillTreeManager, QuestManager, PlayerController.transform)
4. Serializuje do JSON (Newtonsoft)
5. Zapisuje do pliku
6. `SaveSystem.Save()` publikuje `OnGameSaved { checkpointId }` (powiadomienie po pomyślnym zapisie — dla UI save feedback)

**Informacja zwrotna:**
- `OnCheckpointReached { checkpointId }` — publikowane przez `SavePoint` natychmiast po interakcji, przed zapisem; subskrybuje GameManager (log diagnostyczny)
- `OnGameSaved { checkpointId }` — publikowane przez `SaveSystem` po pomyślnym zapisie pliku; przeznaczone dla UI (⏳ planowane)

**Implementacja:**
- `Assets/Ratocalypse/Scripts/Interactable/SavePoint.cs` — `SavePoint : MonoBehaviour, IInteractable`; pole `savePointId` (SerializeField); publikuje `OnCheckpointReached`, wywołuje `SaveSystem.Instance.Save()`
- `Assets/Ratocalypse/Scripts/Core/SaveSystem.cs` — `SaveSystem`; metoda `Save(string savePointId)`; publikuje `OnGameSaved` po `File.WriteAllText()`

**Historia zmian:**
| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. |
| 0.2 | Naprawiono opis przepływu: `OnCheckpointReached` publikuje `SavePoint` przed zapisem; `OnGameSaved` publikuje `SaveSystem` po zapisie. Poprzedni opis przypisywał oba zdarzenia do jednego miejsca. |

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
