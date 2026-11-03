# Przegląd architektury — decyzje projektowe

---

## GDD-ARC-002 Przegląd architektury — decyzje projektowe

**Status:** Zatwierdzona
**Wersja:** 0.1

**Cel:** Zebranie decyzji architektonicznych obowiązujących w całym projekcie — jeden punkt referencyjny dla zasad, które wpływają na wiele systemów jednocześnie.

**Odpowiedzialność:**
- Za: dokumentowanie zasad architektury aktywnych w całym projekcie; katalog sytuacji wyjątkowych z powodami ich istnienia.
- NIE za: specyfikację konkretnych mechanik gry (patrz `Documentation/Player/`, `Documentation/Systems/`); parametry rozgrywki; zasady techniczne dla agentów (`.claude/zasady/zasady-techniczne.md`).

**Elementy:**
| Klasa / plik | Rola |
|---|---|
| `PlayerStats` | Czysty DTO (ScriptableObject) — zero logiki zdarzeń wewnątrz |
| `PlayerHealthComponent` | Jedyny wydawca zdarzeń zdrowia gracza |
| `PlayerController` | Click-to-move; `RequireComponent(NavMeshAgent)` |
| `PlayerCombat` | MonoBehaviour na tym samym GO co PlayerController |
| `EnemyAI` + `EnemyData` | FSM + ScriptableObject; jeden asset = jeden typ wroga |
| `InventorySystem` | Jedyne miejsce przeliczania bonusów ekwipunku |
| `InventoryUI` | Rodzeństwo `background`/`panel` toggle razem |
| `SkillTreeUI` | Musi siedzieć na rodzicu panelu (nie na samym panelu) |
| `SaveSystem` | Singleton MonoBehaviour; Newtonsoft JSON 3.2.2 |
| `OcclusionFader` | Na Main Camera; logika po 7+ iteracjach — zmiana przez dyskusję |
| `XPSystem` | Słucha zdarzeń; wywołuje `PlayerHealthComponent.GainXP()` |
| `QuestManager` | Singleton; stany `None → Active → ItemFound → Completed` |

**Kontrakt (API):** nie dotyczy — dokument zbiorczy; kontrakty poszczególnych klas w plikach obszarowych.

**Zasady użycia:**
1. `EventBus.Clear()` — nigdy nie wywoływać przy zmianie sceny. `DontDestroyOnLoad` obiekty zarządzają subskrypcjami przez `OnEnable`/`OnDisable`.
2. Ruch gracza i wrogów wyłącznie przez `NavMeshAgent` — gra jest izometrycznym click-to-move, nie physics-based movement.
3. Okna obrażeń w walce przez Animation Events (`OnAttackHit()`), nie przez `Trigger`/`Collider` na czas trwania ataku.
4. `FMODEvents` dostępny przez `GameManager.FMODEvents` (przypisany w Inspektorze) — bez `Resources.Load`.
5. `PlayerStats` — bonusy z ekwipunku (`bonus*`) i skilli (`skillBonus*`) są rozdzielone; `ResetBonuses()` nie dotyka `skillBonus*`.
6. `InventorySystem.RecalculateBonuses()` — jedyne miejsce aplikowania bonusów ekwipunku do `PlayerStats`.
7. `ItemData` i `PlayerStats` pola `bonus*` mapują się 1:1 — dodanie bonusu wymaga aktualizacji obu.
8. `isTwoHanded=true` w MainHand automatycznie zwalnia OffHand; item wraca do baga.
9. `SkillTreeUI` musi siedzieć na rodzicu panelu — `SetActive(false)` na samym panelu wyłącza `Update()`.
10. `SaveSystem` — `itemRegistry` i `questRegistry` to tablice ScriptableObject przypisane ręcznie w Inspektorze; brak SO w registry → item/quest pomijany przy wczytaniu.

**Parametry i ograniczenia:**
| Parametr | Wartość | Jednostka |
|---|---|---|
| Sloty plecaka | 20 | szt. |
| Sloty ekwipunku | 6 (MainHand, OffHand, Helmet, Chest, Legs, Boots) | szt. |
| Próby NavMesh.SamplePosition w Patrol | 8 | szt. |
| Format zapisu | Newtonsoft JSON 3.2.2, Formatting.Indented | — |
| Ścieżka zapisu | `Application.persistentDataPath/save.json` | — |

