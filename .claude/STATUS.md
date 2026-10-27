# STATUS — Ratocalypse 2.5

> Prowadzi Prowadzący. Aktualizacja przy każdym przejściu przez bramkę.
> Etapy: 0 Pomysł · 1 Specyfikacja · 2 Plan · 3a Kod · 3b Integracja · 4 Weryfikacja · 5 Zamknięcie · 6 Akceptacja autora · 7 Commit · 8 Push

---

## Aktywne zadanie

| Pole | Wartość |
|---|---|
| Zadanie | — |
| Ścieżka | — |
| Etap | — |
| Czeka na | — |
| Następny krok | — |
| Specyfikacje | — |
| Plan | — |
| Gałąź | — |

### Dziennik bramek

| Data | Etap | Zdarzenie / decyzja |
|---|---|---|

---

## Kolejka

Kolejność = priorytet. Numery z przeglądu zasad (2026-10-25/26). Faza A roadmapy.

| # | Zadanie | Ścieżka | Zasada | Uwagi |
|---|---|---|---|---|
| 1 | `DamageInfo` + specyfikacja reguł otrzymywania obrażeń (`GDD-CMB`) | pełny | Z4.5 | obejmuje #5 (podwójny wzór obrażeń) |
| 2 | Limit pancerza: specyfikacja 75%, kod `Clamp01` | bugfix | Z7.1 | |
| 9 | `PlayerCombat` bez sprawdzenia `GameManager.Instance` | bugfix | Z5.5 | |
| 3 | Zdarzenie zapisu `OnGameSaved` vs `OnCheckpointReached` | bugfix + dokumentacja | — | |
| 4 | `itemId` w `ItemData`; zapis po ID; błąd przy nieznanym ID | pełny | Z2.2–3 | zgodność z istniejącymi zapisami |
| 12 | Skille do ScriptableObject (zachowanie ID) | refaktor | Z2.1 | |
| 10 | Reguła typ → slot z `InventoryUI` do `InventorySystem` | refaktor | Z6.3 | |
| 26 | Assembly Definitions + struktura testów | pełny | Z3 | |
| 25 | Plik akcji Input Systemu (5 plików + mapa `Debug`) | pełny | Z14 | **przed serią pomiarów** |
| 18 | Źródło losowości z seedem i strumieniami | pełny | Z10.1 | **przed serią pomiarów** |
| 19 | Metryki obciążenia w CSV + tolerancja | pełny | Z10.2 | w ramach `GDD-OPT-001` |
| 20 | Overlay i alokacje `PerformanceMonitor` podczas pomiaru | bugfix | Z10.3 | |
| 21 | Metodologia `GDD-OPT-001` (czas klatki, percentyle) | dokumentacja | Z10 | |
| 24 | Przeniesienie `PlayerAnimator.controller` | integracja | Z12 | |
| 16 | Enumy z `GameEvents.cs` do osobnych plików | refaktor | konwencje §1 | |
| 6 | Kodowanie: `IDamageable.cs`, `ButtonHoverEffect.cs` → UTF-8 | refaktor | konwencje §9 | |
| 7 | `?.` na obiektach Unity (`PlayerCombat`, `EnemyAI`, `MinimapMarker`) | refaktor | konwencje §4 | |
| 8 | Literówka „Ratpocalypse” w `CreateAssetMenu` | refaktor | — | |
| 22 | AI Navigation 2.0.12 → 2.0.13 w dokumentacji | dokumentacja | Z11 | |
| 13 | Drzewko: aktywne umiejętności w planie? | GDD | Z7.4 | decyzja projektowa |
| 17 | Dźwięk w grze | GDD → pełny | Z9 | obszar roadmapy |
| 15 | `FindFirstObjectByType` w `Awake` każdego wroga | optymalizacja | — | kandydat na pierwszy `GDD-OPT` |
| 23 | Git LFS dla scen | decyzja | — | przy konfiguracji Wydawcy |
| 27 | Migracja starych plików: opisy z `.claude/_archiwum/architecture.md` → `Documentation/` (EventBus → `Architecture/`); assety ze `Scenes/` → `Data/`; folder `Data/Quests/`; pliki HTML/PDF z katalogu głównego | dokumentacja + integracja | — | Kronikarz + Integrator; decyzja autora o plikach HTML/PDF |
| 28 | Skille Unity w definicjach agentów (pole `skills:`) po ustaleniu nazw (`/unity:`) | konfiguracja | Z11.6 | Prowadzący + autor |
| 29 | Etap automatów: `hookify` + kontrole Z5, Z9, Z10.1, Z14, UTF-8, struktura plików | konfiguracja | Z11.6 | po ustabilizowaniu zasad |

---

## Aktywne strefy czasowe

| Strefa | Od | Powód |
|---|---|---|
| — | | |

## Działania poza obiegiem

| Data | Co | Na polecenie |
|---|---|---|

## Zamknięte (ostatnie 10)

| Data | Zadanie | Commit |
|---|---|---|
