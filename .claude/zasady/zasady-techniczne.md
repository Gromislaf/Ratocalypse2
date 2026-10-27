# Żelazne zasady techniczne — Ratocalypse 2.5

> Obowiązują wszystkich agentów i każdą zmianę w kodzie, scenach i assetach.
> Zmiana tylko przez wniosek (Z13) zatwierdzony przez autora. Edycja tego pliku wyłącznie w zakresie zatwierdzonej decyzji.
> Rejestry (singletony, strefy chronione, wyjątki, narzędzia MCP, wtyczki): `.claude/zasady/zasady-rejestr.md`. Historia decyzji: `.claude/zasady/decyzje.md`.
> Kod niezgodny z zasadą = zadanie na liście (Z13.6), nie powód do zmian poza cyklem.

---

## Poziom 0 — skrót (kopiowany do CLAUDE.md)

- **Z1.** Powiadomienia między systemami tylko przez EventBus (struct); komendy i zapytania przez metody lub interfejs; zależności bez cykli.
- **Z2.** Definicje w ScriptableObject, w runtime tylko do odczytu, z trwałym unikalnym ID; stan runtime zmienia tylko jego właściciel.
- **Z3.** Komponent ma jedną spójną odpowiedzialność; reguły gry da się wywołać i przetestować bez sceny.
- **Z4.** Obiekty oddziałują na siebie przez interfejsy (`IDamageable`, `IHealable`, `IStatusEffectable`, `IInteractable`), nigdy przez konkretną klasę celu.
- **Z5.** Singletony tylko z rejestru; nowy singleton albo zmiana czasu życia wymaga wniosku.
- **Z6.** UI wyświetla stan i przekazuje intencje gracza; reguły gry decyduje logika, nigdy UI.
- **Z7.** Statystyka = wartość bazowa + bonusy z nazwanych źródeł; jedno miejsce obliczania wartości końcowej z limitami ze specyfikacji; każde źródło zarządza tylko swoimi bonusami.
- **Z8.** AI to jawne stany z jedną drogą przejścia; parametry w EnemyData; różnice między typami przez dane i dodatkowe stany, nie przez warunki na typie.
- **Z9.** Audio wyłącznie przez FMOD i `AudioManager`/`StudioEventEmitter`; zdarzenia FMOD tylko przez `FMODEvents`; dźwięki reakcji podpinane do zdarzeń EventBus.
- **Z10.** Losowość rozgrywki tylko ze źródeł z seedem (osobne strumienie); przebiegi pomiaru porównywalne tylko przy zgodnych metrykach obciążenia; pomiar nie wpływa na wynik.
- **Z11.** Wersja silnika, pakietów, wtyczek i narzędzi agentów zamrożona; zmiana tylko przez wniosek; nigdy w trakcie serii pomiarów.
- **Z12.** Paczek zewnętrznych nie modyfikujemy nigdy; strefy chronione z rejestru zmienia się tylko przez plan wymieniający ochronę, zatwierdzony przez autora.
- **Z13.** Zasad się nie łamie ani nie obchodzi; konflikt z zasadą = zatrzymanie i wniosek; każda decyzja trafia do rejestru decyzji.
- **Z14.** Rozgrywka reaguje na akcje z jednego pliku akcji Input Systemu, nigdy bezpośrednio na urządzenia; logika udostępnia komendy niezależne od inputu.

---

## Z1. Komunikacja między systemami

**Zasada:**
1. Systemy powiadamiają się o tym, co się wydarzyło, wyłącznie przez EventBus. Zdarzenia są typu `struct`.
2. Komendy („zrób to”) i zapytania („podaj mi to”) idą przez publiczne metody wykonawcy albo przez jego interfejs.
3. Zależności między systemami są jednokierunkowe. Jeśli A woła B, to B nie woła A, tylko publikuje zdarzenie.
4. Wewnątrz jednego modułu bezpośrednie referencje są dozwolone. **Moduł** = komponenty na jednym GameObjekcie albo skrypty w jednym folderze funkcji (np. `Scripts/Player/`, `Scripts/Inventory/`).

