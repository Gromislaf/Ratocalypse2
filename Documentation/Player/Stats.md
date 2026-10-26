# Statystyki gracza (PlayerStats)

**Skrypt:** `Assets/Ratocalypse/Scripts/Player/PlayerStats.cs`  
**Typ:** ScriptableObject (jeden asset w projekcie)  


---

## Wartości bazowe

| Parametr | Wartość | Opis |
|---|---|---|
| `baseMaxHp` | 100 | Max HP na poziomie 1 |
| `hpPerLevel` | 15 | Przyrost HP za każdy level |
| `baseMaxStamina` | 100 | Maksymalna stamina |
| `staminaRegenPerSecond` | 20 | Regeneracja staminy/s |
| `dodgeCost` | 25 | Koszt staminy za unik |
| `baseDamage` | 10 | Obrażenia bez broni |
| `damagePerLevel` | 2 | Wzrost obrażeń za level |
| `moveSpeed` | 5 | Prędkość ruchu (NavMeshAgent.speed) |
| `dodgeSpeed` | 12 | Prędkość podczas uniku |
| `dodgeDuration` | 0.3 s | Czas trwania uniku |
| `baseAttackSpeed` | 1.0 | Mnożnik prędkości animacji ataku |
| `baseArmor` | 0% | Procentowa redukcja obrażeń |
| `baseCritChance` | 5% | Szansa na trafienie krytyczne |
| `critMultiplier` | 2x | Mnożnik obrażeń krytycznych |
| `maxLevel` | 20 | Maksymalny poziom gracza |

## Wzory obliczeniowe

```
MaxHp           = baseMaxHp + (hpPerLevel × (level − 1)) + bonusMaxHp + skillBonusMaxHp
TotalDamage     = baseDamage + (damagePerLevel × (level − 1)) + bonusDamage + skillBonusDamage
TotalArmor      = Clamp(baseArmor + bonusArmor + skillBonusArmor, 0, 0.75)   // max 75%
TotalAttackSpeed= baseAttackSpeed + bonusAttackSpeed + skillBonusAttackSpeed
TotalCritChance = Clamp01(baseCritChance + bonusCritChance + skillBonusCritChance)
TotalMoveSpeed  = moveSpeed + bonusMoveSpeed + skillBonusMoveSpeed

XP do następnego levelu = xpForLevel2 × (1.4 ^ (level − 1))
```

## Bonusy — dwa niezależne źródła

| Typ | Pola | Kto ustawia | Kiedy resetowane |
|---|---|---|---|
| Ekwipunek | `bonus*` (bonusDamage, bonusArmor...) | `InventorySystem.RecalculateBonuses()` | Przy każdym equip/unequip |
| Drzewko umiejętności | `skillBonus*` (skillBonusDamage...) | `SkillTreeManager.ApplyBonus()` | `ResetSkillBonuses()` — tylko przy RestoreFromSave |

**Ważne:** `ResetBonuses()` (ekwipunek) **nie dotyka** pól `skillBonus*`. Są od siebie niezależne.

## Level up

Po awansie HP skaluje się proporcjonalnie (nie leczy do pełna, nie traci HP):
```
currentHp = (currentHp / oldMaxHp) × newMaxHp
```
Gracz otrzymuje 1 punkt umiejętności (`availableSkillPoints++`).
