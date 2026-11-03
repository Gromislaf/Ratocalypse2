# Plan: #12 — Refaktor skille do ScriptableObject

**Rozmiar:** M | **Gałąź:** `feature/12-skille-so` | **Spec:** `Documentation/Architecture/SkillSystem.md` GDD-ARC-001

---

## Pliki do zmiany

| Plik | Status | Co |
|---|---|---|
| `Assets/Ratocalypse/Scripts/Core/SkillDefinitionSO.cs` | **nowy** | `SkillDefinitionSO : ScriptableObject` + `SkillEffectType` enum; pola `[SerializeField]` prywatne z getterami read-only (Z2.1); `[CreateAssetMenu(menuName="Ratocalypse/Skill Definition")]` |
| `Assets/Ratocalypse/Scripts/Core/SkillRegistrySO.cs` | **nowy** | `SkillRegistrySO : ScriptableObject`; `[SerializeField] SkillDefinitionSO[] _skills`; `IReadOnlyList<SkillDefinitionSO> All`; `SkillDefinitionSO GetById(string id)` liniowo (12 el.); `[CreateAssetMenu(menuName="Ratocalypse/Skill Registry")]` |
| `Assets/Ratocalypse/Scripts/Core/SkillDefinition.cs` | **usunięty** | Zastąpiony przez powyższe; `SkillBranch` pozostaje w `GameEvents.cs` |
| `Assets/Ratocalypse/Scripts/Player/SkillTreeManager.cs` | **zmieniony** | Dodać `[SerializeField] SkillRegistrySO _registry` + getter `public SkillRegistrySO Registry => _registry`; zmienić `CanUnlock(in SkillDefinition)` → `CanUnlock(SkillDefinitionSO)`; `TryUnlock` i `RestoreFromSave` używają `_registry.GetById` zamiast `SkillDatabase.GetById`; przy nieznanym ID → `null` + `Debug.LogWarning` |
| `Assets/Ratocalypse/Scripts/UI/SkillNodeUI.cs` | **zmieniony** | `Initialize(SkillDefinitionSO skill, …)`; `_skill` typu `SkillDefinitionSO`; dostępy przez gettery SO |
| `Assets/Ratocalypse/Scripts/UI/SkillTreeUI.cs` | **zmieniony** | Iteracja po rejestrze przez `skillTreeManager.Registry.All` zamiast `SkillDatabase.All` |

## Etap 3b — instrukcja dla integratora

1. Utwórz folder `Assets/Ratocalypse/Data/Skills/`.
2. Utwórz 12 assetów `SkillDefinitionSO` (nazwa pliku = `<id>.asset`); wypełnij pola 1:1 z tablicy w `SkillDatabase` (aktualny `SkillDefinition.cs:42`):

| Id | DisplayName | Branch | Tier | EffectType | EffectValue | Prerequisite |
|---|---|---|---|---|---|---|
| warrior_t1 | Mega Siła | Warrior | 1 | Damage | 5 | — |
| warrior_t2 | Żelazna Skóra | Warrior | 2 | Armor | 0.08 | warrior_t1 |
| warrior_t3 | Berserker | Warrior | 3 | AttackSpeed | 0.3 | warrior_t2 |
| warrior_t4 | Śmiertelny Cios | Warrior | 4 | CritChance | 0.1 | warrior_t3 |
| hunter_t1 | Szybkość | Hunter | 1 | MoveSpeed | 0.8 | — |
| hunter_t2 | Wytrzymałość | Hunter | 2 | MaxStamina | 30 | hunter_t1 |
| hunter_t3 | Precyzja | Hunter | 3 | CritChance | 0.08 | hunter_t2 |
| hunter_t4 | Szybki atak | Hunter | 4 | AttackSpeed | 0.4 | hunter_t3 |
| survivor_t1 | Twardziel | Survivor | 1 | MaxHp | 40 | — |
| survivor_t2 | Regeneracja | Survivor | 2 | HpRegen | 1.5 | survivor_t1 |
| survivor_t3 | Forteca | Survivor | 3 | Armor | 0.07 | survivor_t2 |
| survivor_t4 | Witalność | Survivor | 4 | MaxHp | 60 | survivor_t3 |

3. Utwórz `Assets/Ratocalypse/Data/Skills/SkillRegistry.asset` (`SkillRegistrySO`); dodaj wszystkie 12 SO do `_skills`.
4. W scenie na GameObject z `SkillTreeManager`: podepnij `SkillRegistry.asset` pod pole `_registry`.
5. Weryfikacja: konsola bez błędów po wejściu w Play Mode.

## Kolejność kroków implementacji

1. `SkillDefinitionSO.cs` (nowy)
2. `SkillRegistrySO.cs` (nowy)
3. `SkillTreeManager.cs` (aktualizacja)
4. `SkillNodeUI.cs` (aktualizacja)
5. `SkillTreeUI.cs` (aktualizacja)
6. Usunięcie `SkillDefinition.cs`
7. Kompilacja bez błędów → bramka 3a
8. Integrator → etap 3b

## Przypadki testowe (poziom 1 — widać w grze)

| # | Akcja | Oczekiwanie |
|---|---|---|
| T1 | Odblokuj skill Tier 1 (mając punkt) | Kolor zmienia się na unlocked, licznik -1, bonus widoczny w statystykach |
| T2 | Próba odblokowania Tier 2 bez prereq | Klik ignorowany, kolor unavailable |
| T3 | Odblokuj Tier 1 → odblokuj Tier 2 w tej samej gałęzi | Odblokowanie się udaje |
| T4 | Odblokuj skille → SavePoint → wyjście → Load | Skille wracają, bonusy aktywne |

T5 (poziom 2 — log): wpisać nieistniejące ID w save.json → Load → konsola ma `LogWarning`, nie wyjątek.

## Decyzje techniczne

- `SkillEffectType` → `SkillDefinitionSO.cs` (enum ściśle powiązany z SO)
- `SkillBranch` pozostaje w `GameEvents.cs:249` (używany przez `OnSkillUnlocked`)
- `GetById` liniowo (12 elementów — brak potrzeby `Dictionary`)
- Nazwy assetów SO = `<id>.asset` (stabilne, niezależne od nazwy wyświetlanej)
- `SkillTreeUI` dostaje rejestr przez `SkillTreeManager.Registry` getter (jedno pole w scenie)
- Nieznane ID w `RestoreFromSave` → `null` + `Debug.LogWarning` (nie wyjątek)

## Ryzyka

- R1 (średnie): `SkillTreeUI.RefreshAll()` może używać `SkillDatabase.All` → rozwiązane przez getter `Registry` w kroku 4.
- R2 (niskie): inne pliki `.cs` używające `SkillDatabase` / `SkillDefinition` → Grep po usunięciu wychwytuje.

## Wpływ na dokumentację

- GDD-ARC-001 → status `Zaimplementowana` po etapie 4 (kronikarz)
- `Systems/SkillTree.md` → odwołanie do `SkillRegistrySO` zamiast `SkillDatabase` (kronikarz)

## Dziennik bramek

| Data | Etap | Zdarzenie |
|---|---|---|
| 2026-11-03 | 0 | Rozmiar M zatwierdzony |
| 2026-11-03 | 1+2 | Spec GDD-ARC-001 i plan — bramka (oczekuje) |