**Dlaczego:** nadawca powiadomienia nie wie, kto reaguje — nowe reakcje dodaje się bez zmian w istniejącym kodzie. Komenda potrzebuje konkretnego wykonawcy i odpowiedzi; przepychanie jej przez bus ukrywa przepływ sterowania. Zakaz cykli zapobiega splątaniu systemów.

**Dozwolone:** `EnemyAI` publikuje `OnEnemyDied`, a `XPSystem`, `QuestManager` i minimapa reagują niezależnie. `InventoryUI` woła `inventory.Equip(item)` i sprawdza wynik. `PlayerCombat` czyta `PlayerController.CurrentTarget` (ten sam moduł).

**Niedozwolone:** `EnemyAI` trzyma referencję do `XPSystem` i sam przyznaje XP. Komenda udająca zdarzenie (`OnEquipRequested` → czekanie na `OnEquipSucceeded`). `InventorySystem` woła metody `InventoryUI` zamiast publikować `OnInventoryChanged`.

**Sprawdzanie:** Inspektor. Automatycznie tylko część przypadków (zdarzenia `*Requested`).

**Opis architektury:** `Documentation/Architecture/EventBus.md` (GDD-ARC).

---

## Z2. Dane definicyjne i stan gry

**Zasada:**
1. Definicje (przedmioty, wrogowie, questy, umiejętności, parametry bazowe gracza) są w ScriptableObject. Kod nigdy nie zapisuje do serializowanych pól SO w runtime.
2. Każda definicja, do której odwołuje się zapis gry albo inna definicja, ma trwałe, unikalne ID, osobne od nazwy wyświetlanej. Po pierwszym użyciu w zapisie ID się nie zmienia.
3. Zapis gry odwołuje się do definicji wyłącznie przez ID. Nieznane ID przy wczytywaniu = błąd w konsoli, nigdy ciche pominięcie.
4. Każde pole stanu runtime ma jednego właściciela, który je zmienia. Pozostali tylko czytają. Wyjątek: SaveSystem przy wczytywaniu.

| Stan | Właściciel |
|---|---|
| HP, stamina, XP, poziom, punkty umiejętności | `PlayerHealthComponent` |
| `bonus*` | `InventorySystem` |
| `skillBonus*` | `SkillTreeManager` |

**Dlaczego:** definicje to dane projektu, stan to dane rozgrywki — zmieszane gubią zapisy i dają trudne do znalezienia bugi. ID oddzielone od nazwy pozwala zmieniać teksty i balans bez psucia zapisów. Jeden właściciel stanu = wiadomo, gdzie szukać błędnej wartości.

**Dozwolone:** `InventorySystem.RecalculateBonuses()` ustawia `stats.bonusDamage`. `SaveSystem.Load()` przywraca `stats.currentHp`.

**Niedozwolone:** `EnemyAI` odejmuje `stats.currentHp` bezpośrednio (właściwa droga: `IDamageable`). Zmiana `itemData.bonusDamage` w runtime. Zapis używa `itemName`.

**Sprawdzanie:** Inspektor. Unikalność i niepustość ID — walidator w edytorze (etap automatów).

**Wyjątek zatwierdzony:** stan runtime w `PlayerStats` — patrz `.claude/zasady/zasady-rejestr.md`.

---

## Z3. Odpowiedzialność komponentów

**Zasada:**
1. Komponent odpowiada za jedną funkcję gry (np. ruch, walkę, zdrowie). Ocenia się to na poziomie funkcji, nie liczby linii.
2. Reguły gry (wzory obrażeń i XP, warunki zakładania przedmiotów, prereq drzewka, przejścia stanów questa i AI) są w metodach, które nie wymagają sceny, kamery ani fizyki: zwykłe klasy C#, metody ScriptableObject albo metody statyczne.
3. MonoBehaviour łączy reguły z Unity: input, `Update`, animator, NavMesh, publikacja zdarzeń.
4. Komponenty zależne od siebie deklarują to przez `[RequireComponent]`.

