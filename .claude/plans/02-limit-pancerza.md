# Plan: #2 — limit pancerza 75%

Rozmiar: S | Ścieżka: bugfix | Zasada: Z7.1
Specyfikacje: GDD-CMB-006 (`Documentation/Player/Combat.md`), `Documentation/Player/Stats.md`
Gałąź: `feature/02-limit-pancerza`

---

## Zmiany w kodzie

### Plik 1: `Assets/Ratocalypse/Scripts/Player/PlayerStats.cs` (linia 91)

Właściwość `TotalArmor`:
```
// przed
public float TotalArmor => Mathf.Clamp01(baseArmor + bonusArmor + skillBonusArmor);
// po
public float TotalArmor => Mathf.Clamp(baseArmor + bonusArmor + skillBonusArmor, 0f, 0.75f);
```

Uzasadnienie: Z7.1 — jedno miejsce obliczania statystyki z limitem ze specyfikacji.
`TotalArmor` jest jedynym punktem użycia (`PlayerHealthComponent.cs:43`).
`baseArmor` ma już `[Range(0f, 0.75f)]`; bonusy są `[NonSerialized]` — SO nie wymaga zmian.

### Plik 2: `Assets/Ratocalypse/Tests/EditMode/DamageInfoTests.cs` (po linii 274)

Nowe testy T11–T13 w klasie `PlayerHealthComponentTests`:

| Test | Konfiguracja | Oczekiwane |
|---|---|---|
| T11 | `baseArmor=0.5`, `skillBonusArmor=1.0`, `value=100` | `TotalArmor=0.75`, `reduced=25` |
| T12 | `baseArmor=0.75`, reszta=0, `value=100` | `TotalArmor=0.75`, `reduced=25` (dokładnie na granicy) |
| T13 | `baseArmor=1.5f` (przekroczenie), `value=100` | `TotalArmor=0.75`, `reduced=25` (Clamp tnie) |

Regresje T8–T10 (istniejące, `baseArmor=0.5`) — bez zmian.

---

## Zmiany w dokumentacji

### Plik 3: `Documentation/Player/Combat.md` — GDD-CMB-006, bump 0.1 → 0.2

- linia 247: usunąć adnotację „`TotalArmor` nie ma limitu górnego — błąd śledzony jako zadanie #2"
- linia 256: usunąć punkt „Limit górny `TotalArmor ≤ 0,75` nie jest wymuszany — błąd zadania #2"
- linia 263: zmienić zakres `0–1 (brak Clamp, błąd zadania #2)` → `0–0,75`
- linia 278: zastąpić punkt o `TotalArmor > 1` opisem: „Suma pancerzy > 0,75 → `TotalArmor = 0,75` (`Mathf.Clamp`)"
- historia zmian: wiersz `0.2 | Wymuszenie limitu TotalArmor ≤ 0,75 (zadanie #2). Poprzednio Clamp01 umożliwiał 100% redukcję wbrew specyfikacji.`

### Plik 4: `Documentation/Player/Stats.md` — bez zmian

Linia 34 jest już poprawna (`Clamp(..., 0, 0.75)`). Spec była prawidłowa, bug był w kodzie.

---

## Ryzyka

- **R1 (niskie):** Istniejące testy T8 (`baseArmor=0.5`) mieszczą się w limicie — brak zepsutych testów.
- **R2 (niskie):** Asset `PlayerStats.asset` z `baseArmor > 0.75` — `Mathf.Clamp` w runtime zabezpiecza, asset nie wymaga edycji.
- **R3 (minimalne):** Brak wpływu na EventBus ani kontrakt `IDamageable`.

---

## Decyzje techniczne

- `Mathf.Clamp(..., 0f, 0.75f)` — czytelniejsze, spójne z notacją Stats.md linia 34.
- Stała `0.75f` wpisana wprost (nie `const MaxArmorCap`) — spec jest źródłem prawdy.
- T11–T13 w tej samej klasie `PlayerHealthComponentTests`, nie nowy plik.
- Stats.md bez historii zmian — spec była poprawna, dług D7 osobny.

---

## Dziennik bramek

| Data | Etap | Zdarzenie / decyzja |
|---|---|---|
| 2026-11-01 | 0 | Rozmiar S zatwierdzony |
| 2026-11-01 | 0 | Gałąź `feature/02-limit-pancerza` z HEAD `44bb1a0` |
| 2026-11-01 | 2 | Plan skrócony zwrócony przez Architekta; autor zatwierdził (ok) |
| 2026-11-01 | 3a | Programista: `PlayerStats.cs:91` `Clamp01` → `Clamp(..., 0f, 0.75f)` |
| 2026-11-01 | 4 | Inspektor PASS; kompilacja czysta |
| 2026-11-01 | 4 | Tester T11 PASS, T12 PASS; T13 pominięty (minimum-1 poza zakresem #2) |
| 2026-11-01 | 5 | Kronikarz: GDD-CMB-006 v0.2 — usunięte adnotacje o błędzie |
| 2026-11-01 | 6+7 | Commit `c4dd82e`, merge `f3b8463` do `main` |
| 2026-11-01 | 8 | Push `origin/main` |
