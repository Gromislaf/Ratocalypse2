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
