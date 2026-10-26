# Zasady dokumentacji projektowej

> Źródło: wytyczne promotora (`Dokumentacja.pdf`), zaadaptowane do projektu Ratocalypse 2.5.
> Ten plik jest **żelazną zasadą** dla wszystkiego w `Documentation/`.
> Zmiana tylko przez wniosek zaakceptowany przez autora. Agenci nie edytują tego pliku.
> **[projekt]** = uzupełnienie ustalone w projekcie, nie pochodzi z wytycznych.

---

## 1. Żelazne zasady

**D1. Specyfikacja, nie wypracowanie.** Dokumentacja opisuje, jak projekt działa — nie koncepcję ani przebieg prac.

**D2. Precyzja.** Żadnych określeń typu „szybko”, „blisko”, „pewien czas”, „rośnie z czasem”.
Każda wartość ma liczbę i jednostkę (m, m/s, m/s², s, ms, HP, pkt, pkt/s, %, °, B, MB).
Dotyczy także warunków: „pościg, gdy odległość od gracza ≤ 8 m”, nie „gdy gracz jest blisko”.

**D3. Kompletność według szablonu.** Każdy element zawiera wszystkie pola szablonu właściwego dla swojego folderu (§3a, §3b, §3c).
Pole, które nie ma sensu, oznacza się „nie dotyczy” — nie pomija się go.

**D4. Wzory wprost.** Każdy wzór jawnie, z opisem zmiennych, sposobem zaokrąglania i limitami.
Przy pancerzu, krytykach lub odpornościach — jawna kolejność obliczeń.

**D5. Tabele zamiast opisów** dla parametrów, katalogów i wyników pomiarów.

**D6. Tylko aktualny stan.** Treść opisuje wyłącznie obowiązującą wersję. Wcześniejsze reguły są w historii zmian.

**D7. Historia zmian.** Każdy element ma ID i tabelę wersji wyjaśniającą ewolucję decyzji (nie przebieg prac).

**D8. Specyfikacja przed implementacją.** Nowa mechanika lub element architektury jest specyfikowany i zatwierdzony przez autora przed implementacją.
Jeżeli w trakcie testów zmienią się założenia — specyfikacja jest aktualizowana, a zmiana trafia do historii.

**D9. Struktura odpowiada projektowi.** Pliki tworzy się tylko dla tego, co faktycznie istnieje lub jest zatwierdzone. Struktura i prefiksy: `dokumentacja-rejestr.md` (§7).

**D10. Brak zmyślonych danych.** [projekt] Każda liczba w `Optimization/` pochodzi z pomiaru i wskazuje plik źródłowy (CSV).
Brak danych = `TODO: pomiar`, nigdy wartość szacowana.

---

## 2. Wymagane sekcje dokumentacji gry

| # | Sekcja | Plik(i) | Musi zawierać |
|---|---|---|---|
| 1 | Ogólna specyfikacja | `GameOverview.md` | tytuł, gatunek, tryb, perspektywa, platforma, sterowanie, główny cel, warunek zwycięstwa, warunek porażki |
| 1 | Główna pętla | `CoreGameplay.md` | czynności powtarzane przez gracza; przejścia między etapami; źródło progresji; zakończenie pętli |
| 2 | Postać | `Player/*.md` | tabela parametrów; każda mechanika na poziomie „mechaniki sprintu” z wytycznych, z przypadkiem brzegowym |
| 3 | Walka | `Player/Combat.md` | niezależne mechaniki; tabela broni; celowanie, wykrywanie trafień, reakcja na trafienie; reguły obrażeń jako numerowana lista |
| 4 | Przeciwnicy i AI | `Enemies/*.md` | tabela właściwości każdego typu; każdy stan FSM z wejściem i wyjściem (§4) |
| 5 | Poziomy | `Levels/*.md` | mapa z obszarami; start gracza; rozmieszczenie wrogów, przedmiotów, przeszkód; cele i warunki ukończenia; zasady spawnu; przejścia; zdarzenia; ograniczenia ruchu; skala, wymiary, ID elementów; przeznaczenie obszarów |
| 6 | Interfejs | `UI/*.md` | każdy ekran (menu główne, HUD, pauza, zwycięstwo, porażka, inne); każdy element: wygląd, zachowanie, widoczność; interakcje, nawigacja, sytuacje nietypowe |
| 7 | Progresja i balans | `Systems/XPProgression.md`, `Systems/SkillTree.md` | tabela progresji; źródła XP; poziom maksymalny; przenoszenie XP między rozgrywkami; wzory z zaokrąglaniem i limitami |
| 8 | Katalog zawartości | `Assets/*.md` + tabele w plikach systemów | broń, przedmioty, przeciwnicy, umiejętności, obiekty interaktywne, dźwięki, animacje |
| 9 | Historia zmian | w każdym pliku | §5 |

Sekcje projektu [projekt]:

| Sekcja | Folder | Szablon |
|---|---|---|
| Architektura | `Architecture/` | §3b |
| Optymalizacja i pomiary | `Optimization/` | §3c |

---

