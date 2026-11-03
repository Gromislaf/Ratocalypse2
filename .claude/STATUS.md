# STATUS — Ratocalypse 2.5

> Prowadzi Prowadzący. **Punkt kontrolny** (między znacznikami) hook wstrzykuje na starcie każdej sesji — nadpisuj go przy każdej bramce, nie dopisuj; do ~25 linii.
> Reszta pliku jest czytana na żądanie (`/stan`). Dziennik zamkniętego zadania przenosi się do jego planu (`.claude/plans/<ID>.md`, sekcja „Dziennik bramek”).
> Etapy: 0 Pomysł · 1 Specyfikacja · 2 Plan · 3a Kod · 3b Integracja · 4 Weryfikacja · 5 Zamknięcie · 6+7 Akceptacja i commit · 8 Push

<!-- CHECKPOINT -->
## Punkt kontrolny

| | |
|---|---|
| Aktywne zadanie | **#27** Migracja starych plików (M) |
| Gałąź | `feature/27-migracja-plikow` |
| Etap | 7 — Akceptacja i commit |
| Plan | `.claude/plans/task-27-migracja-plikow.md` |

**Wykonane:** docs_mechanics.html ✓ · GameDesignDoc.html ✓ · Architecture/Overview.md (GDD-ARC-002) ✓ · assety Unity (3× create_folder, 2× move, 1× delete) ✓ · T1–T5 ✓ · Kronikarz ✓

**Następny krok:** po commicie → `/push`.

**Decyzje z bramki:** 1a NavMesh zostaje · 2c GameManager usunąć z HTML · 3a Planowane systemy — usunąć zaimplementowane · 4b Fabuła — Kanon+Wdrożony · 5a NewItem delete (puste), NewEnemyData move.

Strefy czasowe: brak.
<!-- /CHECKPOINT -->

---

## Dziennik bramek — aktywne zadanie

| Data | Etap | Zdarzenie / decyzja |
|---|---|---|
| 2026-11-03 | 3a+3b+4+5 | docs_mechanics.html, GameDesignDoc.html, GDD-ARC-002 gotowe; assety zmigrowane; T1–T5 ✓; Kronikarz ✓ |

---

## Kolejka

Kolejność = priorytet. Numery z przeglądu zasad (2026-10-25/26). Faza A roadmapy.

| #   | Zadanie                                                                                                                                                                                                        | Ścieżka                   | Zasada       | Uwagi                                                                      |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------- | ------------ | -------------------------------------------------------------------------- |
| 26  | Assembly Definitions + struktura testów                                                                                                                                                                        | pełny                     | Z3           |                                                                            |
| 25  | Plik akcji Input Systemu (5 plików + mapa `Debug`)                                                                                                                                                             | pełny                     | Z14          | **przed serią pomiarów**; po nim włączyć kontrolę `Input.` w `post-cs.ps1` |
| 32  | `InventoryUI.Toggle` odpytuje `Keyboard.current` bezpośrednio — Z14                                                                                                                                            | refaktor                  | Z14          | znalezione przy #10                                                        |
| 18  | Źródło losowości z seedem i strumieniami                                                                                                                                                                       | pełny                     | Z10.1        | **przed serią pomiarów**; po nim włączyć kontrolę `Random` w `post-cs.ps1` |
| 19  | Metryki obciążenia w CSV + tolerancja                                                                                                                                                                          | pełny                     | Z10.2        | w ramach `GDD-OPT-001`                                                     |
| 20  | Overlay i alokacje `PerformanceMonitor` podczas pomiaru                                                                                                                                                        | bugfix                    | Z10.3        |                                                                            |
| 21  | Metodologia `GDD-OPT-001` (czas klatki, percentyle)                                                                                                                                                            | dokumentacja              | Z10          |                                                                            |
| 24  | Przeniesienie `PlayerAnimator.controller`                                                                                                                                                                      | integracja                | Z12          |                                                                            |
| 16  | Enumy z `GameEvents.cs` do osobnych plików                                                                                                                                                                     | refaktor                  | konwencje §1 |                                                                            |
| 6   | Kodowanie: `GameEvents.cs`, `IDamageable.cs`, `ButtonHoverEffect.cs` → UTF-8 (lista w mapie kodu)                                                                                                              | refaktor                  | konwencje §9 |                                                                            |
| 7   | `?.` na obiektach Unity (`PlayerCombat`, `EnemyAI`, `MinimapMarker`)                                                                                                                                           | refaktor                  | konwencje §4 |                                                                            |
| 30  | `NavMeshModifier` (GUID `1e3fdca…`) brakuje na `Player` i `Female B (1)` — usuń komponenty lub przeinstaluj AI Navigation                                                                                      | bugfix                    | —            | niekrytyczne                                                               |
| 31  | Brak wywołania `SaveSystem.Load()` z GameManager/menu — T3/T5 zadania #4 nieweryfikowalne                                                                                                                      | pełny                     | SAV-002      | wykryte przy testach #4                                                    |
| 13  | Drzewko: aktywne umiejętności w planie?                                                                                                                                                                        | GDD                       | Z7.4         | decyzja projektowa                                                         |
| 17  | Dźwięk w grze                                                                                                                                                                                                  | GDD → pełny               | Z9           | obszar roadmapy                                                            |
| 15  | `FindFirstObjectByType` w `Awake` każdego wroga                                                                                                                                                                | optymalizacja             | —            | kandydat na pierwszy `GDD-OPT`                                             |
| 23  | Git LFS dla scen                                                                                                                                                                                               | decyzja                   | —            |                                                                            |
| 27  | Migracja starych plików: opisy z `.claude/_archiwum/architecture.md` → `Documentation/` (EventBus → `Architecture/`); assety ze `Scenes/` → `Data/`; folder `Data/Quests/`; pliki HTML/PDF z katalogu głównego | dokumentacja + integracja | —            | Kronikarz + Integrator; decyzja autora o plikach HTML/PDF                  |
| 29  | Kontrole automatyczne — reszta: struktura plików (folder vs `struktura-projektu.md`)                                                                                                                           | konfiguracja              | Z11.6        | jakość kodu i UnityEditor — w poprawce 3.1                                 |

