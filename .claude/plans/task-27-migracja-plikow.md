# Plan: #27 — Migracja starych plików

Rozmiar: M | Gałąź: `feature/27-migracja-plikow`
Zasady: dokumentacja-promotor.md, Z2, Z6

## Zakres

1. Aktualizacja `docs_mechanics.html` (1125 linii, 18 sekcji H2) wg `Documentation/`
2. Aktualizacja `GameDesignDoc.html` (810 linii, 15 sekcji H2) wg `Documentation/`, zachowana struktura
3. Nowy plik `Documentation/Architecture/Overview.md` (GDD-ARC-002) z `.claude/_archiwum/architecture.md`
4. Przeniesienie 3 assetów z `Scenes/` → `Data/` przez Integratora (MCP); nowy folder `Data/Quests/`
5. `Dokumentacja.pdf` — autor przenosi ręcznie, pomijamy

## Decyzje autora (z bramki)

| # | Decyzja |
|---|---|
| 1a | `NavMesh-Enviormant.asset` zostaje w `Scenes/Building/` — nie przenosić |
| 2c | Sekcja „GameManager" w `docs_mechanics.html` — **usunąć** |
| 3a | Sekcja „Planowane systemy" — Kronikarz usuwa zaimplementowane (pokryte przez Documentation/), zostają niezaimplementowane |
| 4b | Sekcja „Fabuła i setting" w `GameDesignDoc.html` — Kronikarz czyta Kanon i Wdrożony z `Vision/Content/` |
| 5a | `NewItem.asset`/`NewEnemyData.asset` — Integrator sprawdza przez MCP; jeśli puste → usuwa zamiast przenosić |

## Faza 1 — Kronikarz: Aktualizacja HTML

### Zasady ogólne

- Źródło prawdy = `Documentation/`. Liczby w HTML muszą być zgodne z `Documentation/`.
- Nie dopisuj historii zmian w HTML.
- Brak pokrycia w `Documentation/` = komentarz `<!-- TODO: brak pokrycia w Documentation/ -->` + dotychczasowa treść (wyjątek: GameManager — usunąć całą sekcję per decyzja 2c).
- Zachowaj strukturę HTML/CSS/klasy; zmieniaj tylko treść wewnątrz tagów.
- Nie czytaj całego HTML od razu — edytuj sekcję po sekcji na podstawie nagłówków `<h2>`.
- Dwa osobne wywołania Kronikarza: osobne dla każdego pliku HTML.

### `docs_mechanics.html` — mapowanie sekcji H2

