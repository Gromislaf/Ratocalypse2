# CLAUDE.md — Ratocalypse 2.5

Izometryczne RPG akcji (click-to-move, Diablo-like) · Unity `6000.4.3f1` · URP 17.4.0 · C# · FMOD · single player, PC.
Projekt jest przedmiotem pracy inżynierskiej autora. **Wszyscy agenci piszą po polsku**; identyfikatory w kodzie po angielsku.

**Kim jesteś:** subagent — rolę, narzędzia i granice określa Twoja definicja w `.claude/agents/`. Główna sesja to **Prowadzący**
(`.claude/output-styles/prowadzacy.md`: cykl pracy, rozmiary zadań, bramki, skrypty) i jako jedyna rozmawia z autorem.
Jedno aktywne zadanie; stan wyłącznie w `.claude/STATUS.md`; plany w `.claude/plans/task-<NN>-<nazwa>.md`.

## Decyzje — czyje?

| Autor (zawsze pytanie ✋) | Agent (sam, w granicach zasad; wypisuje w wyniku „Decyzje techniczne”) |
|---|---|
| zachowanie gry, liczby, kolejność reguł widoczna dla gracza; fabuła, kanon, styl | nazwy prywatnych pól i metod, podział na metody, kolejność kroków |
| zatwierdzenie spec / planu / manifestu, commit, push | plik w granicach `struktura-projektu.md`, logi `[DBG]`, zapis testu |
| wnioski Z13, wyjątki, nowe pakiety, singletony | wybór między równoważnymi rozwiązaniami zgodnymi z zasadami |

## Żelazne zasady — poziom 0
Pełna treść: `.claude/zasady/zasady-techniczne.md` (sekcja `## Zn` — zakres linii z mapy dokumentacji). Rejestry: `zasady-rejestr.md`. Decyzje: `decyzje.md`.

- **Z1.** Powiadomienia między systemami tylko przez EventBus (struct); komendy i zapytania przez metody lub interfejs; zależności bez cykli.
- **Z2.** Definicje w ScriptableObject, w runtime tylko do odczytu, z trwałym unikalnym ID; stan runtime zmienia tylko jego właściciel.
- **Z3.** Komponent ma jedną spójną odpowiedzialność; reguły gry da się wywołać i przetestować bez sceny.
- **Z4.** Obiekty oddziałują na siebie przez interfejsy, nigdy przez konkretną klasę celu; obrażenia jako `DamageInfo`.
- **Z5.** Singletony tylko z rejestru; nowy singleton albo zmiana czasu życia wymaga wniosku.
- **Z6.** UI wyświetla stan i przekazuje intencje gracza; reguły gry decyduje logika, nigdy UI.
- **Z7.** Statystyka = baza + bonusy z nazwanych źródeł; jedno miejsce obliczania z limitami ze specyfikacji.
- **Z8.** AI to jawne stany z jedną drogą przejścia; parametry w EnemyData; różnice między typami przez dane i stany.
- **Z9.** Audio wyłącznie przez FMOD (`AudioManager`/`StudioEventEmitter`/`FMODEvents`); dźwięki reakcji przez zdarzenia.
- **Z10.** Losowość tylko ze źródeł z seedem; pomiary porównywalne tylko przy zgodnych metrykach obciążenia.
- **Z11.** Wersje silnika, pakietów, wtyczek i narzędzi zamrożone; zmiana tylko przez wniosek; nigdy w trakcie serii pomiarów.
- **Z12.** Paczek zewnętrznych nie modyfikujemy; strefy chronione tylko przez zatwierdzony plan.
- **Z13.** Zasad się nie łamie ani nie obchodzi; konflikt = zatrzymanie i wniosek; każda decyzja do rejestru.
- **Z14.** Rozgrywka reaguje na akcje z pliku akcji Input Systemu, nigdy bezpośrednio na urządzenia.

Dokumentacja: `.claude/rules/dokumentacja-promotor.md` (specyfikacja, liczby z jednostkami, ID `GDD-<PREFIKS>-<NNN>`, historia zmian).
Konwencje kodu: `.claude/zasady/konwencje.md` — m.in. **zmiana nazwy pola serializowanego tylko z `[FormerlySerializedAs("stara")]`**
(inaczej przepadają referencje w scenach i assetach; hook ostrzega, skrypt kontroli blokuje commit). Struktura plików: `.claude/zasady/struktura-projektu.md`.

## Czytanie — wszyscy agenci
1. **Kod:** najpierw `.claude/mapa-kodu.md` (klasy, API z liniami, zdarzenia, singletony, SO, pola z Inspektora) → `LSP`/`Grep` → `Read` z `offset`/`limit`.
   Cały plik tylko, gdy zmieniasz jego większość. Mapa jest generowana — przy rozbieżności wierz plikowi.
2. **Dokumentacja i zasady:** najpierw `.claude/mapa-dokumentacji.md` (nagłówki z zakresami linii, tabela elementów GDD) → `Read` zakresu.
3. **Zmiany:** `git diff main...HEAD` i `git diff`. **Szukanie w plikach — narzędziem `Grep`**, nigdy `grep`/`Select-String`/`cat` przez Bash.
4. **Unity:** stan sceny i komponentów najpierw przez MCP (jeśli masz); pliki `.unity`/`.prefab`/`.asset` — Grepem, tylko gdy MCP nie wystarcza.
5. Plików streszczonych w zleceniu nie czytasz ponownie. **Raport telegraficzny:** tabela/punkty, wynik, uwagi, decyzje — bez prozy i streszczeń.

## Narzędzia — wszyscy agenci
- Tylko narzędzia z Twojej definicji; zablokowane = zgłoś w wyniku, bez obejść. Instalacje — tylko tego, o co autor poprosił wprost.
- `git` bez `-C` i bez `cd` (jesteś w folderze projektu). `git commit/merge/push` — wyłącznie `wydawca`.
- Komunikaty hooków są wiążące: odmowa czytania w całości → czytaj sekcję; `[kontrola jakości]` po zapisie `.cs` → popraw albo uzasadnij.
- `.meta`, `.unity`, `.prefab`, `.asset` nigdy ręcznie; zapis sceny i assetów przez MCP — tylko `integrator` po `/manifest-ok`. Kod — zwykłą edycją plików, nigdy przez MCP.
- `Assets/Ratocalypse/Tests/` zablokowane do zadania #26. Wtyczki, skille, MCP: rejestr w `zasady-rejestr.md`.
- Wniosek o zmianę zasady (Z13) zwracasz w wyniku — nie łamiesz zasady „na chwilę”.

## Gdzie co jest
Kod i assety: `Assets/Ratocalypse/` · specyfikacja: `Documentation/` · wizja i treści: `Vision/` (`Vision/Content/`: Szkic → Kanon → Wdrożony) ·
zasady, rejestry, decyzje: `.claude/zasady/` · wytyczne dokumentacji: `.claude/rules/` · agenci / komendy / styl / hooki / skrypty: `.claude/agents|commands|output-styles|hooks|narzedzia/`.
