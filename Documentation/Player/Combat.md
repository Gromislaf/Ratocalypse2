# System walki gracza

**Skrypt:** `Assets/Ratocalypse/Scripts/Player/PlayerCombat.cs`  

---

## CMB-001 Cykl ataku

**Warunki aktywacji:** `canAttack == true && IsInAttackRange == true && CurrentTarget != null`

**Przebieg działania:**
1. `TriggerAttack()` — ustawia `canAttack = false`, ustawia `animator.SetFloat("AttackSpeed", TotalAttackSpeed)`, odpala trigger `"Attack"`
2. **Animation Event `OnAttackHit()`** — w klatce uderzenia: oblicza obrażenia, wywołuje `target.TakeDamage()`
3. **Animation Event `OnAttackEnd()`** — koniec animacji: `canAttack = true`, `ResetTrigger("Attack")`

**Parametry animatora:**

| Parametr | Typ | Opis |
|---|---|---|
| `Attack` | trigger | Odpala animację ataku |
| `AttackSpeed` | float | Mnożnik prędkości animacji = `TotalAttackSpeed` |

---

## CMB-002 Obliczanie obrażeń

**Wzór:**
```
damage = TotalDamage   // baseDamage + damagePerLevel*(lvl-1) + bonusDamage + skillBonusDamage
if (Random.value < TotalCritChance):
    damage *= critMultiplier   // domyślnie 2x
damage = Round(damage)
target.TakeDamage(damage, isCritical, knockbackDirection)
```

**Parametry:**

| Parametr | Wartość bazowa | Źródło |
|---|---|---|
| `baseDamage` | 10 | PlayerStats SO |
| `damagePerLevel` | 2 | PlayerStats SO |
| `baseCritChance` | 5% | PlayerStats SO |
| `critMultiplier` | 2x | PlayerStats SO |

---

## CMB-003 Fallback timer

**Opis:** Gdy brak Animatora lub Animation Events (np. testy), timer liczy `1 / TotalAttackSpeed` s i resetuje `canAttack`.

**Warunki aktywacji:** Animator == null lub brak skonfigurowanych Animation Events.

---

## CMB-004 Interfejsy walki

| Interfejs | Metody | Implementuje |
|---|---|---|
| `IDamageable` | `IsAlive`, `TakeDamage()` | PlayerHealthComponent, EnemyAI |
| `IHealable` | `Heal()` | PlayerHealthComponent |
| `IStatusEffectable` | `ApplyStatusEffect()` | ⏳ Planowane |

**Przypadek brzegowy:** `ResetTrigger("Attack")` wywoływany zarówno w `OnAttackEnd` jak i `TickFallback` — zapobiega zakleszczeniu triggera w Any State.

**Interakcje:** PlayerCombat czyta `CurrentTarget` i `IsInAttackRange` z PlayerController (RequireComponent — ten sam GO).
