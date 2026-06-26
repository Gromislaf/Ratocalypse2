using UnityEngine;
using System.Collections.Generic;

// Trzyma stan odblokowanych umiejętności i aplikuje ich bonusy do PlayerStats.
// Przypisz do tego samego GO co PlayerController.
public class SkillTreeManager : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;

    private readonly HashSet<string> _unlocked = new();

    public IReadOnlyCollection<string> UnlockedSkills => _unlocked;

    public bool IsUnlocked(string skillId) => _unlocked.Contains(skillId);

    public bool CanUnlock(in SkillDefinition skill)
    {
        if (_unlocked.Contains(skill.id)) return false;
        if (stats.availableSkillPoints < 1) return false;
        if (!string.IsNullOrEmpty(skill.prerequisiteId) && !_unlocked.Contains(skill.prerequisiteId))
            return false;
        return true;
    }

    public bool TryUnlock(string skillId)
    {
        var def = SkillDatabase.GetById(skillId);
        if (def == null) return false;
        if (!CanUnlock(def.Value)) return false;

        _unlocked.Add(skillId);
        stats.availableSkillPoints--;
        ApplyBonus(def.Value);

        EventBus.Publish(new OnSkillUnlocked
        {
            skillId = skillId,
            branch = def.Value.branch,
            remainingPoints = stats.availableSkillPoints
        });

        return true;
    }

    // Używane przez SaveSystem — przywraca bonusy bez kosztowania punktów.
    public void RestoreFromSave(IEnumerable<string> savedSkills)
    {
        stats.ResetSkillBonuses();
        _unlocked.Clear();
        foreach (var id in savedSkills)
        {
            var def = SkillDatabase.GetById(id);
            if (def == null) continue;
            _unlocked.Add(id);
            ApplyBonus(def.Value);
        }
    }

    private void ApplyBonus(in SkillDefinition skill)
    {
        switch (skill.effectType)
        {
            case SkillEffectType.Damage:      stats.skillBonusDamage      += skill.effectValue; break;
            case SkillEffectType.Armor:       stats.skillBonusArmor       += skill.effectValue; break;
            case SkillEffectType.CritChance:  stats.skillBonusCritChance  += skill.effectValue; break;
            case SkillEffectType.MoveSpeed:   stats.skillBonusMoveSpeed   += skill.effectValue; break;
            case SkillEffectType.MaxHp:       stats.skillBonusMaxHp       += skill.effectValue; break;
            case SkillEffectType.HpRegen:     stats.skillBonusHpRegen     += skill.effectValue; break;
            case SkillEffectType.AttackSpeed: stats.skillBonusAttackSpeed += skill.effectValue; break;
            case SkillEffectType.MaxStamina:  stats.skillBonusMaxStamina  += skill.effectValue; break;
        }
    }
}
