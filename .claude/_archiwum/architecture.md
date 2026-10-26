# Ratocalypse 2.5 — Architecture Decisions

> Nie zmieniaj tych decyzji bez dyskusji z użytkownikiem.
> Sekcje oznaczone ⚠️ były szczególnie trudne — wymagały wielu iteracji.

---

## Fundamenty

- **PlayerStats** — czysty DTO (ScriptableObject). Zero `EventBus.Publish` w środku. Zdarzenia wydaje `PlayerHealthComponent`. Bonusy z ekwipunku (`bonus*`) i skilli (`skillBonus*`) są rozdzielone — `ResetBonuses()` nie dotyka `skillBonus*`.
- **EventBus.Clear()** — **nigdy** nie wywoływać przy zmianie sceny. `DontDestroyOnLoad` obiekty zarządzają subskrypcjami przez `OnEnable`/`OnDisable`.
- **NavMesh** — ruch gracza i wrogów wyłącznie przez `NavMeshAgent`. Gra to izometryczne click-to-move (Diablo-like), nie physics-based movement.
- **Animation Events** — okna obrażeń w walce przez Animation Events (`OnAttackHit()`), nie przez `Trigger`/`Collider` na czas trwania ataku.
- **FMODEvents** — dostępny przez `GameManager.FMODEvents` (przypisany w Inspektorze). Nie używaj `Resources.Load`.

---

## PlayerHealthComponent
`Assets/Ratocalypse/Scripts/Player/PlayerHealthComponent.cs`