---

## Aktywne strefy czasowe

| Strefa | Od | Powód |
|---|---|---|
| — | | |

## Działania poza obiegiem

| Data | Co | Na polecenie |
|---|---|---|
| 2026-10-29 | Commit Poprawki 3: agenci, komendy, hooki, CLAUDE.md, narzedzia/ → `setup/poprawka-3` → push `main` `2a9b37d` | autora |
| 2026-11-01 | Commit Poprawki 3.1: kontrole automatyczne → `setup/poprawka-3-1` → scalenie do `main`, wykonany ręcznie | autora |

## Zamknięte (ostatnie 10)

| Data | Zadanie | Commit |
|---|---|---|
| 2026-11-03 | #10 Reguła typ → slot z `InventoryUI` do `InventorySystem` (dziennik: `.claude/plans/task-10-typ-slot-rule.md`) | ac0796d → merge d006fd9 → push |
| 2026-11-03 | #12 Skille do ScriptableObject (zachowanie ID) (dziennik: `.claude/plans/task-12-skille-so.md`) | 0086001 → merge 52c7144 → push |
| 2026-11-02 | #8 Literówka „Ratpocalypse” w `CreateAssetMenu` | 23d1e2a → merge 97e73f1 → push |
| 2026-11-02 | #22 AI Navigation 2.0.12 → 2.0.13 w dokumentacji | bez zmian — nieaktualne (wersja tylko w manifest.json) |
| 2026-11-01 | #4 itemId w ItemData; zapis i odtwarzanie ekwipunku po ID (GDD-ITM-001, Z2.2-3) (dziennik: `.claude/plans/task-04-itemid.md`) | `3741ef4` → merge |
| 2026-11-01 | #3 naprawa przepływu OnCheckpointReached/OnGameSaved w SavePoint (Z1) (dziennik: `.claude/plans/task-03-zdarzenia-zapisu.md`) | `c74957a` → merge `496902a` → push |
| 2026-11-01 | #2 limit pancerza TotalArmor ≤ 0,75 (dziennik: `.claude/plans/02-limit-pancerza.md`) | `c4dd82e` → merge `f3b8463` → push |
| 2026-11-01 | #9 null-check GameManager.Instance w PlayerCombat (Z5.5) | `6ac4b3e` → merge `1715bc2` → push |
| 2026-10-30 | #28 Skille Unity w agentach — rozwiązane w poprawce 3.1 (pole „Skille” w planie) | poprawka 3.1 |
| 2026-10-28 | #1 DamageInfo + GDD-CMB (dziennik: `.claude/plans/task-01-damageinfo.md`) | `e8dbd2d` + `b677ad6` → merge `2649542` → push |