| Sekcja H2 | Źródło `Documentation/` | Uwagi |
|---|---|---|
| EventBus | `Systems/EventBus.md` | Tabela zdarzeń 1:1 |
| GameManager | — | **USUNĄĆ** (decyzja 2c) |
| Audio — FMOD | `Assets/Audio.md` | Metody AudioManager, Z9 |
| Statystyki gracza | `Player/Stats.md` | HP=100, +15/level, maxLevel=20, bonusy |
| Zdrowie i stamina | `Player/Stats.md` + `Player/Combat.md` (GDD-CMB-003/004/006) | iframes, regen, redukcja pancerzem |
| Ruch i unik | `Player/Movement.md` | priorytety kliknięcia, dodgeCost=25, moveSpeed=5 |
| Walka gracza | `Player/Combat.md` (GDD-CMB-001–006) | cykl, fallback, Animation Events, interfejsy |
| Kamera | `Player/Camera.md` | OcclusionFader — nie modyfikować logiki ⚠️ |
| Wrogowie — AI i FSM | `Enemies/EnemyAI.md` + `Enemies/EnemyTypes.md` | 6 stanów, detectRange=8m, attackRange=1.5m, patrolRadius=4m, cooldown=1.5s, xp=25 |
| System ekwipunku | `Systems/Inventory.md` (INV-001–006) | Reguła typ→slot w InventorySystem (zadanie #10) |
| Przedmioty | `Assets/Items.md` (GDD-ITM-001) | typy, bonusy |
| HUD gracza | `UI/HUD.md` | inicjalizacja, Image.fillAmount |
| Ekwipunek UI | `UI/InventoryUI.md` | 20 bag + 6 equip, toggle I |
| Planowane systemy | — | Kronikarz usuwa zaimplementowane (mają plik w Documentation/); zostają tylko faktycznie nieistniejące |

### `GameDesignDoc.html` — mapowanie sekcji H2

| Sekcja H2 | Źródło `Documentation/` |
|---|---|
| Przegląd projektu | `GameOverview.md` + `CoreGameplay.md` |
| Architektura i wzorce | `Systems/EventBus.md` + `Architecture/SkillSystem.md` |
| System gracza | `Player/Movement.md` + `Player/Camera.md` + `Player/Stats.md` |
| System walki | `Player/Combat.md` |
| Wrogowie (AI) | `Enemies/EnemyAI.md` + `Enemies/EnemyTypes.md` |
| Ekwipunek i przedmioty | `Systems/Inventory.md` + `Assets/Items.md` + `UI/InventoryUI.md` |
| Doświadczenie i poziomy | `Systems/XPProgression.md` |
| Drzewko umiejętności | `Systems/SkillTree.md` + `Architecture/SkillSystem.md` |
| System questów | `Systems/QuestSystem.md` + `UI/QuestDialogUI.md` |
| Fabuła i setting | `Vision/Content/` — **Kanon i Wdrożony** (decyzja 4b) |
| Minimapa | `Systems/Minimap.md` |
| Interfejs użytkownika | `UI/HUD.md` + `UI/InventoryUI.md` + `UI/SkillTreeUI.md` + `UI/QuestDialogUI.md` |
| Audio | `Assets/Audio.md` |
| Status implementacji | badge per sekcja — ocena z Documentation/ (co ma plik = Zaimplementowana) |
| Sterowanie | `GameOverview.md` |

## Faza 2 — Kronikarz: `Documentation/Architecture/Overview.md`

Nowy plik, szablon §3b (element architektury).

**ID:** `GDD-ARC-002`
**Cel:** Zebranie decyzji architektonicznych obowiązujących w całym projekcie.

Struktura:
- Jeden element GDD-ARC-002 wg szablonu §3b
- 13 bloków z archiwum staje się wierszami tabeli „Decyzje projektowe" (kolumny: Klasa/obszar, Reguła, Status iteracji)
- Oznaczenia ⚠️/✅ z archiwum → kolumna „Status iteracji": wartości `trudna`/`gładka` (D1 — nie emoji w polach GDD)
- `FindFirstObjectByType w Awake ⚠️` → wiersz w „Sytuacjach wyjątkowych" (znane, zadanie #15)
- `SkillTreeUI NA RODZICU panelu` → wiersz w „Decyzje projektowe" z uzasadnieniem

Źródła Kronikarza:
- `.claude/_archiwum/architecture.md` (149 linii — Read całości)
- `.claude/zasady/dokumentacja-rejestr.md` — potwierdzenie prefiks GDD-ARC
- `Documentation/Architecture/SkillSystem.md` — wzorzec formatowania

## Faza 3 — Integrator: przeniesienie assetów

Przez MCP (`manage_asset`). Kolejność:

1. `manage_asset action=create_folder` → `Assets/Ratocalypse/Data/Quests`
2. `manage_asset action=get_info` → `Assets/Ratocalypse/Scenes/NewItem.asset` → jeśli puste: `action=delete`; jeśli wypełnione: `action=move` → `Data/Items/NewItem.asset`
3. `manage_asset action=get_info` → `Assets/Ratocalypse/Scenes/NewEnemyData.asset` → jeśli puste: `action=delete`; jeśli wypełnione: `action=move` → `Data/Enemies/NewEnemyData.asset`
4. `manage_asset action=move` → `Assets/Ratocalypse/Scenes/Game/New Quest Data.asset` → `Assets/Ratocalypse/Data/Quests/NewQuestData.asset` (zmiana nazwy — usunięcie spacji)
5. `NavMesh-Enviormant.asset` — **nie ruszać** (zostaje w `Scenes/Building/`)

Uwaga: `move` zachowuje GUID → referencje w scenach nie pękną (Z2).

## Pliki z integracji

- `Assets/Ratocalypse/Data/Quests/` (nowy folder)
- `Assets/Ratocalypse/Data/Items/` (nowy folder)
- `Assets/Ratocalypse/Data/Enemies/` (nowy folder)
- `Assets/Ratocalypse/Data/Enemies/NewEnemyData.asset`
- `Assets/Ratocalypse/Data/Quests/NewQuestData.asset`
- `Assets/Ratocalypse/Scenes/NewItem.asset` (usunięty)

## Przypadki testowe — poziom 1

| # | Co sprawdzić | Oczekiwane |
|---|---|---|
| T1 | Otworzyć `docs_mechanics.html` w przeglądarce | 17 sekcji H2 (bez GameManager), liczby zgodne z Documentation/ |
| T2 | Otworzyć `GameDesignDoc.html` w przeglądarce | 15 sekcji H2, badge Status implementacji aktualny |
| T3 | Otworzyć `Documentation/Architecture/Overview.md` w edytorze | GDD-ARC-002 pełny, tabela 13 wierszy |
| T4 | Unity Editor: otwórz scenę gameplay, sprawdź nawigację | Wrogowie poruszają się, brak missing references |
| T5 | Project Window po migracji | `Data/Quests/NewQuestData.asset` istnieje; stare ścieżki w Scenes/ — nieobecne |

## Ryzyka

| Ryzyko | Mitygacja |
|---|---|
| GUID zmienia się przy `manage_asset action=move` | Grep po GUID w plikach .unity po migracji; jeśli zmieniony → cofnąć |
| Kronikarz dopisuje liczby spoza Documentation/ | Jawna reguła: brak pokrycia = `<!-- TODO -->`, nie wymyślanie danych |
| HTML psuje się po edycji | Kronikarz zmienia tylko treść wewnątrz istniejących tagów |

## Dziennik bramek

| Data | Etap | Zdarzenie |
|---|---|---|
| 2026-11-03 | 0+1+2 | Rozmiar M zatwierdzony; spec + plan zaakceptowane; decyzje: 1a, 2c, 3a, 4b, 5a |
