# AI przeciwników — Maszyna stanów (FSM)

**Skrypt:** `Assets/Ratocalypse/Scripts/Enemy/EnemyAI.cs`  
**Status:** ✅ Zaimplementowane

---

## Stany i przejścia

```
PATROL → (gracz w detectRange) → ALERT → (alertDelay minął) → CHASE → (w attackRange) → ATTACK
                                                                  ↑                           |
                                                    (gracz > losePlayerRange)           (HP ≤ 0)
                                                         PATROL ←                        DEAD

CHASE / PATROL → (ApplyStun()) → STUNNED → (czas minął) → CHASE
```

---

## Specyfikacja stanów

### PATROL

**Opis:** Wróg porusza się po przypisanej trasie lub losowo w promieniu startowej pozycji.

**Warunki wejścia:** Stan domyślny; powrót po utracie gracza z zasięgu.

**Zasady działania:**
- Jeśli przypisano `waypoints[]` — chodzi po nich w pętli
- Bez waypointów — losuje punkt w promieniu `patrolRadius` od pozycji startowej (do 8 prób na NavMesh)
- Prędkość: `patrolSpeed`

**Warunki wyjścia:** Gracz w zasięgu `detectRange` → ALERT.

---

### ALERT

**Opis:** Wróg wykrył gracza — zatrzymuje się i czeka przed pościgiem.

**Warunki wejścia:** Odległość do gracza ≤ `detectRange`.

**Zasady działania:** Stoi nieruchomo przez `alertDelay` sekund.

**Warunki wyjścia:** Timer `alertDelay` minął → CHASE.

---

### CHASE

**Opis:** Wróg podąża za graczem przez NavMesh.

**Warunki wejścia:** Po zakończeniu stanu Alert; po zakończeniu stanu Stunned.

**Zasady działania:**
- Prędkość: `moveSpeed`
- NavMeshAgent aktualizuje trasę co klatkę
- Gdy gracz niedostępny (NavMesh bez drogi) — stoi w miejscu

**Warunki wyjścia:**
- Odległość ≤ `attackRange` → ATTACK
- Odległość > `losePlayerRange` → PATROL
- `ApplyStun()` wywołane → STUNNED

---

### ATTACK

**Opis:** Wróg w zasięgu ataku — atakuje przez Animation Event.

**Warunki wejścia:** Odległość do gracza ≤ `attackRange`.

**Zasady działania:**
- Animator trigger `"Attack"` odpala animację
- **Animation Event `OnAttackHit()`** — `playerDamageable.TakeDamage(attackDamage)`
- Cooldown: `attackCooldown` między atakami
- NavMesh zatrzymany podczas ataku

**Warunki wyjścia:**
- Gracz wychodzi poza `attackRange` → CHASE
- HP ≤ 0 → DEAD
- `ApplyStun()` → STUNNED

---

### STUNNED

**Opis:** Wróg ogłuszony — nieruchomy, bez ataku.

**Warunki wejścia:** `ApplyStun(duration)` wywołane przez PlayerCombat.

**Zasady działania:** NavMesh zatrzymany, brak ataku przez `duration` sekund.

**Warunki wyjścia:** Timer minął → CHASE.

---

### DEAD

**Opis:** Wróg zabity.

**Warunki wejścia:** `currentHp ≤ 0`.

**Zasady działania:**
1. NavMesh wyłączony
2. Collider wyłączony (brak interakcji z graczem)
3. Animator trigger `"Dead"`
4. Publikuje `OnEnemyDied { xpReward }` → XPSystem przyznaje XP
5. `Destroy(gameObject)` po 3 sekundach

**Warunki wyjścia:** Brak (stan terminalny).

---

## Interakcje z innymi systemami

| System | Interakcja |
|---|---|
| PlayerCombat | Wywołuje `TakeDamage()` (przez IDamageable), `ApplyStun(duration)` |
| XPSystem | Subskrybuje `OnEnemyDied` — przyznaje `xpReward` XP |
| MinimapController | MinimapMarker (typ Enemy) — znika automatycznie przy Destroy |
| EventBus | Publikuje `OnEnemyDamaged`, `OnEnemyDied` |
