# System questów

**Skrypty:** `Assets/Ratocalypse/Scripts/Core/QuestManager.cs`, `QuestData.cs`, `Assets/Ratocalypse/Scripts/NPC/NPCInteraction.cs`  

---

## QuestData (ScriptableObject)

| Pole | Typ | Opis |
|---|---|---|
| `questId` | string | Unikalny identyfikator |
| `questName` | string | Wyświetlana nazwa |
| `description` | string | Treść questa |
| `targetItem` | ItemData | Przedmiot do znalezienia |
| `xpReward` | int | XP za ukończenie |
| `searchAreaRadius` | float | Promień obszaru poszukiwań (m) |

---

## Stany QuestManager

```
None → Active → ItemFound → Completed
```

| Stan | Opis |
|---|---|
| `None` | Brak aktywnego questa |
| `Active` | Quest przyjęty, gracz szuka przedmiotu |
| `ItemFound` | `targetItem` wykryty w plecaku |
| `Completed` | Quest ukończony, XP przyznane |

**Ograniczenie:** Jeden aktywny quest na raz.

---

## Przepływ questa

1. Gracz klika NPC (warstwa `NPC`) → PlayerController podchodzi (`interactRange = 2 m`) → `NPCInteraction.Interact()`
2. `QuestDialogUI.ShowQuestOffer()` — dialog z opisem i przyciskiem [Akceptuj]
3. Gracz akceptuje → `QuestManager.AcceptQuest(quest, searchAreaCenter)` → stan: **Active**
   - Publikuje `OnQuestAccepted` → MinimapController pokazuje zielony okrąg obszaru
4. Gracz podnosi `ItemPickup` w obszarze → `InventorySystem.AddToBag()` → `OnInventoryChanged`
   - QuestManager: `HasItemInBag(targetItem) == true` → stan: **ItemFound**
   - Publikuje `OnQuestItemFound`
5. Gracz wraca do NPC → `QuestDialogUI.ShowQuestComplete()` — dialog z [Odbierz nagrodę]
6. `QuestManager.CompleteQuest()` → usuwa item z plecaka → publikuje `OnQuestCompleted { questId, xpReward }`
   - XPSystem przyznaje `xpReward` XP → stan: **Completed**

---

## NPCInteraction — dialogi wg stanu

| Stan QuestManager | Dialog NPC |
|---|---|
| `None` | Oferta questa — pokazuje opis, przycisk [Akceptuj] |
| `Active` | "Wróć gdy znajdziesz [item]" |
| `ItemFound` | Odbiór nagrody — przycisk [Odbierz nagrodę] |
| `Completed` | "Dziękuję, dobra robota!" |

---

## Interakcje z innymi systemami

| System | Interakcja |
|---|---|
| InventorySystem | Nasłuchuje `OnInventoryChanged` → `HasItemInBag()` |
| MinimapController | `OnQuestAccepted` → rysuje QuestAreaCircle |
| XPSystem | `OnQuestCompleted` → `GainXP(xpReward)` |
| QuestDialogUI | Wyświetla odpowiedni panel wg stanu |
