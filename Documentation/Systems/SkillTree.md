# Drzewko umiejętności

**Skrypty:** `Assets/Ratocalypse/Scripts/Player/SkillTreeManager.cs`, `Assets/Ratocalypse/Scripts/Core/SkillDefinition.cs`  


---

## Ogólne zasady

- 12 umiejętności w 3 gałęziach po 4 tiery
- Każdą umiejętność można odblokować tylko raz
- Tier N wymaga odblokowanego Tier N-1 w tej samej gałęzi (Tier 1 bez prereq)
- Koszt: 1 punkt umiejętności za skill
- Punkty zdobywane: 1 za każdy level up (`availableSkillPoints++`)

---

## Gałąź: Warrior ⚔️

| ID | Tier | Nazwa | Efekt | Prereq |
|---|---|---|---|---|
| `warrior_t1` | 1 | Mega Siła | +5 obrażeń | — |
| `warrior_t2` | 2 | Żelazna Skóra | +8% pancerza | warrior_t1 |
| `warrior_t3` | 3 | Berserker | +0.3 szybkości ataku | warrior_t2 |
| `warrior_t4` | 4 | Śmiertelny Cios | +10% szansy na kryta | warrior_t3 |

---

## Gałąź: Hunter 🏹

| ID | Tier | Nazwa | Efekt | Prereq |
|---|---|---|---|---|
| `hunter_t1` | 1 | Szybkość | +0.8 prędkości ruchu | — |
| `hunter_t2` | 2 | Wytrzymałość | +30 max staminy | hunter_t1 |
| `hunter_t3` | 3 | Precyzja | +8% szansy na kryta | hunter_t2 |
| `hunter_t4` | 4 | Szybki atak | +0.4 szybkości ataku | hunter_t3 |

---

## Gałąź: Survivor 🛡️

| ID | Tier | Nazwa | Efekt | Prereq |
|---|---|---|---|---|
| `survivor_t1` | 1 | Twardziel | +40 max HP | — |
| `survivor_t2` | 2 | Regeneracja | +1.5 HP/s | survivor_t1 |
| `survivor_t3` | 3 | Forteca | +7% pancerza | survivor_t2 |
| `survivor_t4` | 4 | Witalność | +60 max HP | survivor_t3 |

---

## Typy efektów (SkillEffectType)

`Damage`, `Armor`, `CritChance`, `MoveSpeed`, `MaxHp`, `HpRegen`, `AttackSpeed`, `MaxStamina`

---

## Logika odblokowania

**Warunki (`CanUnlock`):**
1. Skill nie jest już odblokowany
2. `availableSkillPoints >= 1`
3. Prereq odblokowany (lub brak prereq dla Tier 1)

**Efekt (`TryUnlock`):** Dodaje ID do `_unlocked`, odejmuje punkt, wywołuje `ApplyBonus()`, publikuje `OnSkillUnlocked`.

---

## Integracja z SaveSystem

`RestoreFromSave(IEnumerable<string> savedSkills)`:
1. Wywołuje `stats.ResetSkillBonuses()`
2. Aplikuje bonusy z listy zapisanych ID — bez kosztowania punktów
