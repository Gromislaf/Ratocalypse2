using System.Collections.Generic;
using UnityEngine;

namespace Ratocalypse
{
    // Rejestr wszystkich umiejętności w grze — SO z listą SkillDefinitionSO.
    [CreateAssetMenu(menuName = "Ratocalypse/Skill Registry")]
    public class SkillRegistrySO : ScriptableObject
    {
        [SerializeField] private SkillDefinitionSO[] _skills;

        public IReadOnlyList<SkillDefinitionSO> All => _skills ?? System.Array.Empty<SkillDefinitionSO>();

        // Szukanie liniowe — 12 elementów, Dictionary zbędny.
        public SkillDefinitionSO GetById(string id)
        {
            if (_skills == null) return null;
            foreach (var skill in _skills)
                if (skill != null && skill.Id == id) return skill;
            return null;
        }
    }
}