Implementuje `IDamageable` i `IHealable` — wrogowie trafiają gracza przez interfejs, nie twardą referencję. Obsługuje iframes (`IsInvincible` property ustawiany przez `PlayerController` podczas dodge'a). HP regen w `Update()` (`stats.TotalHpRegen * deltaTime`), publikuje `OnPlayerHealed`.

---

## PlayerController
`Assets/Ratocalypse/Scripts/Player/PlayerController.cs`

`RequireComponent(NavMeshAgent)`. Click-to-move: LPM → raycast (priorytet: Interactable > Enemy > Ground). Unik: Shift+PPM → `DodgeCoroutine` (odłącza `agent.updatePosition`, przesuwa Transform bezpośrednio, Warp na NavMesh po zakończeniu). Obrót przez `Quaternion.Slerp`, nie przez `agent.updateRotation`. Subskrybuje `OnPlayerLevelUp`/`OnItemEquipped`/`OnItemUnequipped` → odświeża `agent.speed`.

---

## PlayerCombat
`Assets/Ratocalypse/Scripts/Player/PlayerCombat.cs`

MonoBehaviour na tym samym GO co PlayerController (`RequireComponent`). Czyta `CurrentTarget` i `IsInAttackRange` z PlayerController. Animation Events: `OnAttackHit()` (klatka uderzenia) + `OnAttackEnd()` (reset cooldown). Fallback timer (`1f / TotalAttackSpeed`) działa bez animatora. `animator.ResetTrigger(HashAttack)` w `OnAttackEnd` i fallback — zapobiega stale trigger przy Any State. EnemyAI sam publikuje `OnEnemyDamaged` — PlayerCombat publikuje tylko `OnCriticalHit`.

---

## PlayerAnimator
`Assets/Cartoon Heroes/Male/3D/Animation Skeleton/PlayerAnimator.controller` — DZIAŁA ✅

Parametry: `Speed` (float), `AttackSpeed` (float), `Attack` (trigger), `Dodge` (trigger), `Hit` (trigger), `Die` (trigger).  
Stany: `Idle/Run` (BlendTree Speed 0→1), `Attack`.  
Przejścia: `Any State → Attack` (Has Exit Time OFF, Can Transition To Self OFF), `Attack → Idle/Run` (Has Exit Time ON, Exit Time 0.95, Duration 0.1, brak Conditions).  
Enemy Animator Controller: plik istnieje (`Assets/Ratocalypse/Animations/EnemyAnimator.controller`), aktualnie pusty — EnemyAI null-safe przez `?.`.

---

## EnemyAI + EnemyData
`Assets/Ratocalypse/Scripts/Enemy/`

`EnemyData` (ScriptableObject, jeden asset = jeden typ wroga) + `EnemyAI` (MonoBehaviour + `IDamageable`).  
FSM: `Patrol` → `Alert` (stoi `alertDelay` sek.) → `Chase` → `Attack` → `Stunned` → `Dead`.  
Patrol: losowy punkt w promieniu `patrolRadius` (8 prób NavMesh.SamplePosition) lub waypoints.  
Ataki przez Animation Event `OnAttackHit()`. Gracz nakłada stun przez `ApplyStun(duration)`.  
Publikuje `OnEnemyDamaged` i `OnEnemyDied` (z `xpReward` dla XPSystem).  
`FindFirstObjectByType<PlayerController>()` w `Awake()` — do rozważenia cache przez GameManager.

---

## InventorySystem + ItemData
`Assets/Ratocalypse/Scripts/Inventory/`

`InventorySystem` (MonoBehaviour na graczu) trzyma `List<ItemData> bag` (20 slotów) i `Dictionary<EquipmentSlot, ItemData> equipped`.  
`RecalculateBonuses()` woła `PlayerStats.ResetBonuses()` i sumuje bonusy ze wszystkich equipped slotów — **jedyne miejsce gdzie bonusy ekwipunku są aplikowane**.  
`ItemData` to ScriptableObject (jeden asset = jeden typ itemu). Bonusy mapują się 1:1 na pola `bonus*` w `PlayerStats` — nie dodawaj bonusów bez aktualizacji obu.  
`EquipmentSlot` enum: `MainHand, OffHand, Helmet, Chest, Legs, Boots`. Broń `isTwoHanded=true` w MainHand automatycznie zwalnia OffHand (item wraca do baga).

---

## InventoryUI
`Assets/Ratocalypse/Scripts/UI/InventoryUI.cs`

Panel, toggle klawisz `I`. 20 bag slotów + 6 equip slotów na sylwetce.  
Klik bag → auto-equip do właściwego slotu lub `UseConsumable`. Klik equip → `Unequip`.  
Subskrybuje `OnInventoryChanged`/`OnItemEquipped`/`OnItemUnequipped` przez EventBus.  
`background` i `panel` są osobnymi GO (rodzeństwo) — oba toggle razem w `Toggle()` i `Start()`.  
Wzajemnie zamyka SkillTree przez EventBus: `OnInventoryToggled` / `OnSkillTreeToggled`.

---

## PlayerHUD
`Assets/Ratocalypse/Scripts/UI/PlayerHUD.cs`

HP fill (Image.fillAmount pionowy, `HealthFull` nad `HealthEmpty`) + Stamina fill (poziomy).  
Subskrybuje `OnPlayerDamaged`, `OnPlayerHealed`, `OnPlayerStaminaChanged`. Init z PlayerStats w `Start()`.

---

## XP / LevelSystem
`Assets/Ratocalypse/Scripts/Player/XPSystem.cs`

Słucha `OnEnemyDied` i `OnQuestCompleted` → wywołuje `PlayerHealthComponent.GainXP()`.  
`XPBarUI` subskrybuje `OnPlayerXPGained` i `OnPlayerLevelUp` (notyfikacja "LEVEL UP!").

---

## SkillTree
`Assets/Ratocalypse/Scripts/`

Dane zdefiniowane w kodzie, brak assetów.
- `Core/SkillDefinition.cs` — `SkillEffectType` enum + `SkillDefinition` (readonly struct) + `SkillDatabase` (static, 12 skilli, 3 gałęzie × 4 tiery). Prerequisite: tier N wymaga tier N-1 tej samej gałęzi.
- `Player/SkillTreeManager.cs` — MonoBehaviour na graczu. `HashSet<string> _unlocked`. `TryUnlock(id)` sprawdza punkty + prerequisite, aplikuje bonus do `stats.skillBonus*`, publikuje `OnSkillUnlocked`. `RestoreFromSave(IEnumerable<string>)` gotowe dla SaveSystem.
- `UI/SkillNodeUI.cs` — prefab node'a: Image + Button + 2× TMP. `Initialize()` + `Refresh()`.
- `UI/SkillTreeUI.cs` — panel, toggle `C`. Buduje 12 node'ów w `Start()`, po 4 na kolumnę (warriorColumn / hunterColumn / survivorColumn, VerticalLayoutGroup).
- `SkillTreeUI` musi siedzieć na **rodzicu** panelu (nie na samym panelu) — inaczej `SetActive(false)` wyłącza `Update()`.
- Skille jednorazowe, 1 punkt/level, max 20 punktów (poziom 20), 12 skilli do odblokowania.

---

## QuestSystem
`Assets/Ratocalypse/Scripts/Core/` + `Scripts/NPC/` + `Scripts/UI/`

- `QuestData` (ScriptableObject): `questId`, `questName`, `description`, `targetItem`, `xpReward`, `searchAreaRadius`.
- `QuestManager` (singleton): stany `None → Active → ItemFound → Completed`. `AcceptQuest()` / `CompleteQuest()`. Słucha `OnInventoryChanged` — wykrywa zebranie `targetItem` i przechodzi do `ItemFound`.
- `NPCInteraction` (IInteractable na NPC z BoxCollider): przełącza dialog wg stanu questa (`ShowQuestOffer` / `ShowMessage` / `ShowQuestComplete`).
- `QuestDialogUI`: TMP panel z przyciskami Accept / Complete / Close. `ShowMessage()` dla powiadomień bez akcji.
- Publikuje: `OnQuestAccepted`, `OnQuestItemFound`, `OnQuestCompleted` (XP odbiera XPSystem).

---

## OcclusionFader ⚠️
`Assets/Ratocalypse/Scripts/Player/OcclusionFader.cs` — na Main Camera

**Nie modyfikuj logiki bez dyskusji — wymagało 7+ iteracji żeby działało.**  
`OverlapSphere` + filtr Y + filtr kierunku (dot product XZ). Wyłącza renderer **i** collider razem.  
Main Camera Culling Mask **musi** zawierać warstwę "Buildings".  
Planowany upgrade: instant hide (`renderer.enabled`) → płynny fade przez `MaterialPropertyBlock` + `_Alpha` w Shader Graph.

---

## SaveSystem ✅
`Assets/Ratocalypse/Scripts/Core/`

- `SaveData` — czysty DTO (nie MonoBehaviour). Pola: `version`, HP/Stamina/XP/Level/SkillPoints, `bag` (List\<string\>), `equipped` (Dictionary\<string,string\>), `unlockedSkills`, `activeQuestId`, `questState` (int), `searchAreaX/Y/Z`, `completedQuestIds`, `posX/Y/Z`, `lastSavePointId`.
- `SaveSystem` (Singleton MonoBehaviour) — `Save(savePointId)`, `Load()`, `DeleteSave()`. Format: **Newtonsoft JSON 3.2.2** (`Formatting.Indented`). Ścieżka: `Application.persistentDataPath/save.json`. SerializeField: `stats`, `inventory`, `skillTree`, `playerTransform`, `itemRegistry[]`, `questRegistry[]`.
- `SavePoint` (IInteractable) — `Assets/Ratocalypse/Scripts/Interactable/`. Collider isTrigger na layerze "Interactable". `Interact()` → `SaveSystem.Instance.Save(savePointId)`. Publikuje `OnGameSaved { checkpointId }`.

**Kolejność przywracania w Load():**
1. `PlayerStats` runtime fields (level/xp/hp/stamina/skillPoints)
2. `SkillTreeManager.RestoreFromSave()` — przelicza `skillBonus*`
3. `InventorySystem.RestoreFromSave()` — przelicza bonusy ekwipunku
4. `QuestManager.RestoreFromSave()` — odtwarza stan questa, re-publikuje `OnQuestAccepted` jeśli Active/ItemFound
5. `NavMeshAgent.Warp(savedPos)` — teleportuje gracza

**Ważne:** `itemRegistry` i `questRegistry` to tablice ScriptableObject przypisane ręcznie w Inspektorze — SaveSystem szuka po `itemName`/`questId`. Brak SO w registry → item/quest pomijany przy wczytaniu.

**QuestManager:** dodano `HashSet<string> _completedQuests`, `CompletedQuests` property, `RestoreFromSave()`. `CompleteQuest()` zapisuje questId do historii przed wyzerowaniem `ActiveQuest`.
