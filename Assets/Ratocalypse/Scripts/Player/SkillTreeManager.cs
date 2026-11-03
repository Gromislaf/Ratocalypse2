using UnityEngine;
using System.Collections.Generic;
using Ratocalypse;

// Trzyma stan odblokowanych umiejętności i aplikuje ich bonusy do PlayerStats.
// Przypisz do tego samego GO co PlayerController.
public class SkillTreeManager : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("stats")]
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private SkillRegistrySO _registry;

    private readonly HashSet<string> _unlocked = new();

    public IReadOnlyCollection<string> UnlockedSkills => _unlocked;
    public SkillRegistrySO Registry => _registry;

    private void Awake()
    {
        if (_stats == null)    Debug.LogError("[SkillTreeManager] PlayerStats nie przypisany!", this);
        if (_registry == null) Debug.LogError("[SkillTreeManager] SkillRegistrySO nie przypisany!", this);
    }

    public bool IsUnlocked(string skillId) => _unlocked.Contains(skillId);

    public bool CanUnlock(SkillDefinitionSO skill)
    {
        if (_stats == null || _registry == null) return false;
        if (_unlocked.Contains(skill.Id)) return false;
        if (_stats.availableSkillPoints < 1) return false;
        if (skill.Prerequisite != null && !_unlocked.Contains(skill.Prerequisite.Id))
            return false;
        return true;
    }

    public bool TryUnlock(string skillId)
    {
        if (_stats == null || _registry == null) return false;
        var def = _registry.GetById(skillId);
        if (def == null) return false;
        if (!CanUnlock(def)) return false;

        _unlocked.Add(skillId);
        _stats.availableSkillPoints--;
        ApplyBonus(def);

        EventBus.Publish(new OnSkillUnlocked
        {
            skillId = skillId,
            branch = def.Branch,
            remainingPoints = _stats.availableSkillPoints
        });

        return true;
    }

    // Używane przez SaveSystem — przywraca bonusy bez kosztowania punktów.
    public void RestoreFromSave(IEnumerable<string> savedSkills)
    {
        _stats.ResetSkillBonuses();
        _unlocked.Clear();
        foreach (var id in savedSkills)
        {
            var def = _registry.GetById(id);
            if (def == null)
            {
                Debug.LogWarning($"[SkillTreeManager] Unknown skill ID: {id}");
                continue;
            }
            _unlocked.Add(id);
            ApplyBonus(def);
        }
    }

    private void ApplyBonus(SkillDefinitionSO skill)
    {
        switch (skill.EffectType)
        {
            case SkillEffectType.Damage:      _stats.skillBonusDamage      += skill.EffectValue; break;
            case SkillEffectType.Armor:       _stats.skillBonusArmor       += skill.EffectValue; break;
            case SkillEffectType.CritChance:  _stats.skillBonusCritChance  += skill.EffectValue; break;
            case SkillEffectType.MoveSpeed:   _stats.skillBonusMoveSpeed   += skill.EffectValue; break;
            case SkillEffectType.MaxHp:       _stats.skillBonusMaxHp       += skill.EffectValue; break;
            case SkillEffectType.HpRegen:     _stats.skillBonusHpRegen     += skill.EffectValue; break;
            case SkillEffectType.AttackSpeed: _stats.skillBonusAttackSpeed += skill.EffectValue; break;
            case SkillEffectType.MaxStamina:  _stats.skillBonusMaxStamina  += skill.EffectValue; break;
        }
    }
}
