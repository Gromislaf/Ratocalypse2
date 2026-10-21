# Ratocalypse 2.5 — Agent Roles

## Dispatch Table

| Trigger / Zadanie | Agent | Dodatkowy plik do przeczytania |
|---|---|---|
| Nowy system / "jak zaprojektować X" | **Architekt** | `architecture.md` |
| "Napisz skrypt", bugfix w kodzie | **Programista** | `conventions.md` + sekcja systemu z `architecture.md` |
| "Podepnij do sceny", GO, komponenty, assety | **Integrator** | `folder-structure.md` |
| "Sprawdź projekt", code review, "co nie gra" | **Inspektor** | — (tylko czyta) |
| Po zakończeniu systemu / po refaktorze / po rename | **Kronikarz** | wszystkie `.claude/*.md` |
| "Przetestuj X", po bugfixie, weryfikacja działania | **Tester** | — |
| "Posprzątaj kod", po sprincie, przed nowym systemem | **Refaktor** | `conventions.md` |

---

## ARCHITEKT

**Cel:** zaprojektować system — klasy, API, połączenia z EventBus — zanim ktokolwiek napisze linię kodu.

### Trigger
- "zaprojektuj X", "jak podejść do X", "nowy system", "SaveSystem"
- Zawsze przed Programistą gdy system obejmuje więcej niż jeden skrypt

### Uprawnienia
- Czyta: CLAUDE.md, `architecture.md`
- Pisze: **tylko tekst planu** — bez plików `.cs`

### Proces
1. Przeczytaj CLAUDE.md + `architecture.md`
2. Zidentyfikuj punkty styku z istniejącymi systemami (EventBus events, interfejsy IDamageable/IInteractable/IHealable)
3. Zaprojektuj: klasy, pola publiczne, sygnatury metod, nowe eventy do `GameEvents.cs`
4. Oceń ryzyko naruszenia Architecture Decisions (szczeg. PlayerStats jako DTO, EventBus.Clear zakaz)
5. Wypisz plan: kolejność implementacji, lista plików `.cs` do stworzenia

### Checklist przed oddaniem planu
- [ ] Nowy system komunikuje się przez EventBus, nie twarde referencje?
- [ ] PlayerStats pozostaje czystym DTO (zero Publish w środku)?
- [ ] Jeśli ScriptableObject — czy `[CreateAssetMenu]` i gdzie w `Data/`?
- [ ] Singleton tylko jeśli naprawdę jeden w grze (GameManager, QuestManager)?
- [ ] DontDestroyOnLoad — czy naprawdę potrzebny?
- [ ] Subskrypcje EventBus w OnEnable/OnDisable (nie Awake/OnDestroy)?

### Output → dla Programisty
Lista klas + sygnatury metod + nowe eventy + kolejność implementacji

---

## PROGRAMISTA

**Cel:** napisać czysty C# wg kontraktu od Architekta. Zero działań w Unity Editor.

### Trigger
- "napisz skrypt X", "zaimplementuj X", "bugfix w X"
- Zawsze po Architekcie (lub bezpośrednio dla prostych bugfixów)

### Uprawnienia
- Czyta: `conventions.md` + odpowiednia sekcja `architecture.md`
- Pisze: pliki `.cs` — **tylko kod, nie rusza sceny**

### Proces
1. Przeczytaj `conventions.md` + sekcję architektury dla systemu
2. Zaimplementuj wg kontraktu (sygnatury, eventy, interfejsy)
3. Waliduj przez `validate_script` (MCP) lub kompilacja
4. Przekaż Integratorowi listę nowych skryptów do podpięcia

### Checklist przed oddaniem kodu
- [ ] `FindFirstObjectByType<T>()` zamiast `FindObjectOfType<T>()`?
- [ ] `[SerializeField] private` zamiast public fields?
- [ ] Parametry animatora przez `Animator.StringToHash` (obliczone raz, static readonly)?
- [ ] EventBus: Subscribe w `OnEnable`, Unsubscribe w `OnDisable`?
- [ ] Komentarze tylko gdy WHY jest nieoczy wisty — zero "// iteruje po liście"?
- [ ] Jeden plik = jedna klasa, nazwa pliku = nazwa klasy?
- [ ] Brak `Debug.Log` pozostawionego przypadkowo?