**Dlaczego:** wąska odpowiedzialność ułatwia znalezienie błędu i zmianę bez skutków ubocznych. Reguły poza sceną da się sprawdzić testem EditMode zamiast ręcznego grania — sytuacje wyjątkowe ze specyfikacji stają się przypadkami testowymi.

**Dozwolone:** `PlayerStats.TakeDamage()` liczy redukcję pancerzem, `PlayerHealthComponent` publikuje zdarzenia i obsługuje iframes. Definicje skilli w danych, `SkillTreeManager` łączy je ze stanem gracza.

**Niedozwolone:** wzór obrażeń wpisany w `Update()` albo w Animation Event. Komponent obsługujący jednocześnie ruch i ekwipunek. Warunek „czy można założyć przedmiot” sprawdzany tylko w UI.

**Sprawdzanie:** Inspektor.

---

## Z4. Interakcje między obiektami

**Zasada:**
1. Zadawanie obrażeń, leczenie, nakładanie efektów i interakcja z obiektem odbywają się wyłącznie przez odpowiedni interfejs. Wołający nie wie, czy celem jest gracz, wróg czy obiekt.
2. Interfejs to jedyna droga do zmiany stanu innego obiektu w tych obszarach (uzupełnia Z2.4).
3. Referencję do interfejsu pobiera się raz (`Awake`, moment wybrania celu) i przechowuje — nie co klatkę.
4. Nowy rodzaj oddziaływania = nowy interfejs albo rozszerzenie istniejącego przez wniosek, nie rzutowanie na konkretną klasę.
5. Obrażenia przekazuje się jako jedna struktura `DamageInfo` (wartość, typ, krytyk, odrzut, źródło), zgodnie ze specyfikacją reguł otrzymywania obrażeń (`GDD-CMB`).

**Dlaczego:** walka, questy i interakcje działają z każdym nowym typem obiektu bez zmian w kodzie wołającego. Jedna struktura obrażeń pozwala dodawać typy, odporności i efekty bez zmiany sygnatur we wszystkich implementacjach.

**Dozwolone:** `EnemyAI` trafia gracza przez `IDamageable`. `PlayerController` woła `IInteractable.Interact()`, nie wiedząc, czy to NPC, przedmiot czy SavePoint.

**Niedozwolone:** `if (target is EnemyAI enemy) enemy.ApplyStun(...)` zamiast `IStatusEffectable`.

**Sprawdzanie:** Inspektor; rzutowania na konkretne klasy celów — wyszukiwanie automatyczne z oceną.

---

## Z5. Usługi globalne (singletony)

**Zasada:**
1. Singleton to wyłącznie usługa, która ze swojej natury występuje w grze raz. Obiekty rozgrywki (gracz, wrogowie, przedmioty) nigdy nie są singletonami.
2. Dozwolone są tylko singletony z rejestru. Rejestr określa czas życia każdego: cała gra albo jedna scena.
3. `DontDestroyOnLoad` tylko dla singletonów oznaczonych w rejestrze jako „cała gra”.
4. Singleton udostępnia funkcje przez metody i właściwości (Z1), nie udostępnia swoich wewnętrznych referencji do innych systemów.
5. Kod, który może działać w scenie bez danego singletona (Benchmark, testy), sprawdza jego obecność.

**Dlaczego:** singleton to globalna zależność. Każdy nowy zwiększa liczbę ukrytych powiązań, utrudnia testy i uruchamianie scen w izolacji. Zamknięta lista wymusza świadomą decyzję.

**Dozwolone:** `SavePoint.Interact()` woła `SaveSystem.Instance.Save(id)`. `EnemyAI` czyta `GameManager.Instance.IsPaused` po sprawdzeniu obecności `Instance`.

**Niedozwolone:** `PlayerController.Instance`. Nowy `UIManager.Instance` bez wniosku.

**Sprawdzanie:** automatycznie — klasy ze `static … Instance` porównane z rejestrem.

---

## Z6. Interfejs użytkownika

