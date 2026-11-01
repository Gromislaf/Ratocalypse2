# Plan: #1 — Wprowadzenie `DamageInfo` i dostosowanie `IDamageable`

Specyfikacje: GDD-CMB-004 (v0.2), GDD-CMB-005 (v0.1), GDD-CMB-006 (v0.1)
Ścieżka: pełny

---

## Nowe pliki

| Plik | Co |
|---|---|
| `Assets/Ratocalypse/Scripts/Core/DamageType.cs` | `public enum DamageType { Physical }` w `namespace Ratocalypse` |
| `Assets/Ratocalypse/Scripts/Core/DamageInfo.cs` | `public readonly struct DamageInfo` w `namespace Ratocalypse` — pola: `value`, `type`, `isCritical`, `knockback`, `source` (camelCase wg DP-1 B) + konstruktor |

## Pliki zmieniane

| Plik | Co się zmienia |
|---|---|
| `Core/IDamageable.cs` | `TakeDamage(float, bool, Vector3)` → `TakeDamage(DamageInfo info)` |
| `Player/PlayerHealthComponent.cs` | Nowa implementacja `IDamageable.TakeDamage(DamageInfo)` wg GDD-CMB-006 (7 kroków); usunięcie publicznej `TakeDamage(float)` |
| `Enemy/EnemyAI.cs` | (1) `IDamageable.TakeDamage(DamageInfo)` — odbiór; (2) `OnAttackHit()` — budowanie `DamageInfo` do ataku |
| `Player/PlayerCombat.cs` | `ApplyDamage()` buduje `DamageInfo` zamiast 3 luźnych argumentów |

`PlayerStats.cs` — bez zmian (jeśli autor wybiera opcję C wniosku; z opcją A: +1 linia zaokrąglenia).

## Kolejność kroków

1. Utwórz `DamageType.cs` — projekt kompiluje się
2. Utwórz `DamageInfo.cs` — projekt kompiluje się
3. Zmień `IDamageable.cs` — **projekt przestaje się kompilować** (celowy stan przejściowy)
4. Zmień `PlayerHealthComponent.cs` — implementacja wg GDD-CMB-006
5. Zmień `EnemyAI.cs` — implementacja `TakeDamage(DamageInfo)` (odbiór)
6. Zmień `EnemyAI.cs` — `OnAttackHit()` buduje `DamageInfo` (atak)
7. Zmień `PlayerCombat.cs` — `ApplyDamage()` buduje `DamageInfo`
8. Kompilacja — projekt wraca do zielonego

## Decyzje projektowe

### DP-1. Pola `DamageInfo` — **decyzja: B (camelCase)**
Publiczne pola `readonly` camelCase: `value`, `type`, `isCritical`, `knockback`, `source` — spójne ze struct-ami zdarzeń `GameEvents.cs`.

### DP-2. `namespace Ratocalypse` — **decyzja: A (tylko nowe pliki)**
Namespace tylko w `DamageInfo.cs` i `DamageType.cs`. Istniejące 4 pliki bez zmian namespace.

### DP-3. Publiczna `TakeDamage(float)` — **decyzja: A (usunąć)**
Logika trafia wprost do `IDamageable.TakeDamage(DamageInfo)`. Jedno wejście, pełna zgodność z Z4.5.

## Wniosek Z13 — zaokrąglenie pancerza — **decyzja: A (naprawić w #1)**
Dodać `reduced = Mathf.Round(reduced * 10f) / 10f;` w `PlayerStats.TakeDamage(float)` przed `Mathf.Max`. Zakres zadania #1 obejmuje `PlayerStats.cs` jako plik dotykany.

## Przypadki testowe (checklista ręczna do zadania #26)

| ID | Scenariusz | Oczekiwane |
|---|---|---|
| T1 | Wróg atakuje, `IsInvincible=false`, `TotalArmor=0` | HP spada o `attackDamage` (min 1); `OnPlayerDamaged` raz |
| T2 | Wróg atakuje, `IsInvincible=true` | 0 obrażeń; brak zdarzeń |
| T3 | Śmiertelne trafienie | `currentHp=0`; `OnPlayerDamaged` przed `OnPlayerDied` |
| T4 | `info.Source == null` | `LogWarning`; obrażenia stosowane |
| T5 | Gracz uderza krytem | `OnEnemyDamaged { isCritical=true }` + `OnCriticalHit` |
| T6 | Gracz uderza normalnie | `OnEnemyDamaged { isCritical=false }`; brak `OnCriticalHit` |
| T7 | Cel ginie przed `OnAttackHit` | `IsAlive` check; brak obrażeń |
| T8 | `TotalArmor=0.5`, obrażenia 10 | `reduced=5`; HP spada o 5 |
| T9 | `TotalArmor=0.5`, obrażenia 1 | `reduced=0.5` → min 1; HP spada o 1 |
| T10 | `value=0` | `reduced=0` → min 1; HP spada o 1 |

## Wpływ na dokumentację (dla Kronikarza)

- `Combat.md`: GDD-CMB-005/006 → `Zaimplementowana`; wpisy w historiach zmian
- `decyzje.md`: wpis o zamknięciu #1

---

## Dziennik bramek (przeniesiony ze STATUS po zamknięciu)

| Data | Etap | Zdarzenie / decyzja |
|---|---|---|
| 2026-10-26 | 0→1 | Zadanie #1 aktywne; start wywiadu specyfikacyjnego |
| 2026-10-26 | 1→2 | Specyfikacja GDD-CMB-005 i GDD-CMB-006 zatwierdzona przez autora |
| 2026-10-26 | 2→3a | Plan zatwierdzony; DP-1 B (camelCase), DP-2 A (namespace tylko nowe), DP-3 A (usunąć TakeDamage(float)), Z13 A (zaokrąglenie w PlayerStats) |
| 2026-10-27 | 3a start | Gałąź feature/01-damageinfo potwierdzona; Programista aktywny |
| 2026-10-27 | 3a→4 | Kompilacja OK (0 błędów); przekazano do Inspektora |
| 2026-10-27 | 4 Inspektor | Zero 🔴 — wszystkie Z4/Z1/Z3/Z7/DP-1/2/3/Z13A zgodne |
| 2026-10-27 | 4 Tester | Testy automatyczne zablokowane (#26); checklista ręczna gotowa; T7 naprawiony (IsAlive check w EnemyAI.OnAttackHit) |
| 2026-10-27 | 4→6 | Etap 4 zamknięty; autor przystępuje do checklisty ręcznej T1–T10 |
| 2026-10-28 | 6→7 | Checklista T1–T10 zaliczona (T4 odroczone do #26); commit `e8dbd2d`; scalenie do main wstrzymane na życzenie autora |
| 2026-10-28 | 7→5 | Powrót do etapu 5 — etap był pominięty; testy z `e8dbd2d` do weryfikacji; decyzja A/B ws. testów oczekuje autora |
| 2026-10-28 | 5→7 | Etap 5 zakończony (kronikarz, redaktor ✅, audytor ✅ — 4 🟡 naprawione); drugi commit `b677ad6` |
| 2026-10-28 | 7→8 | STATUS `7970972`; scalenie `--no-ff` do main `2649542` |
| 2026-10-28 | 8 | Push: `feature/01-damageinfo` i `main` → origin |
