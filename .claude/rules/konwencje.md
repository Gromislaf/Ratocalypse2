---
paths:
  - "**/*.cs"
---
# Konwencje kodu — Ratocalypse 2.5

> Konwencje obowiązują w nowym i zmienianym kodzie. Zmienia je autor bez procedury wniosku (to nie są żelazne zasady),
> ale każda zmiana trafia do `.claude/zasady/decyzje.md`.
> Przy sprzeczności z `.claude/rules/zasady-techniczne.md` wygrywa zasada techniczna.
> Istniejących plików nie dostosowuje się masowo — tylko przy zmianie lub refaktorze danego pliku.

---

## 1. Nazewnictwo

| Element | Styl | Przykład |
|---|---|---|
| Klasy, struktury, enumy, interfejsy | PascalCase; interfejsy z `I` | `PlayerHealthComponent`, `IDamageable` |
| Metody, właściwości, zdarzenia | PascalCase | `TakeDamage()`, `IsAlive`, `OnEnemyDied` |
| Pola prywatne (także `[SerializeField]`) | `_camelCase` | `_canAttack`, `[SerializeField] private PlayerStats _stats;` |
| Parametry, zmienne lokalne | camelCase | `rawDamage`, `targetSlot` |
| Stałe i hashe animatora | `static readonly` / `const`, PascalCase | `static readonly int HashSpeed = Animator.StringToHash("Speed");` |
| Pliki | jeden plik = jedna klasa; nazwa pliku = nazwa klasy | `PlayerController.cs` |

- Identyfikatory po angielsku. Komentarze po polsku.
- Komentarz tylko gdy *dlaczego* jest nieoczywiste (ograniczenie, obejście, bug silnika) — nie „co robi kod”.
- **Wyjątek od „jeden plik = jedna klasa”:** struktury zdarzeń EventBus są zebrane w `Core/GameEvents.cs`. Enumy mają własne pliki.

## 2. Namespace

- Cały kod gry w `namespace Ratocalypse`.
- Wprowadzane przy zmianie pliku (nie masowo). Dodanie namespace nie psuje referencji w scenach — Unity wiąże skrypty po GUID z `.meta`.

## 3. Pola i serializacja

- `MonoBehaviour`: pola wyłącznie `[SerializeField] private` (+ właściwość, jeśli potrzebny dostęp z zewnątrz). Nigdy pola `public`.
- **Definicje ScriptableObject:**
  - istniejące klasy (`ItemData`, `EnemyData`, `QuestData`, `PlayerStats`, `FMODEvents`) zachowują pola `public` — zakaz zapisu w runtime pilnuje Z2.1;
  - **nowe** klasy definicji: `[SerializeField] private` + właściwości tylko do odczytu.
- **Zmiana nazwy pola serializowanego** wyłącznie z `[FormerlySerializedAs("staraNazwa")]` — bez tego wartości z Inspektora i assetów przepadają bez komunikatu.
- Referencje przypinane w Inspektorze sprawdzane w `Awake()`:
  ```csharp
  if (_stats == null) Debug.LogError("[PlayerCombat] PlayerStats nie przypisany!", this);
  ```

## 4. Obiekty Unity i null

- **Nie używaj `?.` ani `??` na obiektach Unity** (`Object`, `Component`, `MonoBehaviour`, `GameObject`, `ScriptableObject`). Zniszczony obiekt Unity nie jest `null` dla tych operatorów.
- Zamiast tego: `if (_animator != null) …` albo `if (_animator) …`.
- Dla zwykłych obiektów C# (`SaveData`, kolekcje, struktury) `?.` i `??` są dozwolone.

## 5. Unity 6 API

```csharp
// NIE (deprecated)            // TAK
FindObjectOfType<T>()          FindFirstObjectByType<T>()        // gdy kolejność ma znaczenie
                               FindAnyObjectByType<T>()          // najszybsze, gdy nie ma
                               FindFirstObjectByType<T>(FindObjectsInactive.Include)

CinemachineVirtualCamera       CinemachineCamera                 // Cinemachine 3.x; Brain bez zmian
```

`Find*ObjectByType` tylko w `Awake`/`Start`, nigdy w `Update`.

## 6. Wydajność w kodzie co klatkę

- `GetComponent<T>()` — cache w `Awake()`, nie w `Update()`.
- W `Update`/`FixedUpdate`/`LateUpdate` i w kodzie wołanym co klatkę: bez `new` kolekcji, LINQ, konkatenacji i interpolacji stringów, lambd z przechwytywaniem.
- Parametry animatora przez zapamiętane hashe (`Animator.StringToHash`), nie stringi.
- Fizyka: warianty `NonAlloc` tam, gdzie to możliwe.

## 7. EventBus

```csharp
private void OnEnable()  => EventBus.Subscribe<OnSomeEvent>(Handler);
private void OnDisable() => EventBus.Unsubscribe<OnSomeEvent>(Handler);
// NIE: Awake/Start + OnDestroy — obiekty DontDestroyOnLoad wyciekają subskrypcje
```

- Nigdy `EventBus.Clear()` przy zmianie sceny.
- Kiedy zdarzenie, a kiedy komenda — Z1.

## 8. Logi

- `Debug.Log` / `LogWarning` / `LogError` z prefiksem systemu: `[Benchmark]`, `[SaveSystem]`, `[EnemyAI]`.
- Tylko zdarzenia rzadkie (start/koniec procesu, zapis, błąd konfiguracji). **Nigdy** w `Update` ani w pętli.
- Logi diagnostyczne tymczasowe usuwa się przed commitem.

## 9. Kodowanie plików

- Wszystkie pliki `.cs` i `.md` w **UTF-8**.

## 10. Input, audio, losowość

Nie powtarzamy tu zasad — obowiązują:
- input: **Z14** (plik akcji Input Systemu; zakaz `Keyboard.current`/`Mouse.current` poza warstwą inputu),
- audio: **Z9** (FMOD, `FMODEvents`, reakcje przez zdarzenia),
- losowość: **Z10** (źródła z seedem; zakaz `UnityEngine.Random` w kodzie rozgrywki).

## 11. URP

- Shadery zgodne z URP — bez shaderów Built-in.
- Renderer assets w `Assets/Settings/` (osobne PC i Mobile).
- Post-processing przez URP Volume.

## 12. Testy

- Lokalizacja, struktura i Assembly Definitions — ustalane w zadaniu 26. Do tego czasu nowych testów nie tworzy się.