**Zasada:**
1. UI czyta stan (bezpośrednio albo z danych zdarzenia) i go wyświetla.
2. UI przekazuje intencję gracza do logiki jedną komendą (Z1). O skutkach decyduje logika.
3. UI nie zawiera reguł gry: warunków, mapowań typów na sloty, wzorów ani limitów. Wolno liczyć tylko to, co służy prezentacji (wypełnienie paska, kolor ostrzegawczy).
4. UI odświeża się na zdarzenia, nie przez odpytywanie stanu co klatkę.
5. Przełączanie i wzajemne zamykanie paneli odbywa się przez zdarzenia.

**Dlaczego:** reguła w UI działa tylko po kliknięciu — ten sam efekt wywołany z innego miejsca (podniesienie, wczytanie zapisu, test, skrót) ją ominie. Logika w jednym miejscu = jedno miejsce do testowania i opisania w specyfikacji.

**Dozwolone:** `SkillNodeUI` woła `TryUnlock(id)`, manager sprawdza punkty i prereq. `PlayerHUD` liczy `fillAmount = currentHp / maxHp`.

**Niedozwolone:** UI decyduje, czy przedmiot jest konsumablem i do którego slotu trafia. UI sprawdza „czy gracza stać” zamiast zapytać logikę.

**Sprawdzanie:** Inspektor; odwołania do typów logiki (`ItemType`, `EquipmentSlot`, wzorów) w `Scripts/UI/` — wyszukiwanie automatyczne z oceną.

---

## Z7. Statystyki i bonusy

**Zasada:**
1. Wartość końcowa każdej statystyki (`Total*`, `MaxHp`…) jest liczona wyłącznie w `PlayerStats`, według wzoru i limitów z `Documentation/Player/Stats.md`.
2. Każde źródło bonusów ma własny zestaw pól i własny reset: ekwipunek (`bonus*`, `InventorySystem`), drzewko (`skillBonus*`, `SkillTreeManager`). Źródło nie zmienia ani nie resetuje bonusów innego źródła.
3. Bonus ze źródła jest zawsze przeliczany od zera ze stanu źródła (założone przedmioty, odblokowane skille), nie dodawany przyrostowo.
4. **Próg przebudowy.** Wniosek o system modyfikatorów (`GDD-ARC`) jest obowiązkowy przed implementacją któregokolwiek z:
   - trzeciego źródła bonusów (statusy, buffy, aury, mikstury czasowe),
   - bonusu czasowego,
   - bonusu procentowego lub mnożnikowego,
   - bonusu warunkowego (zależnego od stanu).

   Do tego czasu nie dodaje się nowych zestawów pól `xxxBonus*`.

**Dlaczego:** jedno miejsce obliczeń = jedna prawda zgodna ze specyfikacją. Rozdzielone źródła się nie nadpisują. Przeliczanie od zera daje identyczny wynik po wczytaniu zapisu. Próg zatrzymuje rozrost pól i wymusza decyzję w momencie, gdy jest potrzebna.

**Dozwolone:** `RecalculateBonuses()` zeruje i sumuje `bonus*` z założonych przedmiotów. `RestoreFromSave()` zeruje `skillBonus*` i nakłada skille od nowa.

**Niedozwolone:** `PlayerCombat` mnoży obrażenia „bo buff aktywny”. Nowe pola `buffBonusDamage` bez wniosku.

**Sprawdzanie:** Inspektor; nowe pola `*Bonus*` w `PlayerStats` — automatycznie.

**Powiązanie:** przekroczenie progu = przegląd wyjątku Z2 (stan runtime w `PlayerStats`).

---

## Z8. Zachowanie przeciwników

**Zasada:**
1. Zachowanie wroga to jawne stany. Każdy stan ma zachowanie, warunki wejścia i wyjścia zgodne ze specyfikacją `GDD-AI`.
2. Zmiana stanu odbywa się tylko przez jedną metodę przejścia (obecnie `EnterState()`), nigdy przez bezpośrednie przypisanie `state = …`.
3. Wartości zachowania występujące w specyfikacji jako parametry (zasięgi, czasy, prędkości, obrażenia) są w `EnemyData`. Stałe techniczne są nazwanymi stałymi z komentarzem.
4. Różnice między typami wrogów wynikają z danych i z dodatkowych stanów lub zachowań przypisanych do typu, nie z warunków `if (enemyType == …)` we wspólnym kodzie.
5. AI oddziałuje na gracza wyłącznie przez interfejsy (Z4); z gracza czyta tylko pozycję i to, co udostępniają interfejsy.
6. **Próg przebudowy.** Wniosek o docelową strukturę AI (`GDD-ARC`: stany jako klasy, maszyna hierarchiczna, pakiet Behavior lub inna) jest obowiązkowy przed implementacją któregokolwiek z:
   - pierwszego typu wroga z własnym, unikalnym stanem,
   - faz walki,
   - zachowania grupowego.

   Do tego czasu obowiązuje obecny `enum` + `switch`.

