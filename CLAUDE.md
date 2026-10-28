# CLAUDE.md — Ratocalypse 2.5

Izometryczne RPG akcji (click-to-move, Diablo-like) · Unity `6000.4.3f1` · URP 17.4.0 · C# · FMOD · single player, PC (mobile dopuszczony w przyszłości).
Projekt jest przedmiotem pracy inżynierskiej autora (architektura zdarzeniowa, mechaniki, zapis stanu, deterministyczny pomiar wydajności).

**Wszyscy agenci piszą do autora i w raportach po polsku.** Identyfikatory w kodzie po angielsku.

**Kim jesteś:** jeśli działasz jako subagent — Twoją rolę, narzędzia i granice określa Twoja definicja w `.claude/agents/`.
Główna sesja działa w stylu **Prowadzący** (`.claude/output-styles/prowadzacy.md`) i jako jedyna rozmawia z autorem.

---

## Cykl pracy

| Etap | Kto | Bramka (✋ = wyraźna zgoda autora) |
|---|---|---|
| 0 Pomysł | Prowadzący + autor (treść: `/wizja`) → `adwokat` | ✋ zadanie i jego **rozmiar S/M/L**; zadanie aktywne → `wydawca` tworzy gałąź (czyste drzewo) |
| 1 Specyfikacja | Prowadzący + autor (`/spec`) → `redaktor` | zgodność z wytycznymi promotora; ✋ (przy M razem z planem) |
| 2 Plan techniczny | `architekt` (`/planuj`) | ✋ plan (przy S — plan skrócony) |
| 3a Kod | `programista` | kompilacja bez błędów |
| 3b Integracja | `integrator` | ✋ `/manifest-ok`; konsola bez nowych błędów |
| 4 Weryfikacja | `inspektor` (diff) → `tester` | brak 🔴; scenariusze testów przechodzą |
| 5 Zamknięcie | `kronikarz` (+ `redaktor`, jeśli zmieniła się wersja specyfikacji) | **twarda bramka** — bez niej nie ma commita |
| 6+7 Akceptacja i commit | autor → Prowadzący (`/commit`: skrypt kontroli) → `wydawca` | ✋ „akceptuję, commit” → commit i scalenie do `main` |
| 8 Push | `wydawca` (`/push`) | ✋ push gałęzi i `main` |

**Rozmiar zadania** (proponuje Prowadzący, zatwierdza autor w etapie 0):

| | Kiedy | Etapy |
|---|---|---|
| **S** | ≤ 3 pliki, bez nowego zachowania (bugfix, refaktor, balans, literówka) | bez etapu 1 (lub tylko poprawka wartości); plan skrócony; Inspektor tylko 🔴 |
| **M** | nowa reguła lub mechanika w istniejącym systemie | specyfikacja i plan zatwierdzane **w jednej bramce** |
| **L** | nowy system, zmiana architektury, system pomiarowy | pełny cykl |

Bramki, których **nigdy** się nie łączy ani nie pomija: manifest Integratora, push, status *Kanon*, wnioski Z13.
Zmiana założeń specyfikacji w etapach 3–6 → powrót do etapu 1 (nowa wersja + historia zmian).
`audytor` — tylko `/audyt` i koniec fazy roadmapy, nie w każdym zadaniu.
**Jedno aktywne zadanie naraz, jedno zadanie = jedna sesja** (po pushu `/clear`). Stan wyłącznie w `.claude/STATUS.md` — bez plików pamięci. Plany: `.claude/plans/<ID>.md`.

## Decyzje — czyje?

| Autor (zawsze pytanie ✋) | Agent (decyduje sam w ramach zasad, wypisuje w wyniku „Decyzje techniczne”) |
|---|---|
| co gra robi: zachowanie, liczby, kolejność reguł widoczna dla gracza | nazwy prywatnych pól i metod, podział na metody, kolejność kroków implementacji |
| fabuła, kanon, styl | w którym pliku (w granicach `struktura-projektu.md`) |
| zatwierdzenie spec/planu/manifestu, commit, push | jak zapisać test, jakie logi `[DBG]` |
| wnioski Z13, wyjątki, nowe pakiety, singletony | wybór między równoważnymi rozwiązaniami zgodnymi z zasadami |

Autor może zawetować każdą decyzję techniczną. Pytania do autora zbiera się w **jedną wiadomość z rekomendacjami**; „ok” = przyjęte wszystkie rekomendacje.

---

## Żelazne zasady — poziom 0

Pełna treść: `.claude/zasady/zasady-techniczne.md`. Rejestry: `.claude/zasady/zasady-rejestr.md`. Decyzje: `.claude/zasady/decyzje.md`.

