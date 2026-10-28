# System walki gracza

---

## GDD-CMB-001 Cykl ataku

**Status:** Zaimplementowana
**Wersja:** 0.1

**Opis funkcjonalny:** Gracz automatycznie atakuje wybrany cel, gdy jest w zasięgu i `canAttack == true`.
**Warunki uruchomienia:** `canAttack == true && IsInAttackRange == true && CurrentTarget != null`

**Zasady działania:**
1. `TriggerAttack()` — ustawia `canAttack = false`, ustawia parametr animatora `AttackSpeed = TotalAttackSpeed`, odpala trigger `Attack`.
2. **Animation Event `OnAttackHit()`** — w klatce uderzenia: wywołuje `ApplyDamage()`, które buduje `DamageInfo` ([GDD-CMB-005]) i woła `target.TakeDamage(DamageInfo)`.
3. **Animation Event `OnAttackEnd()`** — koniec animacji: `canAttack = true`, `ResetTrigger("Attack")`.

**Ograniczenia:** Atak nie startuje, gdy `CurrentTarget == null` lub `IsInAttackRange == false`. Brak globalnego cooldownu poza `canAttack`.

**Parametry:**

| Parametr | Wartość | Jednostka | Zakres |
|---|---|---|---|
| `Attack` | trigger animatora | — | — |
| `AttackSpeed` | `TotalAttackSpeed` | — | > 0 |

**Wzory:** nie dotyczy (warunki logiczne).

**Interakcje:** [GDD-CMB-002] oblicza wartość obrażeń i buduje `DamageInfo`; [GDD-CMB-003] fallback gdy brak Animatora; [GDD-CMB-005] struktura przekazywana do celu.

**Sytuacje wyjątkowe:**
- Animator == null → `ApplyDamage()` wywoływane natychmiastowo w `TriggerAttack()`; cel otrzymuje obrażenia bez animacji.
- Cel ginie przed `OnAttackHit()` → `IDamageable.IsAlive` sprawdzane w `ApplyDamage()`; hit pominięty.

**Informacja zwrotna:** nie dotyczy (feedback po stronie odbiorcy — [GDD-CMB-006]).

**Implementacja:** `Assets/Ratocalypse/Scripts/Player/PlayerCombat.cs`

**Historia zmian:**

| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. |

---

## GDD-CMB-002 Obliczanie obrażeń wychodzących gracza

**Status:** Zaimplementowana
**Wersja:** 0.2

**Opis funkcjonalny:** Oblicza wartość obrażeń zadanych przez gracza i buduje strukturę `DamageInfo`.
**Warunki uruchomienia:** Wywołanie `ApplyDamage()` w `PlayerCombat` — z Animation Event lub z fallback timera ([GDD-CMB-003]).

**Zasady działania:**
1. Pobierz `damage = TotalDamage` (`baseDamage + damagePerLevel × (level − 1) + bonusDamage + skillBonusDamage`).
2. Rzuć krytem: `isCritical = RollCrit()` — `true` z prawdopodobieństwem `TotalCritChance`.
3. Jeśli `isCritical`: `damage *= critMultiplier`.
4. Zaokrąglij: `damage = Round(damage)` (zaokrąglenie matematyczne do 0 miejsc po przecinku, `Mathf.Round`).
5. Oblicz kierunek odrzutu: `knockback = (targetPosition − sourcePosition).normalized`; jeśli cel nie jest `Component` → `knockback = Vector3.zero`.
6. Zbuduj `DamageInfo { value = damage, type = Physical, isCritical, knockback, source = gameObject }` ([GDD-CMB-005]).
7. Wywołaj `target.TakeDamage(DamageInfo)`.
8. Jeśli `isCritical`: opublikuj `OnCriticalHit { damage, worldPosition }` przez EventBus (`GameEvents.cs`).

**Ograniczenia:** Losowanie krytu używa `Random.value` — naruszenie Z10; zostanie zastąpione strumieniem z seedem w zadaniu #18. Brak limitu dolnego `TotalDamage` po stronie nadawcy; minimalna strata 1 HP stosowana przez odbiorcę ([GDD-CMB-006] krok 4).

**Parametry:**

| Parametr | Wartość bazowa | Jednostka | Zakres |
|---|---|---|---|
| `baseDamage` | 10 | HP | ≥ 1 |
| `damagePerLevel` | 2 | HP/poziom | ≥ 0 |
| `baseCritChance` | 5 | % | 0–100 |
| `critMultiplier` | 2 | × | > 1 |