**Dlaczego:** jawne stany z jedną drogą przejścia da się opisać w specyfikacji i przetestować. Warunki na typie rosną z każdym wrogiem i psują zachowanie pozostałych. Próg wymusza wybór struktury, gdy wiadomo, czego wymagają konkretni wrogowie.

**Dozwolone:** trafiony wróg w Patrol/Alert przechodzi do pościgu przez `EnterState(State.Chase)`.

**Niedozwolone:** `if (data.enemyType == EnemyType.Leaper) Teleport();` w `UpdateChase()`. `state = State.Attack` z pominięciem `EnterState()`.

**Sprawdzanie:** automatycznie — przypisania `state =` poza metodą przejścia i porównania `enemyType ==` w kodzie AI; reszta Inspektor.

---

## Z9. Audio

**Zasada:**
1. Cały dźwięk przez FMOD. `AudioSource` i `AudioClip` nie są używane.
2. Referencje do zdarzeń FMOD wyłącznie z `FMODEvents` (przez `GameManager.FMODEvents`). Bez ścieżek `"event:/…"` w kodzie i bez `Resources.Load`.
3. Dźwięki reakcji (trafienie, krytyk, śmierć, podniesienie, level up, zapis) odtwarza odbiorca zdarzeń EventBus, nie kod, który zdarzenie wywołał.
4. Dźwięki ciągłe lub przypięte do obiektu: `StudioEventEmitter` na obiekcie albo Animation Event.
5. Każda instancja z `CreateInstance` / `CreateAndAttach` ma właściciela, który ją zwalnia najpóźniej w `OnDestroy`.
6. Każde użyte zdarzenie FMOD ma wpis w `Documentation/Assets/Audio.md` (`GDD-AUD`) z identyfikatorem zgodnym z `FMODEvents`.

**Dlaczego:** jeden kanał audio = jedno miejsce konfiguracji i diagnozy. Reakcje przez zdarzenia (Z1) pozwalają zmieniać dźwięk bez dotykania logiki. Zwalnianie instancji zapobiega wyciekom.

**Dozwolone:** odbiorca subskrybuje `OnCriticalHit` i woła `AudioManager.Instance.PlayOneShot(FMODEvents.critHit, e.worldPosition)`.

**Niedozwolone:** `PlayerCombat.ApplyDamage()` woła `AudioManager`. `RuntimeManager.PlayOneShot("event:/SFX/Hit")`.

**Sprawdzanie:** automatycznie — `AudioSource`, `AudioClip`, `Resources.Load`, `"event:/` w kodzie; reszta Inspektor.

---

## Z10. Determinizm i pomiary

**Zasada:**
1. Kod rozgrywki nie korzysta bezpośrednio z `UnityEngine.Random` ani z `new System.Random()` bez seeda. Losowanie przez źródło losowości z seedem, z osobnymi strumieniami dla niezależnych obszarów (spawn, AI każdego wroga, walka, loot).
2. Scenariusz pomiaru (scena, liczba wrogów, seed, nagranie inputu, warmup, czas) jest opisany w `GDD-OPT-001`. Przebiegi są porównywalne tylko wtedy, gdy metryki obciążenia (liczba aktywnych wrogów, rozkład stanów AI, trasa gracza) są zgodne w tolerancji określonej w `GDD-OPT-001`. Przebieg spoza tolerancji jest odrzucany, nie uśredniany.
3. Podczas zbierania danych nic poza grą nie generuje pracy: overlay ukryty, brak logów w pętli, brak alokacji w ścieżce pomiaru.
4. Porównywane są tylko przebiegi z tą samą wersją metodologii, konfiguracją buildu, wersją silnika (Z11) i sprzętem. Pomiary rozstrzygające — w buildzie, nie w edytorze.
5. Każda zmiana systemu pomiarowego = nowa wersja `GDD-OPT-001`. Wyników z różnych wersji metodologii się nie porównuje.

