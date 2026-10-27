# GDD — Ratocalypse 2.5

> Esencja gry i kierunek rozwoju. **Bez parametrów liczbowych** — liczby są tylko w `Documentation/` (odwołania przez ID).
> Filary zmienia autor (wpis w `.claude/zasady/decyzje.md`). Roadmapę autor zmienia swobodnie.

---

## Czym jest gra

Izometryczne RPG akcji w stylu Diablo (click-to-move), osadzone w postapokaliptycznym świecie zmutowanych szczurów
(`Vision/Lore.md`). Single player, PC; wersja mobilna dopuszczona w przyszłości.

## Filary designu

<!-- 3–5 filarów — uzupełniane w pierwszej sesji /wizja (kopia z Vision/Zasady.md, część 2) -->

## Zakres

| Obszar | Stan | Specyfikacja |
|---|---|---|
| Ruch, unik, kamera | zaimplementowane | `Documentation/Player/` |
| Walka | zaimplementowane | `Documentation/Player/Combat.md` |
| Wrogowie (szczur zwykły) | zaimplementowane | `Documentation/Enemies/` |
| Ekwipunek, przedmioty | zaimplementowane | `Documentation/Systems/Inventory.md`, `Documentation/Assets/Items.md` |
| Doświadczenie, poziomy | zaimplementowane | `Documentation/Systems/XPProgression.md` |
| Drzewko umiejętności (pasywne bonusy) | zaimplementowane | `Documentation/Systems/SkillTree.md` |
| Questy (znajdź przedmiot) | zaimplementowane | `Documentation/Systems/QuestSystem.md` |
| Minimapa, HUD | zaimplementowane | `Documentation/Systems/Minimap.md`, `Documentation/UI/` |
| Zapis gry | zaimplementowane | `Documentation/Systems/SaveSystem.md` |
| System pomiarowy | zaimplementowane | `Documentation/Optimization/` (do uzupełnienia) |
| Dźwięk | brak | — |

## Roadmapa

| Faza | Cel | Stan |
|---|---|---|
| A | Konflikty i nieścisłości z przeglądu zasad (kolejka w `.claude/STATUS.md`) | w toku |
| B | Przegląd zgodności z nowymi wytycznymi (`/audyt`) → nowa porcja zadań | — |
| C | Nowi przeciwnicy — zaczyna się od decyzji o strukturze AI (próg Z8.6) | — |
| D | Umiejętności aktywne — zaczyna się od decyzji o systemie modyfikatorów (próg Z7.4) | — |

## Otwarte pytania projektowe

- Drzewko umiejętności: stary GDD opisywał umiejętności aktywne (Furia, Atak obrotowy…), kod ma pasywne bonusy — które są docelowe? (zadanie 13)
- Dźwięk: zakres i moment wprowadzenia (zadanie 17).
