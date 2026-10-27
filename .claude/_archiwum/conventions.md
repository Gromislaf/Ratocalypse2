# Ratocalypse 2.5 — Coding Conventions & Unity 6 Migration

## Konwencje kodu C#

| Element | Styl | Przykład |
|---|---|---|
| Klasy, metody publiczne | PascalCase | `PlayerHealthComponent`, `TakeDamage()` |
| Pola prywatne | camelCase | `currentHp`, `isAttacking` |
| Pola serializowane | `[SerializeField] private` | `[SerializeField] private PlayerStats stats;` |
| Stałe hashing | `static readonly int` | `static readonly int HashSpeed = Animator.StringToHash("Speed");` |
| Pliki | Jeden plik = jedna klasa, nazwa pliku = nazwa klasy | `PlayerController.cs` |
| Namespace | `Ratocalypse` (opcjonalne, utrzymuj spójność) | |

### Czego unikać
- `public` fields — używaj properties lub `[SerializeField] private`
- Komentarze "co robi kod" — nazwy metod już to mówią. Komentarz tylko gdy WHY jest nieoczywiste (constraint, workaround, bug)
- `GetComponent<T>()` w `Update()` — cache w `Awake()`
- Alokacje w `Update()` — unikaj `new`, LINQ, string concatenation każdą klatkę
- `Debug.Log` zostawiony w kodzie produkcyjnym

### EventBus — wzorzec subskrypcji
```csharp
private void OnEnable()  => EventBus.Subscribe<OnSomeEvent>(Handler);
private void OnDisable() => EventBus.Unsubscribe<OnSomeEvent>(Handler);
// NIE: Awake/Start + OnDestroy — DontDestroyOnLoad obiekty wyciekają subskrypcje
```

---

## Unity 6 API — zmiany względem 2022.3

### FindObject
```csharp
// STARE (2022) — deprecated
FindObjectOfType<PlayerController>()

// NOWE (Unity 6)
FindFirstObjectByType<PlayerController>()          // szybsze, dowolna kolejność
FindAnyObjectByType<PlayerController>()            // najszybsze gdy kolejność nieważna
FindFirstObjectByType<T>(FindObjectsInactive.Include) // gdy szukasz nieaktywnego
```

### Cinemachine 3.x
```csharp
// STARE
CinemachineVirtualCamera  →  CinemachineCamera
// Brain na Main Camera pozostaje: CinemachineBrain
```

### Input System
```csharp
// NIE używaj
Input.GetKey(KeyCode.Space)

// Używaj (New Input System)
Keyboard.current.spaceKey.wasPressedThisFrame
Mouse.current.leftButton.wasPressedThisFrame
Mouse.current.position.ReadValue()
```

### Physics (sprawdź deprecation warnings)
```csharp
// Sprawdź czy używasz aktualnego API:
Physics2D.OverlapCircleAll  // może być deprecated — sprawdź w konsoli
```

---

## FMOD — zasady

- **Cały audio przez FMOD** — nigdy `AudioSource` / `AudioClip`
- Eventy przez `FMODUnity.RuntimeManager` lub `StudioEventEmitter`
- `FMODEvents` dostępny przez `GameManager.FMODEvents` (przypisany w Inspektorze) — nie przez `Resources.Load`
- Nie modyfikuj plików w `Assets/Plugins/FMOD/`

---

## URP — zasady

- Shadery muszą być zgodne z URP — nie używaj Built-in shaderów
- Renderer assets w `Assets/Settings/` (osobne dla PC i Mobile)
- Post-processing przez URP Volume system
