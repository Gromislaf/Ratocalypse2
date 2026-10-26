# Typy przeciwników

**Skrypt danych:** `Assets/Ratocalypse/Scripts/Enemy/EnemyData.cs` (ScriptableObject)  
Typy: 1 zaimplementowany (Szczur zwykły), 5 planowanych

---

## Parametry EnemyData (wspólne dla wszystkich typów)

| Pole | Opis |
|---|---|
| `maxHp` | Punkty życia |
| `moveSpeed` | Prędkość podczas pościgu |
| `patrolSpeed` | Prędkość podczas patrolu |
| `patrolRadius` | Promień losowania punktu patrolowego (bez waypointów) |
| `detectRange` | Zasięg wykrycia gracza → stan Alert |
| `losePlayerRange` | Zasięg rezygnacji z pościgu → powrót do Patrol |
| `alertDelay` | Czas postoju w stanie Alert przed pościgiem |
| `attackRange` | Zasięg ataku wręcz |
| `attackDamage` | Obrażenia zadawane graczowi |
| `attackCooldown` | Minimalny czas między atakami |
| `stunDuration` | Czas ogłuszenia (wywołane przez PlayerCombat) |
| `xpReward` | XP przyznawane graczowi po zabiciu |

---

## ENE-001 Szczur zwykły ✅

| Parametr | Wartość |
|---|---|
| maxHp | 50 |
| moveSpeed | 3.5 m/s |
| patrolSpeed | 1.5 m/s |
| patrolRadius | 4 m |
| detectRange | 8 m |
| losePlayerRange | 14 m |
| alertDelay | 0.6 s |
| attackRange | 1.5 m |
| attackDamage | 10 HP |
| attackCooldown | 1.5 s |
| stunDuration | 2 s |
| xpReward | 25 XP |

**Zachowanie AI:** Patrol losowy → Alert → pościg → atak wręcz. Grupowy — skuteczny w dużych liczbach.

---

## ENE-002 Opój (tank) ⏳

**Specjalność:** Duże HP, wolny, atak AoE, odporny na knockback.

<!-- TODO: uzupełnić wartości po implementacji -->

---

## ENE-003 Spluwacz ⏳

**Specjalność:** Dystansowy. Ucieka gdy gracz zbliży się poniżej min. dystansu. Pluwa jadem (truciznę → status effect).

<!-- TODO: uzupełnić wartości po implementacji -->

---

## ENE-004 Skoczek ⏳

**Specjalność:** Teleportuje się za gracza — flankowanie. Wysoka prędkość.

<!-- TODO: uzupełnić wartości po implementacji -->

---

## ENE-005 Alfa (mini-boss) ⏳

**Specjalność:** Budzi nieaktywnych wrogów w zasięgu. Fazy walki (zmiana zachowania przy % HP).

<!-- TODO: uzupełnić wartości po implementacji -->

---

## ENE-006 Król Szczurów (boss) ⏳

**Specjalność:** 3 fazy, summon dodatkowych wrogów, AoE, ucieczka przy niskim HP.

<!-- TODO: uzupełnić wartości po implementacji -->
