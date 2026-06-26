# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Ratocalypse 2.5 — Project Context

## Project Overview
- **Genre:** RPG
- **Engine:** Unity 6000.4.3f1 (Unity 6)
- **Migration:** Ported from Unity 2022.3.4f1
- **Language:** C#
- **Render Pipeline:** URP (Universal Render Pipeline 17.4.0) — separate PC and Mobile renderer assets in `Assets/Settings/`

## Current Migration Status
- ✅ Scenes — przeniesione
- ✅ Assets (grafika / audio) — przeniesione
- ⏳ Scripts C# — do przeniesienia / przepisania
- ⏳ Prefabs — do przeniesienia

## Build Order (wg GDD — aktualizuj ✅ gdy gotowe)
1. ✅ `EventBus` — komunikacja między systemami
2. ✅ `PlayerStats` (ScriptableObject) + `PlayerHealthComponent`
3. ✅ `PlayerController` + `CameraController` — ruch click-to-move NavMesh + kamera izometryczna
4. ✅ `PlayerCombat` — atak + hitbox przez Animation Events (EnemyAI gotowe, można budować)
5. ✅ `EnemyAI` — FSM (Patrol/Alert/Chase/Attack/Stunned/Dead) + `EnemyData` (ScriptableObject)
6. ✅ `InventorySystem` backend — `ItemData` (ScriptableObject) + `InventorySystem` (MonoBehaviour)
   ✅ `InventoryUI` — plecak (20 slotów) + ekwipunek (6 slotów na sylwetce). Klawisz I toggle.
7. ✅ `PlayerHUD` — pasek HP (HealthFull/Empty, fill pionowy) + Stamina (fill poziomy). Subskrybuje EventBus.
8. ✅ `XP/LevelSystem` — `XPSystem` (słucha OnEnemyDied/OnQuestCompleted → GainXP) + `XPBarUI` (pasek XP, poziom, notyfikacja "LEVEL UP!")
9. ✅ `SkillTree` — kod + UI gotowe. Testowanie odłożone do momentu gdy w grze pojawią się wrogowie/XP
10. ⏳ `QuestSystem`
11. ⏳ `SaveSystem` — format do potwierdzenia z użytkownikiem przed implementacją

## Core Systems
- **Combat** — walka gracza i wrogów
- **Inventory** — ekwipunek, przedmioty
- **Save/Load** — zapis i wczytywanie stanu gry (format JSON/BinaryFormatter/własny — potwierdź z użytkownikiem przed implementacją)

## Dependencies / Packages
- **Cinemachine 3.1.6** (Unity Package Manager) — Unity 6 version, changed API vs 2.x
- **Input System 1.19.0** (Unity Package Manager) — New Input System
- **TextMeshPro** — bundled via `com.unity.ugui 2.0.0`
- **FMOD** (`Assets/Plugins/FMOD/`) — audio middleware; use `FMODUnity.RuntimeManager` and `StudioEventEmitter`, NOT Unity's `AudioSource`/`AudioClip`
- **AI Navigation 2.0.12** — `NavMeshAgent` for enemy pathfinding
- **Timeline 1.8.12** — cutscenes/sequences
- **Visual Scripting 1.9.11** — may be used in some scenes

## Unity 6 Migration Notes
Przy przenoszeniu skryptów z 2022.3 zwróć uwagę na:
- `FindObjectOfType<T>()` → `FindFirstObjectByType<T>()` lub `FindAnyObjectByType<T>()`
- `Physics2D.OverlapCircleAll` i podobne — sprawdź deprecation warnings
- Cinemachine 3.x: `CinemachineVirtualCamera` → `CinemachineCamera`; `CinemachineBrain` na Main Camera pozostaje
- Input System — używaj `InputActionAsset` / `PlayerInput` (nowy system), nie `Input.GetKey()`
- URP: shadery i materiały muszą być zgodne z URP — nie używaj Built-in shaderów
- FMOD: nie zastępuj FMOD zwykłym `AudioSource` — cały audio idzie przez FMOD

## Conventions
- `PascalCase` dla klas i publicznych metod, `camelCase` dla prywatnych pól
- Prywatne pola serializowane: `[SerializeField] private`
- Unikaj `public` fields — używaj properties lub `[SerializeField]`
- Jeden skrypt = jedna klasa, nazwa pliku = nazwa klasy
- Namespace: `Ratocalypse` (opcjonalnie, utrzymuj spójność)