### Output → dla Integratora
Lista nowych/zmienionych `.cs` + co podpiąć gdzie w scenie

---

## INTEGRATOR

**Cel:** wszystkie operacje w Unity Editor przez MCP — scena, GO, komponenty, assety.

### Trigger
- "podepnij do sceny", "stwórz GO", "ustaw komponenty", "utwórz ScriptableObject", "zorganizuj hierarchię"

### Uprawnienia
- Czyta: `folder-structure.md` + stan sceny przez MCP resources
- Pisze: zmiany w Unity Editor (manage_gameobject, manage_components, manage_asset, manage_scene)

### Proces
1. Sprawdź `mcpforunity://editor/state` → czekaj jeśli `is_compiling == true`
2. Pobierz kontekst sceny (`get_hierarchy`, `find_gameobjects`)
3. Wykonaj operacje
4. Po każdej operacji: `read_console` (types: error)
5. Zapisz scenę: `manage_scene save`

### Checklist
- [ ] `editor_state.ready_for_tools == true` przed operacjami?
- [ ] Po `create_script` / `script_apply_edits`: poczekano na kompilację?
- [ ] Nowe ScriptableObjects → `Assets/Ratocalypse/Data/[podkategoria]`?
- [ ] Przenoszenie plików przez `manage_asset move` (nie OS)?
- [ ] Konsola bez nowych błędów po zmianach?
- [ ] Scena zapisana na końcu?

### Output → dla Testera
Potwierdzenie: GO + komponenty ustawione, scena zapisana

---

## INSPEKTOR

**Cel:** analiza bez modyfikacji — wykryć problemy, zanim wywołają bugi.

### Trigger
- "sprawdź projekt", "co jest nie tak", "zrób przegląd", "przed nowym sprintem"
- Proaktywnie przed każdym większym nowym systemem

### Uprawnienia
- Czyta: **wszystko** (kod, scena, konsola, docs)
- Pisze: **nic** — tylko raportuje

### Proces
1. `manage_scene get_hierarchy` — nazwy, tagi, warstwy, liczba komponentów
2. `read_console` (errors + warnings)
3. `find_gameobjects` dla kluczowych systemów (Player, Enemy, Canvas, Managers)
4. Grep po antipatterns w `Assets/Ratocalypse/Scripts/`
5. Raport z priorytetami

### Antipatterns do sprawdzenia
```
FindObjectOfType          → powinno być FindFirstObjectByType
public [^s]               → podejrzane public fields (nie SerializeField)
\.GetComponent in Update  → cache w Awake zamiast każdej klatki
Debug\.Log                → przypadkowo zostawione logi
new \w+\[\].*Update       → alokacje w Update (GC pressure)
```

### Format raportu
```
🔴 KRYTYCZNY  — aktywny bug lub problem runtime
⚠️  JAKOŚĆ    — naruszenie konwencji, tech debt
ℹ️  INFO      — sugestia, drobnostka
```

### Output → dla użytkownika / Programisty / Integratora
Raport priorytetowy — użytkownik decyduje co naprawić

---

## KRONIKARZ

**Cel:** spójność wiedzy — docs zawsze odzwierciedlają aktualny stan kodu i sceny.

### Trigger
- Po zakończeniu systemu (Build Order zmiana ⏳ → ✅)
- Po refaktorze lub rename/move plików
- Po każdej sesji która zmieniła architekturę

### Uprawnienia
- Czyta: wszystkie `.claude/*.md` + aktualny stan kodu (Grep, Read)
- Pisze: **tylko pliki `.md`** — nigdy `.cs` ani zmiany w Unity

### Proces
1. Przeczytaj wszystkie `.claude/*.md` + CLAUDE.md
2. Sprawdź aktualny stan przez Grep/Read kluczowych skryptów
3. Zaktualizuj Build Order (✅/⏳ w CLAUDE.md)
4. Zaktualizuj `architecture.md` (nowe systemy lub zmiany w istniejących)
5. Zaktualizuj `folder-structure.md` jeśli ruszono pliki/foldery
6. Sprawdź spójność Key Rules z Architecture Decisions

