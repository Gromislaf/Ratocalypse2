# Dialog questa UI

**Skrypt:** `Assets/Ratocalypse/Scripts/UI/QuestDialogUI.cs`  

---

## Elementy UI

| Element | Typ | Opis |
|---|---|---|
| `panel` | GameObject | Główny kontener — toggle widoczności |
| `questNameText` | TMP_Text | Nazwa questa |
| `descriptionText` | TMP_Text | Opis / treść dialogu |
| `acceptButton` | Button | Widoczny przy ofercie questa |
| `completeButton` | Button | Widoczny przy odbiorze nagrody |
| `closeButton` | Button | Zawsze widoczny gdy panel otwarty |

---

## Stany dialogu

| Metoda | Kiedy | Widoczne przyciski |
|---|---|---|
| `ShowQuestOffer()` | Stan QuestManager == None | Accept + Close |
| `ShowMessage("Wróć gdy znajdziesz...")` | Stan == Active | Close |
| `ShowQuestComplete()` | Stan == ItemFound | Complete + Close |
| `ShowMessage("Dziękuję!")` | Stan == Completed | Close |

---

## Inicjalizacja

`NPCInteraction.Awake()` wywołuje `FindFirstObjectByType<QuestDialogUI>(includeInactive: true)` — panel może być domyślnie wyłączony w scenie.
