# Rejestr decyzji

> Każda decyzja autora w sprawie zasad, wyjątków i struktury — także odrzucenia (Z13.4).
> Przed złożeniem wniosku sprawdź, czy podobna sprawa nie była już rozstrzygnięta.
> Nowe wpisy na górze.

| Data | Typ | Dotyczy | Decyzja | Uzasadnienie |
|---|---|---|---|---|
| 2026-10-28 | proces | Testy — zadanie #1 vs #26 | Opcja A — pliki `DamageInfoTests.cs` i `Ratocalypse.Tests.EditMode.asmdef` zostają na gałęzi `feature/01-damageinfo` jako zalążek #26; zablokowane `#if RATOCALYPSE_TESTS_ENABLED`; §12 w `konwencje.md` do uzupełnienia w #26 | Logika testów T1–T4 T8–T10 zgodna z kontraktem; bezpieczne dla kompilacji; oszczędność pracy w #26 |
| 2026-10-28 | proces | Cykl — etap 5 | Etap 5 (Zamknięcie: kronikarz → redaktor → audytor) jest obowiązkowy przed etapem 6 na ścieżkach pełnej, bugfix i refaktor | Pominięcie etapu w zadaniu #1; naprawione retroaktywnie |
| 2026-10-27 | struktura | `.gitignore` | Dopisać: `.obsidian/`, `settings.local.json`, `.claude/stan/` | Pliki lokalne i tymczasowe nie należą do repozytorium |
| 2026-10-27 | proces | Wydawca — gałąź | Gałąź `feature/<ID>-…` tworzona gdy zadanie staje się aktywne; tylko przy czystym drzewie roboczym; zakaz `git stash` — niezatwierdzone zmiany przedstawić autorowi i czekać na decyzję | Stash ukrywa zmiany przed autorem i może prowadzić do utraty pracy |
| 2026-10-27 | struktura | Pliki zasad — ładowanie | `zasady-techniczne.md`, `konwencje.md` i pokrewne w `.claude/zasady/` — czytane fragmentami na żądanie, nie ładowane automatycznie; oszczędne czytanie we wszystkich agentach (tylko sekcje wskazane w zleceniu) | Automatyczne ładowanie dużych plików zasad przy każdym zleceniu marnotrawi kontekst |
| 2026-10-27 | narzędzia | `redaktor` — model i wyzwalacze | `redaktor` działa na Haiku z wbudowaną checklistą wytycznych promotora; wywoływany tylko przy: nowej specyfikacji, nowej wersji istniejącej (zmiana zachowania/parametru), `/audyt`; pomijany przy literówkach, formatowaniu i samej stopce *Implementacja* | Haiku wystarczy do sprawdzenia listy kontrolnej; oszczędność przy częstych drobnych zmianach |
| 2026-10-26 | wniosek Z13 | GDD-CMB-006 krok 3, zaokrąglenie pancerza | Opcja A — `Mathf.Round(reduced * 10f) / 10f` przed `Mathf.Max`; zaokrąglenie do 1 m.p.p. zgodne ze spec. Implementacja w `PlayerHealthComponent.TakeDamage(DamageInfo)` (nie `PlayerStats` — `TakeDamage(float)` usunięta per DP-3) | `currentHp` z ułamkami nieakceptowalne w HUD; naprawa w zakresie zadania #1 |
| 2026-10-26 | konwencja | Pola `DamageInfo` (DP-1) | Opcja B — publiczne pola `readonly` camelCase (`value`, `type`, `isCritical`, `knockback`, `source`), spójnie z polami struct-ów zdarzeń w `GameEvents.cs` | Spójność wewnątrz projektu ważniejsza niż idiom ogólny C# |
| 2026-10-26 | konwencja | `namespace` w zadaniu #1 (DP-2) | Opcja A — namespace tylko w nowych plikach; istniejące 4 pliki bez zmian namespace | Minimalna powierzchnia zmiany; istniejące pliki dostaną namespace przy kolejnej edycji |
| 2026-10-26 | projekt | Usunięcie `TakeDamage(float)` z `PlayerHealthComponent` (DP-3) | Opcja A — usunąć; logika trafia do `IDamageable.TakeDamage(DamageInfo)` | Jedno wejście, pełna zgodność z Z4.5; brak wywołujących poza klasą |
| 2026-10-26 | proces | Bramka manifestu | `/manifest-ok` wpisane przez autora odblokowuje zapis sceny na 60 min (hook UserPromptSubmit); zapis tylko dla `integrator` | Techniczne wymuszenie zgody autora |
| 2026-10-26 | narzędzia | MCP Unity | Klasyfikacja: zakaz / odczyt / kompilacja / testy / zapis (6 narzędzi) / wyłączone do wniosku — rejestr | Narzędzia omijające kontrolę (execute_*, edycja skryptów) wyłączone |
| 2026-10-26 | narzędzia | Wtyczki | `csharp-lsp`, Unity (bez CLI/Pipeline, z blokadą skilli), `claude-md-management`, `document-skills`; `hookify` na etapie automatów; Serena wyłączona | Tylko narzędzia, bez wtyczek wnoszących własny proces pracy |
| 2026-10-26 | zasada | Z11.6 | Wtyczki, skille i narzędzia MCP z rejestru; jawne listy narzędzi w definicjach agentów | Subagent bez listy dziedziczy wszystkie narzędzia |
| 2026-10-26 | struktura | Pliki zasad | `.claude/rules/` tylko z `paths` (ładowane wg folderu); rejestry i decyzje w `.claude/zasady/`; rola Prowadzącego w stylu odpowiedzi `prowadzacy` | Subagenci czytają `CLAUDE.md`; pliki `rules/` bez `paths` ładują się do każdej sesji |
| 2026-10-26 | proces | Git | Gałąź `feature/<ID>-…` na start 3a; commit ✋ → scalenie `--no-ff` do `main` ✋ → push ✋ (wariant B) | Commit i push osobno |
| 2026-10-26 | proces | Agenci | Testy pisze `tester` (tylko `Tests/`); `inspektor` i `audytor` na najmocniejszym modelu; `kronikarz` nigdy nie zmienia zachowania ani parametrów w specyfikacji | Kod nie ocenia sam siebie; specyfikacja jest wzorcem |
| 2026-10-26 | proces | Komendy | Polskie nazwy bez znaków diakrytycznych; `/stan` i `/planuj` zamiast `/status` i `/plan` (kolizja z wbudowanymi); `/bug`; `/pomiary start` / `stop`; `/manifest-ok` | — |
| 2026-10-26 | proces | Adwokat diabła | Subagent `adwokat` (tylko czyta); obowiązkowy przed *Szkic* → *Kanon*; rama zasad wizji W1–W5 w `Vision/Zasady.md`, wytyczne autora uzupełniane w pierwszej sesji `/wizja` | Spójność wizji wymaga punktu odniesienia |
| 2026-10-26 | struktura | Warsztat treści | `Vision/Content/` (Story, Quests, Items, Enemies, Characters); stany Szkic → Kanon → Wdrożony; wdrożony plik nie jest edytowany | Jedno źródło prawdy po wdrożeniu |
| 2026-10-26 | struktura | `Vision/` | `GDD.md`, `ArtDirection.md`, `Lore.md`, `Content/`, `_ref/`; `GDD.md` przeniesiony z katalogu głównego do `Vision/` | Wizja oddzielona od specyfikacji dla promotora |
| 2026-10-26 | proces | Agent twórczy | Tryb Prowadzącego `/wizja` (wariant A) | Rozmowa z autorem możliwa tylko w głównej sesji |
| 2026-10-26 | proces | Plan pracy | Fazy: A konflikty z przeglądu → B przegląd zgodności → C nowi przeciwnicy → D aktywne umiejętności; roadmapa w `Vision/GDD.md`, modyfikowalna przez autora bez wniosku | — |
| 2026-10-26 | proces | Prowadzący | Na starcie sesji sam podaje stan; jedno aktywne zadanie naraz | — |
| 2026-10-26 | konwencja | Pola publiczne w definicjach SO | Istniejące klasy zachowują `public`; nowe klasy definicji `[SerializeField] private` + właściwości | Zmiana istniejących grozi utratą danych w assetach |
| 2026-10-26 | konwencja | Namespace | Cały kod w `Ratocalypse`, wprowadzane przy zmianie pliku | Ochrona przed kolizją nazw z pakietami; referencje po GUID się nie psują |
| 2026-10-26 | konwencja | Pola prywatne | `_camelCase`; bez masowej zmiany nazw | Odróżnia pola od zmiennych lokalnych; zmiana nazw serializowanych pól grozi utratą danych |
| 2026-10-26 | zasada | Z14 | Input przez plik akcji Input Systemu jako żelazna zasada | Wersja mobilna dopuszczona w przyszłości; koszt teraz mały, później duży |
| 2026-10-26 | zasada | Z13 | Jeden format wniosku dla wszystkich typów; rejestr decyzji z odrzuceniami | Stabilność zasad; brak powtarzanych wniosków |
| 2026-10-26 | strefa chroniona | Z12, `PlayerAnimator.controller` | Przenieść z `Cartoon Heroes/` do `Assets/Ratocalypse/Animations/` (wariant A), nie robić wyjątku | Usuwa sprzeczność z zakazem modyfikacji paczek; chroni przed nadpisaniem przy aktualizacji |
| 2026-10-26 | zasada | Z12 | Strefy zewnętrzne bez wyjątków; własne przez plan + zgodę; ochrona czasowa systemu pomiarowego | — |
| 2026-10-26 | zasada | Z11 | Zamrożenie na Unity `6000.4.3f1`; bez przejścia na 6.3 LTS | Przejście w dół nieoficjalne i ryzykowne; brak poprawek po 6.5 akceptowalny w horyzoncie pracy |
| 2026-10-26 | zasada | Z10.2 | Wariant B: czas rzeczywisty + weryfikacja metryk obciążenia (nie stały krok czasu gry) | Wybór autora; mniejsza zmiana kodu, gra w normalnym tempie |
| 2026-10-26 | zasada | Z9 | Dźwięki reakcji przez odbiorców EventBus | Logika nie zależy od audio (Z1) |
| 2026-10-26 | zasada | Z8 | Obecny `enum` + `switch` do progu; przebudowa AI dopiero przy pierwszym unikalnym stanie, fazach lub zachowaniu grupowym | Obecna struktura wystarcza dla jednego typu wroga |
| 2026-10-26 | wyjątek odrzucony | Z2.1, `SkillDatabase` | Skille **bez wyjątku** — migracja do ScriptableObject z zachowaniem ID | Spójność z zasadą danych; balans bez zmian w kodzie |
| 2026-10-26 | zasada | Z7 | Zostają dwa źródła bonusów; system modyfikatorów dopiero po przekroczeniu progu | Wszystkie 12 skilli to pasywne bonusy stałe — obecny model wystarcza |
| 2026-10-26 | zasada | Z6 | UI bez reguł gry | — |
| 2026-10-26 | zasada | Z5 | Zamknięta lista 5 singletonów (stan zastany) | — |
| 2026-10-26 | zasada | Z4.5 | `DamageInfo` wprowadzane teraz (wariant A), jako jedno zadanie ze specyfikacją reguł obrażeń | Zmiana mała (4 pliki); reguły obrażeń i tak wymagane w dokumentacji |
| 2026-10-25 | zasada | Z3 | Spójna odpowiedzialność; reguły gry testowalne bez sceny | — |
| 2026-10-25 | wyjątek | Z2.1, `PlayerStats` | Stan runtime zostaje w `PlayerStats` (wariant A) | Single player; problem edytora już rozwiązany przez `[NonSerialized]` + `OnEnable` |
| 2026-10-25 | zasada | Z1 | Powiadomienia przez EventBus, komendy przez metody; moduł = jeden GameObject lub jeden folder funkcji | — |
| 2026-10-25 | struktura dokumentacji | `Optimization/`, prefiks `OPT`, szablon §3c | Zatwierdzone | System pomiarowy i optymalizacje wymagają własnego szablonu |
| 2026-10-25 | struktura dokumentacji | `Architecture/`, prefiks `ARC`, szablon §3b | Zatwierdzone; `EventBus.md` przenoszony z `Systems/` | Dokumentacja obejmuje cały projekt |
| 2026-10-25 | proces | Dokumentacja | Wytyczne promotora jako żelazne zasady dokumentacji; wyjątki od struktury przez wniosek; rejestr struktury osobno | — |
| 2026-10-25 | proces | Cykl pracy | Specyfikacja przed implementacją, zatwierdzana przez autora | Wymóg wytycznych promotora (D8) |
