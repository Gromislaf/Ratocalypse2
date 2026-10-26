# EventBus — system komunikacji

**Skrypt:** `Assets/Ratocalypse/Scripts/Core/EventBus.cs`  
**Zdarzenia:** `Assets/Ratocalypse/Scripts/Core/GameEvents.cs`  

---

## Zasada działania

Żaden system nie trzyma bezpośredniej referencji do innego. Komunikacja wyłącznie przez zdarzenia:

```
Publish:   EventBus.Publish(new OnPlayerDamaged { amount = 20f })
Subscribe: EventBus.Subscribe<OnPlayerDamaged>(OnDamaged)   // w OnEnable
           EventBus.Unsubscribe<OnPlayerDamaged>(OnDamaged) // w OnDisable
```

Zdarzenia są `struct` — zero alokacji na stercie (GC-friendly).

**Ważne:** Każdy subskrybent musi wywołać `Unsubscribe` w `OnDisable`. Brak tego → callback na zniszczonym obiekcie → błąd.

---

## Tabela zdarzeń

| Zdarzenie | Dane | Kto publikuje | Kto nasłuchuje |
|---|---|---|---|
| `OnPlayerDamaged` | amount, currentHp, maxHp | PlayerHealthComponent | PlayerHUD |
| `OnPlayerHealed` | amount, currentHp, maxHp | PlayerHealthComponent | PlayerHUD |
| `OnPlayerStaminaChanged` | current, max | PlayerHealthComponent | PlayerHUD |
| `OnPlayerDied` | — | PlayerHealthComponent | GameManager |
| `OnPlayerXPGained` | amount, totalXP | PlayerHealthComponent | XPBarUI |
| `OnPlayerLevelUp` | newLevel, remainingPoints | PlayerHealthComponent | XPBarUI, PlayerController, PlayerCombat |
| `OnEnemyDamaged` | amount, isCritical, worldPosition | EnemyAI | ⏳ (floating damage text) |
| `OnEnemyDied` | xpReward | EnemyAI | XPSystem |
| `OnCriticalHit` | — | PlayerCombat | ⏳ (efekt wizualny) |
| `OnItemPickedUp` | item | InventorySystem | — |
| `OnItemEquipped` | item, slot | InventorySystem | PlayerController, PlayerCombat, InventoryUI |
| `OnItemUnequipped` | item, slot | InventorySystem | PlayerController, PlayerCombat, InventoryUI |
| `OnInventoryChanged` | — | InventorySystem | InventoryUI, QuestManager |
| `OnInventoryToggled` | isOpen | InventoryUI | — |
| `OnSkillUnlocked` | skillId, branch, remainingPoints | SkillTreeManager | SkillTreeUI |
| `OnQuestAccepted` | quest, searchAreaCenter, searchAreaRadius | QuestManager | MinimapController |
| `OnQuestItemFound` | quest | QuestManager | NPCInteraction |
| `OnQuestCompleted` | questId, xpReward | QuestManager | XPSystem |
| `OnGamePaused` | isPaused | GameManager | — |
| `OnCheckpointReached` | savePointId | SavePoint | GameManager |