## 3a. Szablon mechaniki gry

```markdown
## GDD-<PREFIKS>-<NNN> <Nazwa mechaniki>

**Status:** Projekt | Zatwierdzona | Zaimplementowana
**Wersja:** 0.1

**Opis funkcjonalny:** <co robi — 1–2 zdania>
**Warunki uruchomienia:** <kiedy i w jakich okolicznościach działa>
**Zasady działania:**
1. <operacja>
**Ograniczenia:** <kiedy niedostępne / czego nie można>
**Parametry:**
| Parametr | Wartość | Jednostka | Zakres |
|---|---|---|---|
**Wzory:** <jawnie; zmienne; kolejność; zaokrąglanie; limity — lub „nie dotyczy”>
**Interakcje:** <wpływ na inne systemy, z ID>
**Sytuacje wyjątkowe:**
- <warunek> → <zachowanie>
**Informacja zwrotna:** <co gracz widzi i słyszy>
**Implementacja:** <ścieżki skryptów, kluczowe klasy i pola>
**Historia zmian:**
| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. |
```

## 3b. Szablon elementu architektury [projekt]

```markdown
## GDD-ARC-<NNN> <Nazwa elementu>

**Status:** Projekt | Zatwierdzona | Zaimplementowana
**Wersja:** 0.1

**Cel:** <jaki problem rozwiązuje>
**Odpowiedzialność:** <za co odpowiada, a za co wyraźnie NIE>
**Elementy:**
| Klasa / plik | Rola |
|---|---|
**Kontrakt (API):** <publiczne metody, zdarzenia, interfejsy — sygnatury>
**Zasady użycia:**
1. <reguła>
**Parametry i ograniczenia:** <wydajność, alokacje, limity — z liczbami>
**Interakcje:** <systemy korzystające, z ID>
**Sytuacje wyjątkowe:**
- <warunek> → <zachowanie>
**Decyzje projektowe:**
| Decyzja | Rozważane alternatywy | Uzasadnienie |
|---|---|---|
**Implementacja:** <ścieżki skryptów>
**Historia zmian:**
| Wersja | Zmiana |
|---|---|
```

## 3c. Szablony optymalizacji i pomiarów [projekt]

**Metodologia pomiaru** (opisana raz, w `Optimization/Methodology.md`):

```markdown
## GDD-OPT-001 Metodologia pomiaru

**Cel:** <co mierzymy i po co>
**Środowisko:** <sprzęt, system, wersja Unity, build/edytor, VSync, rozdzielczość>
**Scenariusze:**
| ID scenariusza | Scena | Wrogowie | Nagranie inputu | Seed | Warmup | Czas pomiaru |
|---|---|---|---|---|---|---|
**Metryki:**
| Metryka | Jednostka | Źródło | Agregacja |
|---|---|---|---|
| Czas klatki | ms | PerformanceMonitor | mediana, p95, p99 |
**Procedura:** <liczba powtórzeń, kolejność, co resetujemy między przebiegami>
**Ograniczenia metody:** <co nie jest deterministyczne i jaki ma to wpływ>
**Implementacja:** <BenchmarkRunner.cs, BenchmarkInputRecorder.cs, PerformanceMonitor.cs>
**Historia zmian:**
| Wersja | Zmiana |
|---|---|
```

**Pojedyncza optymalizacja:**

```markdown
## GDD-OPT-<NNN> <Nazwa optymalizacji>

**Status:** Projekt | Zatwierdzona | Zaimplementowana | Odrzucona
**Wersja:** 0.1

**Problem:** <co mierzalnie jest źle — metryka, scenariusz, wartość>
**Hipoteza:** <dlaczego, i co powinno się zmienić>
**Zmiana:** <co dokładnie zmieniono, z ID dotkniętych specyfikacji>
**Scenariusz pomiaru:** <ID z GDD-OPT-001>
**Wyniki:**
| Metryka | Przed | Po | Zmiana | Plik źródłowy |
|---|---|---|---|---|
**Wnioski:** <czy hipoteza się potwierdziła; skutki uboczne>
**Implementacja:** <ścieżki skryptów>
**Historia zmian:**
| Wersja | Zmiana |
|---|---|
```

Odrzucona optymalizacja (brak poprawy) zostaje w dokumentacji ze statusem „Odrzucona” — to też wynik.

---

## 4. Wymagania szczególne

**Reguły otrzymywania obrażeń** — numerowana lista kroków w kolejności wykonania
(np. sprawdzenie komponentu → iframes → pancerz → krytyk → odjęcie HP → dolny limit 0 → stan śmierci).

**Stany AI** — dla każdego stanu: zachowanie, warunki wejścia, warunki wyjścia. Dla pościgu i całej FSM dodatkowo:
- częstotliwość aktualizacji trasy,
- zachowanie po utracie kontaktu wzrokowego,
- czas pamiętania ostatniej pozycji gracza,
- warunek przerwania pościgu,
- reakcja na przeszkody,
- zachowanie przy braku dostępnej drogi.

Diagram stanów jest ilustracją — nie zastępuje reguł.

