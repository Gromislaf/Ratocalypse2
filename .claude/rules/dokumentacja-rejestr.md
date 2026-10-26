# Rejestr struktury dokumentacji

> Aktualna struktura `Documentation/`, prefiksy ID i zatwierdzone wyjątki.
> Zasady: `dokumentacja-promotor.md` §7.
> Agent edytuje ten plik **wyłącznie** po to, by wprowadzić wyjątek zatwierdzony przez autora.

---

## Struktura folderów

```
Documentation/
├── GameOverview.md
├── CoreGameplay.md
├── Player/          Stats, Movement, Combat, Camera
├── Enemies/         EnemyTypes, EnemyAI
├── Systems/         Inventory, QuestSystem, XPProgression, SkillTree, SaveSystem, Minimap
├── Levels/          Building
├── UI/              HUD, InventoryUI, SkillTreeUI, QuestDialogUI, MainMenu*, PauseMenu*, GameOver*
├── Assets/          Items, Audio, Animations*
├── Architecture/    EventBus, Overview*, DataModel*, Services*
├── Optimization/    Methodology*, OPT-xxx*
└── _img/            grafiki według obszarów
```
`*` — do utworzenia, gdy pojawi się treść.

## Prefiksy ID

| Prefiks | Obszar | Plik | Szablon |
|---|---|---|---|
| OVR | Ogólna specyfikacja | GameOverview.md | §3a |
| LOOP | Pętla rozgrywki | CoreGameplay.md | §3a |
| STA | Statystyki postaci | Player/Stats.md | §3a |
| MOV | Ruch, unik | Player/Movement.md | §3a |
| CAM | Kamera | Player/Camera.md | §3a |
| CMB | Walka | Player/Combat.md | §3a |
| ENM | Typy przeciwników | Enemies/EnemyTypes.md | §3a |
| AI | Zachowania AI | Enemies/EnemyAI.md | §3a |
| INV | Ekwipunek | Systems/Inventory.md | §3a |
| QST | Questy | Systems/QuestSystem.md | §3a |
| PRG | Doświadczenie i poziomy | Systems/XPProgression.md | §3a |
| SKL | Drzewko umiejętności | Systems/SkillTree.md | §3a |
| SAV | Zapis gry | Systems/SaveSystem.md | §3a |
| MAP | Minimapa | Systems/Minimap.md | §3a |
| LVL | Poziomy | Levels/*.md | §3a |
| UI | Interfejs | UI/*.md | §3a |
| ITM | Katalog przedmiotów | Assets/Items.md | §3a |
| AUD | Audio | Assets/Audio.md | §3a |
| ANM | Animacje | Assets/Animations.md | §3a |
| ARC | Architektura | Architecture/*.md | §3b |
| OPT | Optymalizacja i pomiary | Optimization/*.md | §3c |

## Zatwierdzone wyjątki

| Data | Zmiana | Uzasadnienie | Zatwierdził |
|---|---|---|---|
| 2026-10-25 | Folder `Architecture/`, prefiks `ARC`, szablon §3b | Dokumentacja obejmuje cały projekt; architektura nie jest mechaniką gry i nie pasuje do szablonu §3a | autor |
| 2026-10-25 | Folder `Optimization/`, prefiks `OPT`, szablony §3c | System pomiarowy i optymalizacje wymagają własnego szablonu (metodologia, wyniki przed/po) | autor |