**Interakcje:**
- [GDD-ARC-001](SkillSystem.md) — System definicji umiejętności (ScriptableObject); `SkillTreeManager` po migracji
- `Documentation/Player/Combat.md` — reguły Animation Events (`OnAttackHit`)
- `Documentation/Systems/SaveSystem.md` — kolejność przywracania danych w `Load()`
- `Documentation/Systems/Inventory.md` — reguły `RecalculateBonuses()`
- `Documentation/Systems/SkillTree.md` — `skillBonus*` oddzielone od `bonus*`
- `Documentation/UI/SkillTreeUI.md` — ograniczenie pozycji `SkillTreeUI` na rodzicu panelu

**Sytuacje wyjątkowe:**
- `FindFirstObjectByType<PlayerController>()` w `EnemyAI.Awake()` — znana sytuacja (zadanie #15, do zastąpienia cache przez GameManager); dopuszczona tymczasowo.

**Decyzje projektowe:**
| Klasa / obszar | Reguła | Status iteracji |
|---|---|---|
| `PlayerStats` | Czysty DTO — zero `EventBus.Publish` wewnątrz; zdarzenia wydaje `PlayerHealthComponent` | gładka |
| `EventBus` | `EventBus.Clear()` nigdy przy zmianie sceny; `DontDestroyOnLoad` zarządza subskrypcjami przez `OnEnable`/`OnDisable` | gładka |
| Ruch | Ruch gracza i wrogów wyłącznie przez `NavMeshAgent`; brak physics-based movement | gładka |
| Walka | Okna obrażeń przez Animation Events (`OnAttackHit()`); bez `Trigger`/`Collider` na czas ataku | gładka |
| Audio | `FMODEvents` przez `GameManager.FMODEvents` (Inspector); bez `Resources.Load` | gładka |
| `PlayerStats` / bonusy | `bonus*` (ekwipunek) i `skillBonus*` (skille) rozdzielone; `ResetBonuses()` nie dotyka `skillBonus*` | gładka |
| `InventorySystem` | `RecalculateBonuses()` — jedyne miejsce aplikowania bonusów ekwipunku | gładka |
| `ItemData` + `PlayerStats` | Pola `bonus*` mapują się 1:1; synchronizacja wymagana przy każdym nowym bonusie | gładka |
| Broń dwuręczna | `isTwoHanded=true` w MainHand zwalnia OffHand automatycznie; item wraca do baga | gładka |
| `SkillTreeUI` | Musi siedzieć na rodzicu panelu (nie na panelu bezpośrednio) — inaczej `SetActive(false)` wyłącza `Update()` | trudna |
| `PlayerCombat` | `animator.ResetTrigger(HashAttack)` w `OnAttackEnd` i fallback — zapobiega stale trigger przy Any State | trudna |
| `PlayerController` | Unik: `DodgeCoroutine` odłącza `agent.updatePosition`, przesuwa Transform bezpośrednio, Warp po zakończeniu | trudna |
| `OcclusionFader` | `OverlapSphere` + filtr Y + filtr kierunku (dot product XZ); wyłącza renderer i collider razem | trudna |

**Implementacja:**
- `Assets/Ratocalypse/Scripts/Player/PlayerStats.cs`
- `Assets/Ratocalypse/Scripts/Player/PlayerHealthComponent.cs`
- `Assets/Ratocalypse/Scripts/Player/PlayerController.cs`
- `Assets/Ratocalypse/Scripts/Player/PlayerCombat.cs`
- `Assets/Ratocalypse/Scripts/Player/PlayerAnimator` — kontroler: `Assets/Cartoon Heroes/Male/3D/Animation Skeleton/PlayerAnimator.controller`
- `Assets/Ratocalypse/Scripts/Player/OcclusionFader.cs` (na Main Camera)
- `Assets/Ratocalypse/Scripts/Enemy/EnemyAI.cs`
- `Assets/Ratocalypse/Scripts/Enemy/EnemyData.cs`
- `Assets/Ratocalypse/Scripts/Inventory/InventorySystem.cs`
- `Assets/Ratocalypse/Scripts/UI/InventoryUI.cs`
- `Assets/Ratocalypse/Scripts/UI/SkillTreeUI.cs`
- `Assets/Ratocalypse/Scripts/Core/SaveSystem.cs`
- `Assets/Ratocalypse/Scripts/Player/XPSystem.cs`
- `Assets/Ratocalypse/Scripts/Core/QuestManager.cs`

**Historia zmian:**
| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. Konwersja 13 wpisów z `.claude/_archiwum/architecture.md` do tabeli decyzji projektowych; wpis `FindFirstObjectByType` przeniesiony do sekcji Sytuacje wyjątkowe (zadanie #15). |