**Element UI** — tabela cech:

| Cecha | Zachowanie |
|---|---|
| Widoczność | |
| Wartość | |
| Aktualizacja | |
| Kolor standardowy / ostrzegawczy | |
| Wartość zerowa | |

**Pozycja katalogu** — tabela z co najmniej: ID, nazwa, typ, efekt, ograniczenia, warunek użycia, czas działania,
zachowanie w sytuacji nietypowej, efekt wizualny, efekt dźwiękowy (ID zdarzenia FMOD).

---

## 5. Identyfikatory i historia zmian

**Format ID:** `GDD-<PREFIKS>-<NNN>`. Numeracja w obrębie prefiksu, numery nie są ponownie używane.

Prefiksy i przypisane im pliki: `dokumentacja-rejestr.md`.

**Wersje:** każdy element ma własny numer (0.1, 0.2…). Nowa wersja = zmiana zachowania, parametru lub kontraktu.
Literówka czy formatowanie nie zmienia wersji.

**Wpis w historii** opisuje *co i dlaczego*, np.
„Dodano 1 s opóźnienia regeneracji po sprincie — sprint bez kary był dominującą strategią.”
Nie opisuje pracy („przepisano skrypt”, „naprawiono bug”).

---

## 6. Checklista Redaktora

- [ ] ID w formacie `GDD-<PREFIKS>-<NNN>`, status, wersja.
- [ ] Wszystkie pola właściwego szablonu (§3a/§3b/§3c) obecne lub „nie dotyczy”.
- [ ] Każda liczba ma jednostkę; brak słów nieprecyzyjnych.
- [ ] Wzory jawne, z zaokrąglaniem, limitami i kolejnością obliczeń.
- [ ] Parametry, katalogi i wyniki w tabelach.
- [ ] Co najmniej jeden przypadek w „Sytuacjach wyjątkowych”.
- [ ] Tekst opisuje tylko aktualną wersję; zmiany w historii.
- [ ] Wpisy historii opisują decyzje, nie przebieg prac.
- [ ] Interakcje odwołują się do ID innych elementów.
- [ ] (AI) każdy stan ma wejście/wyjście + 6 pytań z §4.
- [ ] (UI) każdy element ma tabelę cech.
- [ ] (OPT) każda liczba ma plik źródłowy; scenariusz odwołuje się do GDD-OPT-001.
- [ ] Plik leży we właściwym folderze, prefiks zgodny z rejestrem (`dokumentacja-rejestr.md`).

Format raportu:
```
GDD-XXX-NNN — ✅ zgodne | ⚠️ braki | ❌ niezgodne
  - [D2] „porusza się szybko” → podaj wartość w m/s
  - [D3] brak pola „Informacja zwrotna”
```

---

## 7. Organizacja dokumentacji w projekcie

**Lokalizacja:** `Documentation/` w katalogu głównym repozytorium, wyłącznie Markdown.
**Aktualna struktura folderów, prefiksy i zatwierdzone wyjątki:** `dokumentacja-rejestr.md`.

**Zasady:**
1. Jeden plik = jeden obszar. Plik zawiera wiele elementów, każdy z własnym ID. Powyżej ok. 10 elementów — podział na podfolder.
2. Nowy plik powstaje tylko dla elementu, który istnieje lub jest zatwierdzony do realizacji.
3. Nazwy plików: PascalCase po angielsku. Treść po polsku.
4. Odwołania: ID + link względny, np. `[GDD-INV-002](../Systems/Inventory.md)`.
5. Grafiki w `_img/<obszar>/`. Diagramy stanów i przepływów jako Mermaid w treści pliku.
6. W `Documentation/` NIE ma: planów technicznych (`.claude/plans/`), stanu prac (`.claude/STATUS.md`),
   zasad dla agentów (`.claude/rules/`), wizji i roadmapy (`GDD.md` w katalogu głównym).
7. `.obsidian/` to ustawienia edytora, nie dokumentacja — agenci go ignorują.
8. **Wyjątki od struktury.** Rejestr opisuje stan obecny, nie zamkniętą listę.

   **Bez wniosku** wolno:
   - dodać nowy plik w istniejącym folderze dla zatwierdzonego elementu (zasada 2),
   - dodać nowy element z istniejącym prefiksem do istniejącego pliku.

   **Wniosek o wyjątek** jest wymagany przy: nowym folderze, nowym prefiksie, nowym lub zmodyfikowanym szablonie,
   przeniesieniu pliku, podziale pliku na podfolder. Agent wtedy:
   - nie tworzy folderu ani prefiksu i nie wciska elementu do niepasującego miejsca;
   - zgłasza wniosek: czego dotyczy element; który istniejący folder był najbliższy i dlaczego odpada;
     proponowana lokalizacja, prefiks i szablon; wpływ na istniejące pliki;
   - czeka na decyzję autora (zatwierdzenie, zmiana lub odrzucenie).

   Zatwierdzony wyjątek wprowadza się do `dokumentacja-rejestr.md` — to jedyna dozwolona edycja rejestru przez agenta.