- **Z1.** Powiadomienia między systemami tylko przez EventBus (struct); komendy i zapytania przez metody lub interfejs; zależności bez cykli.
- **Z2.** Definicje w ScriptableObject, w runtime tylko do odczytu, z trwałym unikalnym ID; stan runtime zmienia tylko jego właściciel.
- **Z3.** Komponent ma jedną spójną odpowiedzialność; reguły gry da się wywołać i przetestować bez sceny.
- **Z4.** Obiekty oddziałują na siebie przez interfejsy, nigdy przez konkretną klasę celu; obrażenia jako `DamageInfo`.
- **Z5.** Singletony tylko z rejestru; nowy singleton albo zmiana czasu życia wymaga wniosku.
- **Z6.** UI wyświetla stan i przekazuje intencje gracza; reguły gry decyduje logika, nigdy UI.
- **Z7.** Statystyka = baza + bonusy z nazwanych źródeł; jedno miejsce obliczania z limitami ze specyfikacji; każde źródło zarządza tylko swoimi bonusami.
- **Z8.** AI to jawne stany z jedną drogą przejścia; parametry w EnemyData; różnice między typami przez dane i dodatkowe stany.
- **Z9.** Audio wyłącznie przez FMOD, `AudioManager`/`StudioEventEmitter` i `FMODEvents`; dźwięki reakcji przez zdarzenia EventBus.
- **Z10.** Losowość tylko ze źródeł z seedem (osobne strumienie); pomiary porównywalne tylko przy zgodnych metrykach obciążenia; pomiar nie wpływa na wynik.
- **Z11.** Wersja silnika, pakietów, wtyczek i narzędzi agentów zamrożona; zmiana tylko przez wniosek; nigdy w trakcie serii pomiarów.
- **Z12.** Paczek zewnętrznych nie modyfikujemy nigdy; strefy chronione tylko przez plan wymieniający ochronę, zatwierdzony przez autora.
- **Z13.** Zasad się nie łamie ani nie obchodzi; konflikt = zatrzymanie i wniosek; każda decyzja do rejestru decyzji.
- **Z14.** Rozgrywka reaguje na akcje z pliku akcji Input Systemu, nigdy bezpośrednio na urządzenia.

**Dokumentacja** (`Documentation/`): `.claude/rules/dokumentacja-promotor.md` — specyfikacja, nie wypracowanie; 7 aspektów;
liczby z jednostkami; ID `GDD-<PREFIKS>-<NNN>`; historia zmian; specyfikacja przed implementacją. Struktura: `.claude/zasady/dokumentacja-rejestr.md`.

**Konwencje kodu:** `.claude/zasady/konwencje.md`. **Struktura plików projektu:** `.claude/zasady/struktura-projektu.md`.

---

## Czytanie — zasady wspólne (wszyscy agenci)

1. **Kod: najpierw `.claude/mapa-kodu.md`** — klasy, publiczne API z numerami linii, graf zdarzeń, singletony, SO, pola z Inspektora.
   Potem `LSP` (definicje, odwołania) i `Read` z `offset`/`limit` tylko tych metod, które zmieniasz lub sprawdzasz. Cały plik — tylko gdy zmieniasz jego większość.
   Mapa jest generowana (nagłówek: commit, data); przy rozbieżności z plikiem wierz plikowi. Mapy nie edytuje się ręcznie.
2. **Przegląd zmian = diff:** `git diff main...HEAD` i niezatwierdzone `git diff`. Okolice zmian — fragmentami.
3. **Zasady fragmentami:** skrót Z1–Z14 jest wyżej; pełną sekcję czytasz tylko dla zasad ze zlecenia
   (`Grep -n "^## Z" .claude/zasady/zasady-techniczne.md` → `Read` sekcji). `decyzje.md`, rejestry, struktura, konwencje — `Grep` po słowach kluczowych, nie całość.
4. **Unity:** stan sceny, komponenty, konsola — najpierw przez MCP (jeśli masz narzędzie); pliki `.unity`/YAML dopiero, gdy MCP nie wystarcza.
5. Plików streszczonych w zleceniu nie czytasz ponownie. Raport zwięzły: wyniki, uwagi, decyzje — bez streszczania przeczytanych plików.

## Narzędzia — zasady wspólne

- Używasz **tylko** narzędzi ze swojej definicji. Zablokowane narzędzie = nie szukaj obejścia; zgłoś to w wyniku.
- Kod edytuje się wyłącznie zwykłą edycją plików — nigdy przez MCP (`create_script`, `apply_text_edits` itp. są zablokowane).
- Zapis w scenie i assetach przez MCP: tylko `integrator`, tylko po `/manifest-ok` wpisanym przez autora.
- Wtyczki, skille i narzędzia MCP: rejestr w `.claude/zasady/zasady-rejestr.md` (Z11.6).
- Wniosek o zmianę zasady (Z13) zwracasz w ustalonym formacie jako część wyniku — nie łamiesz zasady „na chwilę”.

---

## Mapa plików

| Co | Gdzie |
|---|---|
| Stan prac, kolejka | `.claude/STATUS.md` |
| Mapa kodu (generowana) | `.claude/mapa-kodu.md` ← `.claude/narzedzia/mapa-kodu.ps1` |
| Skrypty bez agentów (mapa, kontrola przed commitem) | `.claude/narzedzia/` |
| Plany techniczne | `.claude/plans/<ID>.md` |
| Wytyczne dokumentacji (ładowane automatycznie przy pracy w `Documentation/`) | `.claude/rules/` |
| Zasady techniczne, konwencje, rejestry, decyzje, struktura — czytane na żądanie, fragmentami | `.claude/zasady/` |
| Definicje subagentów / komendy / styl Prowadzącego | `.claude/agents/` / `.claude/commands/` / `.claude/output-styles/` |
| Hooki | `.claude/hooks/` (PowerShell 7) |
| Specyfikacja gry i projektu (dla promotora) | `Documentation/` |
| Wizja, filary, roadmapa / styl, świat, zasady wizji | `Vision/GDD.md` / `Vision/ArtDirection.md`, `Vision/Lore.md`, `Vision/Zasady.md` |
| Warsztat treści (Szkic → Kanon → Wdrożony) | `Vision/Content/` |
| Kod i assety gry | `Assets/Ratocalypse/` |