**Wzory:**
```
TotalDamage = baseDamage + damagePerLevel × (level − 1) + bonusDamage + skillBonusDamage
outgoing    = Round(TotalDamage × (isCritical ? critMultiplier : 1))
```
Zaokrąglenie: `Mathf.Round` — do 0 miejsc po przecinku. Limit dolny: brak po stronie nadawcy (patrz [GDD-CMB-006]).

**Interakcje:** [GDD-CMB-001] wywołuje ten element; [GDD-CMB-005] struktura wynikowa; [GDD-CMB-006] dalsze przetwarzanie po stronie celu; `PlayerStats` ([Player/Stats.md]) — `TotalDamage`, `TotalCritChance`, `critMultiplier`.

**Sytuacje wyjątkowe:**
- `target == null || !target.IsAlive` → `ApplyDamage()` pomija obliczenia; brak zdarzenia.
- `target` nie jest `Component` → `knockback = Vector3.zero`.

**Informacja zwrotna:** `OnCriticalHit` subskrybują UI i efekty (realizacja w zadaniu #17).

**Implementacja:** `Assets/Ratocalypse/Scripts/Player/PlayerCombat.cs` — `ApplyDamage()`; `Assets/Ratocalypse/Scripts/Player/PlayerStats.cs` — `TotalDamage`, `RollCrit()`.

**Historia zmian:**

| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. |
| 0.2 | Zaktualizowano sygnaturę `TakeDamage` na `DamageInfo` ([GDD-CMB-005]); dodano krok budowania struktury; uzupełniono tabelę parametrów o jednostki i zakres. |

---

## GDD-CMB-003 Fallback timer ataku

**Status:** Zaimplementowana
**Wersja:** 0.1

**Opis funkcjonalny:** Gdy Animator jest niedostępny lub Animation Events nie są skonfigurowane, timer resetuje `canAttack` po czasie proporcjonalnym do prędkości ataku.
**Warunki uruchomienia:** `canAttack == false && fallbackTimer > 0` — sprawdzane w każdej klatce.

**Zasady działania:**
1. Na początku `TriggerAttack()` timer ustawiony na `1 / max(TotalAttackSpeed, 0,1)` s.
2. Jeśli `animator == null` → `ApplyDamage()` wywoływane natychmiastowo w `TriggerAttack()`.
3. Timer odlicza w `Update()`; po wygaśnięciu: `ResetTrigger("Attack")`, `canAttack = true`, `fallbackTimer = 0`.

**Ograniczenia:** Timer aktywny nawet gdy Animator istnieje — `OnAttackEnd()` zeruje go przez `fallbackTimer = 0`. Nie zastępuje Animation Events w buildzie produkcyjnym.

**Parametry:**

| Parametr | Wartość | Jednostka | Zakres |
|---|---|---|---|
| Czas fallbacku | `1 / max(TotalAttackSpeed, 0,1)` | s | > 0 |
| Min mianownik | 0,1 | — | stała: 0,1 |

**Wzory:** `fallbackDuration = 1 / max(TotalAttackSpeed, 0,1)` [s]. Zaokrąglenie: nie dotyczy (wartość float, brak zaokrąglenia).

**Interakcje:** [GDD-CMB-001]; `PlayerStats` ([Player/Stats.md]) — `TotalAttackSpeed`.

**Sytuacje wyjątkowe:**
- `OnAttackEnd()` wywołane przed wygaśnięciem timera → timer zerowany przez `fallbackTimer = 0`; brak podwójnego resetu `canAttack`.

**Informacja zwrotna:** nie dotyczy.

**Implementacja:** `Assets/Ratocalypse/Scripts/Player/PlayerCombat.cs` — `TickFallback()`.

**Historia zmian:**

| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. |

---

## GDD-CMB-004 Interfejsy walki

**Status:** Zaimplementowana
**Wersja:** 0.2

**Opis funkcjonalny:** Definiuje kontrakty, przez które obiekty oddziałują na siebie w walce.
**Warunki uruchomienia:** nie dotyczy (definicje statyczne).

**Zasady działania:**
1. Wołający pobiera referencję do interfejsu raz (w `Awake` lub przy wyborze celu) i nie rzutuje na konkretną klasę (Z4).
2. `TakeDamage(DamageInfo)` przyjmuje pełną strukturę obrażeń ([GDD-CMB-005]).

**Ograniczenia:** nie dotyczy.

**Parametry:** nie dotyczy.

**Wzory:** nie dotyczy.

**Interakcje:**

| Interfejs | Sygnatura | Plik interfejsu |
|---|---|---|
| `IDamageable` | `bool IsAlive`, `void TakeDamage(DamageInfo)` | `Core/IDamageable.cs` |
| `IHealable` | `void Heal(float amount)` | `Core/IHealable.cs` |

Implementacje: `PlayerHealthComponent.cs` (`IDamageable`, `IHealable`); `EnemyAI.cs` (`IDamageable`).

**Sytuacje wyjątkowe:**
- `ResetTrigger("Attack")` wywoływany zarówno w `OnAttackEnd` jak i `TickFallback` ([GDD-CMB-003]) — zapobiega zakleszczeniu triggera w Any State.

**Informacja zwrotna:** nie dotyczy.

**Implementacja:** `Assets/Ratocalypse/Scripts/Core/IDamageable.cs`, `Assets/Ratocalypse/Scripts/Core/IHealable.cs`.

**Historia zmian:**

| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. |
| 0.2 | Sygnatura `TakeDamage` zmieniona z `(float, bool, Vector3)` na `(DamageInfo)` ([GDD-CMB-005]). |

---

## GDD-CMB-005 Struktura DamageInfo

**Status:** Zaimplementowana
**Wersja:** 0.1

**Opis funkcjonalny:** Niezmienna struktura (`readonly struct`) przekazywana przez `IDamageable.TakeDamage()`. Niesie wszystkie dane jednego uderzenia — wartość, typ, flagę krytu, kierunek odrzutu i źródło.
**Warunki uruchomienia:** Tworzona przez każdy kod zadający obrażenia przed wywołaniem `TakeDamage()`.

**Zasady działania:**
1. Nadawca tworzy `DamageInfo` z kompletnie wypełnionymi polami.
2. Jeśli `source == null`: odbiorca loguje `Debug.LogWarning("[DamageInfo] source == null")` i kontynuuje — obrażenia są stosowane.
3. Odbiorca używa `value` jako wejścia do własnego wzoru redukcji ([GDD-CMB-006]).
4. `knockback` zarezerwowany — `PlayerHealthComponent` ignoruje go w tej wersji; przetwarzanie odrzutu planowane w przyszłym zadaniu.

**Ograniczenia:**
- `knockback` zarezerwowany — brak reakcji po stronie odbiorcy.
- Wrogowie nie rzucają krytem (`isCritical = false` zawsze po stronie `EnemyAI`).

**Parametry:**

| Parametr | Wartość | Jednostka | Zakres |
|---|---|---|---|
| `value` | wartość obrażeń wychodzących (po krycie, przed pancerzem) | HP | ≥ 0 |
| `type` | typ obrażeń; aktualnie tylko `Physical` | `DamageType` (enum) | — |
| `isCritical` | czy trafienie krytyczne | `bool` | — |
| `knockback` | znormalizowany kierunek odrzutu lub `Vector3.zero` | — | znormalizowany lub zero |
| `source` | obiekt zadający cios | `GameObject` | może być `null` (ostrzeżenie) |

**Wzory:** nie dotyczy (struktura danych).

**Interakcje:** [GDD-CMB-002] tworzy strukturę (gracz atakuje); [GDD-CMB-004] kontrakt `IDamageable`; [GDD-CMB-006] odbiorca przetwarza `value`.

**Sytuacje wyjątkowe:**
- `source == null` → `LogWarning`; obrażenia stosowane normalnie.
- `value < 0` → odbiorca oblicza `reduced = value × (1 − TotalArmor)` (wynik ujemny lub zerowy); dolny limit `max(reduced, 1)` w [GDD-CMB-006] krok 4 zapewnia ostatecznie 1 HP straty. Zachowanie poprawne koincydentalnie — nadawca odpowiada za `value ≥ 0`.

**Informacja zwrotna:** nie dotyczy (struktura danych).

**Implementacja:**
- `Assets/Ratocalypse/Scripts/Core/DamageInfo.cs` — `readonly struct DamageInfo`
- `Assets/Ratocalypse/Scripts/Core/DamageType.cs` — `enum DamageType { Physical }`

**Historia zmian:**

| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. Zastępuje trzy luźne parametry `TakeDamage(float, bool, Vector3)`. Zaimplementowana: `DamageInfo.cs`, `DamageType.cs`; commit `e8dbd2d` (2026-10-28). |

---

## GDD-CMB-006 Reguły odbioru obrażeń przez gracza

**Status:** Zaimplementowana
**Wersja:** 0.1

**Opis funkcjonalny:** Definiuje kolejność i wzory przetwarzania `DamageInfo` po stronie gracza — od otrzymania uderzenia do zmiany HP i publikacji zdarzeń.
**Warunki uruchomienia:** Wywołanie `IDamageable.TakeDamage(DamageInfo)` na `PlayerHealthComponent`.

**Zasady działania:**
1. Jeśli `IsInvincible == true` (iframes aktywne) → przerwij; 0 obrażeń; brak zdarzeń.
2. Jeśli `info.source == null` → `Debug.LogWarning("[PlayerHealthComponent] DamageInfo.source == null")`.
3. Oblicz redukcję pancerzem: `reduced = Round(info.value × (1 − TotalArmor), 1 m.p.p.)`. *(Uwaga: `TotalArmor` nie ma limitu górnego — błąd śledzony jako zadanie #2.)*
4. Zastosuj limit dolny obrażeń: `reduced = max(reduced, 1)` — minimalna strata to 1 HP.
5. Odejmij od HP: `currentHp = max(currentHp − reduced, 0)`.
6. Opublikuj `OnPlayerDamaged { amount = reduced, currentHp, maxHp }` przez EventBus (`GameEvents.cs`).
7. Jeśli `currentHp == 0` → opublikuj `OnPlayerDied` przez EventBus (`GameEvents.cs`).

**Ograniczenia:**
- `isCritical` z `DamageInfo` nie zmienia toku obliczeń po stronie odbiorcy w tej wersji.
- Reakcja na `knockback` nie jest zaimplementowana ([GDD-CMB-005] punkt 4).
- Limit górny `TotalArmor ≤ 0,75` nie jest wymuszany — błąd śledzony jako zadanie #2.

**Parametry:**

| Parametr | Wartość | Jednostka | Zakres |
|---|---|---|---|
| Minimalny odbiór obrażeń | 1 | HP | — |
| `TotalArmor` (bazowa, bez ekwipunku) | 0 | — | 0–1 (brak `Clamp`, błąd zadania #2) |

**Wzory:**
```
reduced     = Round(info.value × (1 − TotalArmor), 1 m.p.p.)
finalDamage = max(reduced, 1)
currentHp   = max(currentHp − finalDamage, 0)
```
Zaokrąglenie: `Mathf.Round(x * 10f) / 10f` (1 miejsce po przecinku).

**Interakcje:** [GDD-CMB-005] wejście `DamageInfo`; `PlayerStats` ([Player/Stats.md]) — `TotalArmor`, `currentHp`, `MaxHp`; zdarzenia EventBus: `OnPlayerDamaged`, `OnPlayerDied` (`Assets/Ratocalypse/Scripts/Core/GameEvents.cs`).

**Sytuacje wyjątkowe:**
- `IsInvincible == true` → 0 obrażeń, brak zdarzeń (krok 1).
- Gracz ginie od tego uderzenia → `OnPlayerDamaged` publikowane przed `OnPlayerDied`.
- `TotalArmor > 1` (błąd z zadania #2) → `reduced` ujemne; dolny limit `max(reduced, 1)` chroni przed wyleczeniem przez obrażenia.

**Informacja zwrotna:** `OnPlayerDamaged` → HUD odświeża pasek HP ([Player/Stats.md]). Brak dźwięku i efektów wizualnych — realizacja w zadaniu #17.

**Implementacja:**
- `Assets/Ratocalypse/Scripts/Player/PlayerHealthComponent.cs` — `IDamageable.TakeDamage(DamageInfo)` (obliczenia pancerza, publikacja zdarzeń)
- `Assets/Ratocalypse/Scripts/Player/PlayerStats.cs` — dostawca `TotalArmor`, `currentHp`, `MaxHp`
- `Assets/Ratocalypse/Scripts/Core/GameEvents.cs` — `OnPlayerDamaged`, `OnPlayerDied`

**Historia zmian:**

| Wersja | Zmiana |
|---|---|
| 0.1 | Pierwsza specyfikacja. Dokumentuje istniejące zachowanie kodu; ujednolica z [GDD-CMB-005]. Zaimplementowana w `PlayerHealthComponent.TakeDamage(DamageInfo)`; usunięta metoda `PlayerHealthComponent.TakeDamage(float)` (DP-3); commit `e8dbd2d` (2026-10-28). |
