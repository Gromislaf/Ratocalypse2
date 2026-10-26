# System XP i poziomów

**Skrypty:** `Assets/Ratocalypse/Scripts/Player/XPSystem.cs`, `XPBarUI.cs`, `PlayerHealthComponent.cs`  


---

## Źródła XP

| Źródło | Wartość | Event |
|---|---|---|
| Zabicie wroga | `EnemyData.xpReward` (domyślnie 25 XP) | `OnEnemyDied` |
| Ukończenie questa | `QuestData.xpReward` | `OnQuestCompleted` |

---

## Wzór progu XP na level up

```
XP_required(level) = xpForLevel2 × (1.4 ^ (level − 1))
```

Przykładowe progi (przy `xpForLevel2 = 100`):

| Poziom | XP do awansu |
|---|---|
| 1 → 2 | 100 |
| 2 → 3 | 140 |
| 3 → 4 | 196 |
| 4 → 5 | 274 |

**Maksymalny poziom:** 20 (`PlayerStats.maxLevel`).

---

## Level up — efekty

1. HP skaluje się proporcjonalnie: `currentHp = (currentHp / oldMaxHp) × newMaxHp`
2. `availableSkillPoints++`
3. Publikuje `OnPlayerLevelUp { newLevel, remainingPoints }`

---

## Regeneracja HP

**Źródło:** `skillBonusHpRegen` (Survivor T2: +1.5 HP/s)

**Przebieg:** `PlayerHealthComponent.Update()` dodaje `stats.TotalHpRegen × deltaTime` HP co klatkę. Jeśli `TotalHpRegen == 0` — brak regeneracji.

---

## XPBarUI

**Opis:** Poziomy pasek XP w HUD z licznikiem poziomu i notyfikacją level up.

| Cecha | Zachowanie |
|---|---|
| Widoczność | Zawsze podczas rozgrywki |
| Wartość | `currentXP / xpToNextLevel` |
| Aktualizacja | Subskrybuje `OnPlayerXPGained` |
| Level up | Aktywuje GameObject "LEVEL UP!" na `levelUpDisplayTime` sekund |
