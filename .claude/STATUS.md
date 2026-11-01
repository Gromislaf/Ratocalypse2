# STATUS — Ratocalypse 2.5

> Prowadzi Prowadzący. **Punkt kontrolny** (między znacznikami) hook wstrzykuje na starcie każdej sesji — nadpisuj go przy każdej bramce, nie dopisuj; do ~25 linii.
> Reszta pliku jest czytana na żądanie (`/stan`). Dziennik zamkniętego zadania przenosi się do jego planu (`.claude/plans/<ID>.md`, sekcja „Dziennik bramek”).
> Etapy: 0 Pomysł · 1 Specyfikacja · 2 Plan · 3a Kod · 3b Integracja · 4 Weryfikacja · 5 Zamknięcie · 6+7 Akceptacja i commit · 8 Push

<!-- CHECKPOINT -->
## Punkt kontrolny

| | |
|---|---|
| Aktywne zadanie | **#3** zdarzenie zapisu (`OnCheckpointReached` vs `OnGameSaved`) — bugfix + dokumentacja |
| Gałąź | `feature/03-zdarzenia-zapisu` |
| Etap | 6+7 — akceptacja i commit |
| Czeka na | ✋ „akceptuję, commit" |

Następne w kolejce: **#4** `itemId` w `ItemData` (pełny, Z2.2–3) · **#12** skille do ScriptableObject (refaktor, Z2.1). Pełna kolejka: `/stan`.
Strefy czasowe: brak.
<!-- /CHECKPOINT -->

---

## Dziennik bramek — aktywne zadanie

| Data | Etap | Zdarzenie / decyzja |
|---|---|---|
| 2026-11-01 | 0 | #3 aktywne, rozmiar S; D: OnCheckpointReached — SavePoint publikuje, GameManager zostaje z logiem |
| 2026-11-01 | 3a–5 | Kod, inspekcja, testy (T1+T2 PASS), dokumentacja SAV-001 v0.2 — gotowe |

---

## Kolejka

Kolejność = priorytet. Numery z przeglądu zasad (2026-10-25/26). Faza A roadmapy.

| # | Zadanie | Ścieżka | Zasada | Uwagi |
|---|---|---|---|---|
| 2 | Limit pancerza: specyfikacja 75%, kod `Clamp01` | bugfix | Z7.1 | |
| 9 | `PlayerCombat` bez sprawdzenia `GameManager.Instance` | bugfix | Z5.5 | |
| 3 | Zdarzenie zapisu `OnGameSaved` vs `OnCheckpointReached` | bugfix + dokumentacja | — | |
| 4 | `itemId` w `ItemData`; zapis po ID; błąd przy nieznanym ID | pełny | Z2.2–3 | zgodność z istniejącymi zapisami |
| 12 | Skille do ScriptableObject (zachowanie ID) | refaktor | Z2.1 | |
| 10 | Reguła typ → slot z `InventoryUI` do `InventorySystem` | refaktor | Z6.3 | |
| 26 | Assembly Definitions + struktura testów | pełny | Z3 | |
| 25 | Plik akcji Input Systemu (5 plików + mapa `Debug`) | pełny | Z14 | **przed serią pomiarów**; po nim włączyć kontrolę `Input.` w `post-cs.ps1` |
| 18 | Źródło losowości z seedem i strumieniami | pełny | Z10.1 | **przed serią pomiarów**; po nim włączyć kontrolę `Random` w `post-cs.ps1` |
| 19 | Metryki obciążenia w CSV + tolerancja | pełny | Z10.2 | w ramach `GDD-OPT-001` |
| 20 | Overlay i alokacje `PerformanceMonitor` podczas pomiaru | bugfix | Z10.3 | |
| 21 | Metodologia `GDD-OPT-001` (czas klatki, percentyle) | dokumentacja | Z10 | |
| 24 | Przeniesienie `PlayerAnimator.controller` | integracja | Z12 | |
| 16 | Enumy z `GameEvents.cs` do osobnych plików | refaktor | konwencje §1 | |
| 6 | Kodowanie: `GameEvents.cs`, `IDamageable.cs`, `ButtonHoverEffect.cs` → UTF-8 (lista w mapie kodu) | refaktor | konwencje §9 | |
| 7 | `?.` na obiektach Unity (`PlayerCombat`, `EnemyAI`, `MinimapMarker`) | refaktor | konwencje §4 | |
| 8 | Literówka „Ratpocalypse” w `CreateAssetMenu` | refaktor | — | |
| 30 | `NavMeshModifier` (GUID `1e3fdca…`) brakuje na `Player` i `Female B (1)` — usuń komponenty lub przeinstaluj AI Navigation | bugfix | — | niekrytyczne |
| 22 | AI Navigation 2.0.12 → 2.0.13 w dokumentacji | dokumentacja | Z11 | |
| 13 | Drzewko: aktywne umiejętności w planie? | GDD | Z7.4 | decyzja projektowa |
| 17 | Dźwięk w grze | GDD → pełny | Z9 | obszar roadmapy |
| 15 | `FindFirstObjectByType` w `Awake` każdego wroga | optymalizacja | — | kandydat na pierwszy `GDD-OPT` |
| 23 | Git LFS dla scen | decyzja | — | |
| 27 | Migracja starych plików: opisy z `.claude/_archiwum/architecture.md` → `Documentation/` (EventBus → `Architecture/`); assety ze `Scenes/` → `Data/`; folder `Data/Quests/`; pliki HTML/PDF z katalogu głównego | dokumentacja + integracja | — | Kronikarz + Integrator; decyzja autora o plikach HTML/PDF |
| 29 | Kontrole automatyczne — reszta: struktura plików (folder vs `struktura-projektu.md`) | konfiguracja | Z11.6 | jakość kodu i UnityEditor — w poprawce 3.1 |

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
| 2026-11-01 | #2 limit pancerza TotalArmor ≤ 0,75 (dziennik: `.claude/plans/02-limit-pancerza.md`) | `c4dd82e` → merge `f3b8463` → push |
| 2026-11-01 | #9 null-check GameManager.Instance w PlayerCombat (Z5.5) | `6ac4b3e` → merge `1715bc2` |
| 2026-10-30 | #28 Skille Unity w agentach — rozwiązane w poprawce 3.1 (pole „Skille” w planie) | poprawka 3.1 |
| 2026-10-28 | #1 DamageInfo + GDD-CMB (dziennik: `.claude/plans/task-01-damageinfo.md`) | `e8dbd2d` + `b677ad6` → merge `2649542` → push |