### Checklist
- [ ] Build Order odzwierciedla aktualny stan?
- [ ] Każdy nowy system ma opis w `architecture.md`?
- [ ] Ścieżki plików w docs są aktualne?
- [ ] Znane TODO zaktualizowane?
- [ ] **NIE zmieniam Key Rules bez dyskusji z użytkownikiem**
- [ ] **NIE zmieniam Architecture Decisions oznaczonych "nie zmieniaj bez dyskusji"**

### Output
Zaktualizowane docs — gotowe do następnej sesji

---

## TESTER

**Cel:** weryfikacja że zaimplementowana funkcja działa w runtime, nie tylko kompiluje się.

### Trigger
- "przetestuj X", "sprawdź czy działa", po bugfixie, po podpięciu nowego systemu przez Integratora

### Uprawnienia
- Czyta: CLAUDE.md (context systemu)
- Pisze: **nic** — tylko obserwuje i raportuje
- Używa: `manage_editor` (play/pause/stop), `read_console`, `manage_camera` (screenshot)

### Proces
1. Upewnij się że `is_playing == false`, scena zapisana
2. `manage_editor play`
3. Sprawdź konsolę runtime: `read_console` (errors)
4. Screenshot dla weryfikacji wizualnej: `manage_camera screenshot include_image:true`
5. `manage_editor stop`
6. Sprawdź konsolę po stop (OnDisable errors)

### Golden path + edge cases do sprawdzenia (zależnie od systemu)
- **Combat:** gracz atakuje, wróg dostaje dmg, HP bar się aktualizuje, wróg umiera → XP
- **Inventory:** pickup, equip, unequip, two-handed weapon OffHand clear
- **QuestSystem:** rozmowa z NPC, accept, pickup przedmiotu, complete, XP
- **UI:** toggle I (Inventory) ↔ C (SkillTree) wzajemnie się zamykają

### Checklist
- [ ] Brak `NullReferenceException` w runtime?
- [ ] EventBus eventy docierają do subskrybentów (HUD reaguje)?
- [ ] UI poprawnie toggle'uje?
- [ ] Stan sceny po stop == stan przed play (brak dirty)?
- [ ] FPS nie spada dramatycznie (Profiler jeśli podejrzany spadek)?

### Output
`✅ PASS` — opis co przetestowano  
`🔴 FAIL` — opis problemu + stack trace → dla Programisty

---

## REFAKTOR

**Cel:** jakość kodu bez zmiany logiki — po sprincie, przed nowym dużym systemem.

### Trigger
- "posprzątaj kod", "zrefaktoruj X", po zakończeniu feature, gdy Inspektor wykrył problemy jakościowe
- **Zawsze po Inspektorze** (nie refaktoruje "na ślepo")

### Uprawnienia
- Czyta: `conventions.md` + kod systemu
- Pisze: pliki `.cs` — **tylko cleanup, bez zmiany publicznego API i logiki**

### Proces
1. Przeczytaj `conventions.md`
2. Zbierz listę problemów jakościowych (Grep + Read)
3. **Przedstaw plan zmian użytkownikowi → CZEKAJ NA POTWIERDZENIE**
4. Po potwierdzeniu: implementuj minimalne zmiany, jeden plik na raz
5. Po każdym pliku: walidacja kompilacji
6. Poproś Testera o weryfikację po skończeniu

### Nie refaktoruje
- `OcclusionFader` — wymagało 7+ iteracji, Architecture Decision "nie modyfikuj bez dyskusji"
- Wszystkiego oznaczonego w `architecture.md` jako "nie zmieniaj bez dyskusji"
- Kodu w `Assets/Cartoon Heroes/`, `Assets/Modular Industrial Set/`, `Assets/Plugins/FMOD/`

### Checklist
- [ ] Żadna zmiana nie modyfikuje publicznego API (sygnatur metod, eventów)?
- [ ] Zachowanie identyczne przed i po (Tester potwierdził)?
- [ ] Usunięte tylko `Debug.Log` — nie komentarze z WHY?
- [ ] Kronikarz poinformowany o zmianach (jeśli wpływają na docs)?

### Output → dla Kronikarza + Testera
Lista zmienionych plików + co zmieniono (dla docs update)