## Architecture Decisions (nie zmieniaj bez dyskusji)
- **PlayerStats** to czysty DTO (ScriptableObject) — zero `EventBus.Publish` w środku. Zdarzenia wydaje `PlayerHealthComponent`.
- **PlayerHealthComponent** implementuje `IDamageable` i `IHealable` — wrogowie trafiają gracza przez interfejs, nie twardą referencję.
- **FMODEvents** dostępny przez `GameManager.FMODEvents` (przypisany w Inspektorze) — nie używaj `Resources.Load`.
- **EventBus.Clear()** nigdy nie wywoływać przy zmianie sceny — `DontDestroyOnLoad` obiekty same zarządzają subskrypcjami przez `OnEnable`/`OnDisable`.
- **NavMesh** dla ruchu gracza i wrogów — gra to izometryczne click-to-move (Diablo-like), nie physics-based movement.
- **Animation Events** do okien obrażeń w walce — nie `Trigger`/`Collider` na czas trwania ataku.
- **InventorySystem** (MonoBehaviour na graczu) trzyma `List<ItemData> bag` (20 slotów) i `Dictionary<EquipmentSlot, ItemData> equipped`. `RecalculateBonuses()` woła `PlayerStats.ResetBonuses()` i sumuje bonusy ze wszystkich slotów — jedyne miejsce gdzie bonusy są aplikowane.
- **ItemData** to ScriptableObject (jeden asset = jeden typ itemu). Bonusy (`bonusDamage`, `bonusArmor` itd.) mapują się 1:1 na pola `bonus*` w `PlayerStats` — nie dodawaj bonusów których nie ma w PlayerStats bez aktualizacji obu.
- **EquipmentSlot** enum: `MainHand, OffHand, Helmet, Chest, Legs, Boots`. Broń z `isTwoHanded=true` w MainHand automatycznie zwalnia OffHand (item wraca do plecaka).
- **OcclusionFader** (na Main Camera) — `OverlapSphere` + filtr Y + filtr kierunku (dot product XZ). Wyłącza renderer I collider razem. Nie modyfikuj logiki bez dyskusji — wymagało 7+ iteracji żeby działało poprawnie. Main Camera Culling Mask **musi** zawierać warstwę "Buildings".
- **EnemyAI** (`Assets/Ratocalypse/Scripts/Enemy/`) — `EnemyData` (ScriptableObject, jeden asset = jeden typ wroga) + `EnemyAI` (MonoBehaviour + IDamageable). FSM: Patrol (losowy punkt w promieniu `patrolRadius` lub waypoints) → Alert (stoi `alertDelay` sek.) → Chase → Attack → Stunned → Dead. Ataki przez Animation Event `OnAttackHit()`. Gracz nakłada stun przez `ApplyStun(duration)`. Publikuje `OnEnemyDamaged` i `OnEnemyDied`. XP z `xpReward` zostanie odebrane przez XP system (krok 7).
- **InventoryUI** (`Assets/Ratocalypse/Scripts/UI/`) — `InventorySlotUI` (slot z Image+Button+callback) + `InventoryUI` (panel, toggle I, 20 bag slotów + 6 equip slotów). Klik bag → auto-equip do właściwego slotu lub UseConsumable. Klik equip → Unequip. Subskrybuje OnInventoryChanged/OnItemEquipped/OnItemUnequipped przez EventBus. `background` i `panel` są osobnymi GO (rodzeństwo), oba toggle razem w `Toggle()` i `Start()`.
- **PlayerHUD** (`Assets/Ratocalypse/Scripts/UI/`) — HP fill (Image.fillAmount pionowy, HealthFull nad HealthEmpty) + Stamina fill (poziomy). Subskrybuje OnPlayerDamaged, OnPlayerHealed, OnPlayerStaminaChanged. Init z PlayerStats w Start().
- **PlayerCombat** (`Assets/Ratocalypse/Scripts/Player/`) — MonoBehaviour na tym samym GO co PlayerController (RequireComponent). Czyta `CurrentTarget` i `IsInAttackRange` z PlayerController. Animation Events: `OnAttackHit()` (klatka uderzenia) + `OnAttackEnd()` (reset cooldown). Animator: trigger `"Attack"`, float `"AttackSpeed"` = `stats.TotalAttackSpeed`. EnemyAI sam publikuje `OnEnemyDamaged` — PlayerCombat publikuje tylko `OnCriticalHit`. Fallback timer (`1f / TotalAttackSpeed`) działa bez animatora/eventów. `animator.ResetTrigger(HashAttack)` wywoływane w OnAttackEnd i TickFallback — zapobiega stale trigger przy Any State.
- **PlayerAnimator** (`Assets/Cartoon Heroes/Male/3D/Animation Skeleton/PlayerAnimator.controller`) — DZIAŁA ✅. Parametry: `Speed` (float), `AttackSpeed` (float), `Attack` (trigger), `Dodge` (trigger), `Hit` (trigger), `Die` (trigger). Stany: `Idle/Run` (BlendTree Speed 0→1: Idle/Walk), `Attack`. Przejścia: `Any State → Attack` (trigger Attack, Has Exit Time OFF, Can Transition To Self OFF), `Attack → Idle/Run` (Has Exit Time ON, Exit Time 0.95, Duration 0.1, brak Conditions). Enemy Animator Controller — na razie None (brak animacji wroga, EnemyAI null-safe przez `?.`).
- **SkillTree** (`Assets/Ratocalypse/Scripts/`) — brak assetów, dane zdefiniowane w kodzie.
  - `Core/SkillDefinition.cs` — `SkillEffectType` enum + `SkillDefinition` (readonly struct) + `SkillDatabase` (static, 12 skilli, 3 gałęzie × 4 tiery). Prerequisite: tier N wymaga tier N-1 tej samej gałęzi.
  - `Player/SkillTreeManager.cs` — MonoBehaviour na graczu (obok PlayerController). `HashSet<string> _unlocked`. `TryUnlock(id)` sprawdza punkty + prerequisite, aplikuje bonus do `stats.skillBonus*`, publikuje `OnSkillUnlocked`. `RestoreFromSave(IEnumerable<string>)` do użycia przez SaveSystem.
  - `UI/SkillNodeUI.cs` — prefab node'a: Image tło (złoty/jasny/ciemny wg stanu) + Button + 2× TMP. `Initialize()` + `Refresh()`.
  - `UI/SkillTreeUI.cs` — panel, toggle `C`. Buduje 12 node'ów z prefaba w `Start()`, po 4 na kolumnę (warriorColumn / hunterColumn / survivorColumn, VerticalLayoutGroup). Wyświetla licznik punktów.
  - **PlayerStats** — dodano `skillBonus*` (8 pól NonSerialized), `ResetSkillBonuses()`, `TotalHpRegen`. Bonusy z ekwipunku (`bonus*`) i skilli (`skillBonus*`) są rozdzielone — `ResetBonuses()` nie dotyka skillBonus*.
  - **PlayerHealthComponent** — dodano HP regen loop w Update() (`stats.TotalHpRegen * deltaTime`), publikuje `OnPlayerHealed`.
  - Skille jednorazowe (unlock raz), 1 punkt/level, max 20 punktów na poziomie 20, 12 skilli do odblokowania.
  - ✅ Setup UI w Edytorze zrobiony. SkillTreeUI musi siedzieć na rodzicu panelu (nie na samym panelu) — inaczej SetActive(false) wyłącza Update().
  - Panel I i C wzajemnie się zamykają przez EventBus: OnInventoryToggled / OnSkillTreeToggled.

## Znane TODO / Planowane ulepszenia
- **OcclusionFader fade** — aktualnie instant hide (`renderer.enabled`). Planowany upgrade do płynnego fade'u: otworzyć `Shader Graphs/MIS_Base 1` w Shader Graph Editor → Surface Type: Transparent → dodać właściwość `_Alpha` → podłączyć do Alpha output → w OcclusionFader użyć `MaterialPropertyBlock` zamiast `renderer.enabled`.

## Key Rules for Claude
- Przy pisaniu nowych skryptów używaj Unity 6 API (nie przestarzałych metod z 2022)
- Jeśli naprawiasz skrypt z 2022 — zaznacz co zmieniłeś i dlaczego
- Nie modyfikuj plików w `Assets/Art/`, `Assets/Audio/`, `Assets/Plugins/FMOD/` — to już przeniesione assety i zewnętrzny plugin
- Przy zmianach w systemie walki lub inventory — najpierw opisz plan, potem implementuj
- Przed implementacją Save/Load — potwierdź format zapisu z użytkownikiem