**Dlaczego:** różnica „przed/po” ma wynikać wyłącznie z optymalizacji. Osobne strumienie losowości chronią powtarzalność przed zmianami w niepowiązanym kodzie i pozwalają odtwarzać bugi z seeda.

**Dozwolone:** patrol wroga losuje punkt ze swojego strumienia, zainicjowanego seedem scenariusza i indeksem wroga.

**Niedozwolone:** `Random.value` w `RollCrit()`. Benchmark z widocznym overlayem. Porównanie wyniku z edytora z wynikiem z buildu.

**Sprawdzanie:** automatycznie — `UnityEngine.Random`, `Random.value`, `Random.Range`, `new System.Random()` poza źródłem losowości; punkty 2–5 przy każdym pomiarze.

**Powiązanie:** forma źródła losowości zapada w planie technicznym; jeśli jest usługą globalną — wchodzi do rejestru singletonów przez wniosek (Z5).

---

## Z11. Stos technologiczny

**Zasada:**
1. Obowiązująca wersja silnika: `ProjectSettings/ProjectVersion.txt` (obecnie `6000.4.3f1`). Wersje pakietów: `Packages/manifest.json` i `packages-lock.json`.
2. Aktualizacja w obrębie linii (np. `6000.4.3` → `6000.4.6`) — wniosek z listą zmian dotyczących projektu. Zmiana linii — wniosek z planem migracji.
3. Nigdy w trakcie serii pomiarów. Po aktualizacji pomiary bazowe od nowa, bez porównań ze starymi (Z10.4).
4. Nowy pakiet — wniosek (po co, alternatywy, czy da się bez). Pakiety z git przypięte do tagu lub commita, nigdy do gałęzi.
5. Agenci nie edytują `ProjectVersion.txt`, `manifest.json` ani `packages-lock.json` — także przez MCP.
6. Wtyczki i skille Claude Code oraz narzędzia MCP — tylko z rejestru (`zasady-rejestr.md`). Nowa wtyczka, skill lub odblokowanie narzędzia MCP — wniosek. Każdy subagent ma w definicji **jawną listę narzędzi** i nie dziedziczy wszystkich.

**Dlaczego:** zmiana silnika lub pakietu może zmienić zachowanie i wydajność całej gry naraz i unieważnia pomiary. Przypięte wersje = powtarzalny build.

**Dozwolone:** po zatwierdzonym wniosku autor aktualizuje pakiet w Package Managerze; Kronikarz odnotowuje zmianę.

**Niedozwolone:** agent dopisuje pakiet do `manifest.json`. Pakiet z git przypięty do `#main`.

**Sprawdzanie:** automatycznie — odmowa zapisu tych plików w uprawnieniach.

---

## Z12. Strefy chronione

**Zasada:**
1. Kod i assety zewnętrzne (z rejestru) nie są modyfikowane w żadnych okolicznościach. Potrzebną zmianę robi się na kopii w `Assets/Ratocalypse/`.
2. Strefy chronione (własne pliki z rejestru) można czytać zawsze. Zmiana wymaga planu technicznego, który wprost wymienia ochronę i jej powód, oraz zgody autora na ten plan. Ogólna zgoda na zadanie nie wystarcza.
3. Każda pozycja rejestru ma powód i warunek wygaśnięcia. Ochrona bez warunku wygaśnięcia — tylko dla paczek zewnętrznych.
4. Ochrona może być czasowa (np. na czas serii pomiarów).

**Dlaczego:** paczki zewnętrzne nadpisuje każda aktualizacja. Własne strefy chronione kosztowały wiele iteracji albo są wrażliwe na drobne zmiany. Wymóg wymienienia ochrony w planie zapobiega zmianom „przy okazji”. Warunek wygaśnięcia zapobiega rozrostowi listy.

