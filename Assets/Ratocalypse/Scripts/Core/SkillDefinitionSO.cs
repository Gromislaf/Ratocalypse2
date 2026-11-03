using UnityEngine;

namespace Ratocalypse
{
    // Typy efektów które umiejętność może aplikować na PlayerStats.
    public enum SkillEffectType
    {
        Damage,
        Armor,
        AttackSpeed,
        CritChance,
        MoveSpeed,
        MaxStamina,
        MaxHp,
        HpRegen
    }

    // Dane jednej umiejętności — ScriptableObject, read-only w runtime (Z2).
    [CreateAssetMenu(menuName = "Ratocalypse/Skill Definition")]
    public class SkillDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        [SerializeField] private SkillBranch _branch;
        [SerializeField] private int _tier;
        [SerializeField] private SkillEffectType _effectType;
        [SerializeField] private float _effectValue;
        [SerializeField] private SkillDefinitionSO _prerequisite;

        public string Id            => _id;
        public string DisplayName   => _displayName;
        public string Description   => _description;
        public SkillBranch Branch   => _branch;
        public int Tier             => _tier;
        public SkillEffectType EffectType => _effectType;
        public float EffectValue    => _effectValue;
        // null = brak prerequisite (tier 1)
        public SkillDefinitionSO Prerequisite => _prerequisite;
    }
}