**Sprawdzanie:** automatycznie — paczki zewnętrzne: odmowa zapisu; strefy chronione: tryb „pytaj”.

---

## Z13. Zmiana zasad i wnioski

**Zasada:**
1. Agent, który uzna, że zasada przeszkadza w zadaniu, nie łamie jej, nie obchodzi i nie interpretuje twórczo. Zatrzymuje tę część pracy i zgłasza wniosek.
2. Format wniosku:

```
WNIOSEK: <zmiana zasady | wyjątek | nowy singleton | nowy pakiet | strefa chroniona | struktura dokumentacji>
Dotyczy: <ID zasady / pozycja rejestru>
Problem: <konkretnie, z odwołaniem do pliku lub specyfikacji>
Opcje:
  A. <zostawić zasadę i rozwiązać inaczej — zawsze pierwsza>
  B. <wyjątek / zmiana>
Koszt i skutki: <pliki, inne zasady, specyfikacje, pomiary>
Rekomendacja: <opcja + jedno zdanie dlaczego>
```

3. Wniosek przedstawia autorowi Prowadzący. Subagent zwraca go jako wynik swojej pracy.
4. Każda decyzja, także odrzucenie, trafia do `.claude/zasady/decyzje.md` (data, typ, decyzja, uzasadnienie). Przed zgłoszeniem wniosku agent sprawdza rejestr decyzji i nie składa ponownie odrzuconego wniosku bez nowych argumentów.
5. Zatwierdzoną zmianę wprowadza się do właściwego pliku (treść zasady, skrót poziomu 0, rejestr). Pliki zasad edytuje się tylko w tym celu i tylko w zakresie decyzji.
6. Zasady obowiązują od zatwierdzenia. Kod niezgodny z nową zasadą = zadanie na liście, nie natychmiastowe zmiany poza cyklem.

**Dlaczego:** zasady mają sens tylko wtedy, gdy są stabilne i jednakowo stosowane. Opcja „zostaw zasadę” jako pierwsza wymusza szukanie rozwiązania w ramach zasad. Rejestr decyzji chroni przed powtarzaniem tych samych wniosków.

**Sprawdzanie:** pliki zasad w trybie „pytaj” w uprawnieniach.

---

## Z14. Sterowanie

**Zasada:**
1. Cały input rozgrywki i UI jest zdefiniowany w jednym pliku akcji Input Systemu (`Assets/Ratocalypse/Input/`), z mapami akcji (np. `Gameplay`, `UI`).
2. Kod rozgrywki i UI reaguje na akcje (`SelectPoint`, `Dodge`, `ToggleInventory`, `ToggleSkillTree`, `Zoom`…), nie na `Keyboard.current`, `Mouse.current`, `KeyCode`.
3. Warstwa inputu tłumaczy akcję na komendę logiki (`MoveToWorldPos`, `TryDodge(direction)`, `Interact(target)`). Logika nie wie, co wywołało komendę.
4. Przypisanie klawiszy istnieje tylko w pliku akcji. Tabela sterowania w `GameOverview.md` jest z niego przepisana.
5. Narzędzia deweloperskie (benchmark, monitor wydajności) mają osobną mapę akcji `Debug`.

**Dlaczego:** wersja mobilna (dopuszczona w przyszłości), pad czy zmiana klawiszy to nowa mapa przypisań, nie przepisywanie kontrolerów. Komendy niezależne od inputu działają też dla benchmarku i testów.

**Dozwolone:** akcja `Dodge` wywołuje `controller.TryDodge(kierunek)`; benchmark woła tę samą komendę bez akcji.

**Niedozwolone:** `if (Keyboard.current.iKey.wasPressedThisFrame)` w `InventoryUI`. Logika uniku sprawdzająca, czy wciśnięty jest Shift.

**Sprawdzanie:** automatycznie — `Keyboard.current`, `Mouse.current`, `Touchscreen.current`, `Input.Get*` w `Scripts/` poza warstwą inputu.
