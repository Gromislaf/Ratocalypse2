// Typy efektów które umiejętność może aplikować na PlayerStats.
public enum SkillEffectType
{
    Damage,
    Armor,
    CritChance,
    MoveSpeed,
    MaxHp,
    HpRegen,
    AttackSpeed,
    MaxStamina
}

// Dane jednej umiejętności — definiowane w kodzie, bez asset pipeline.
public readonly struct SkillDefinition
{
    public readonly string id;
    public readonly string displayName;
    public readonly string description;
    public readonly SkillBranch branch;
    public readonly int tier;               // 1–4
    public readonly SkillEffectType effectType;
    public readonly float effectValue;
    public readonly string prerequisiteId;  // pusty = brak prerequisite (tier 1)

    public SkillDefinition(string id, string displayName, string description,
        SkillBranch branch, int tier, SkillEffectType effectType, float effectValue,
        string prerequisiteId = "")
    {
        this.id = id;
        this.displayName = displayName;
        this.description = description;
        this.branch = branch;
        this.tier = tier;
        this.effectType = effectType;
        this.effectValue = effectValue;
        this.prerequisiteId = prerequisiteId;
    }
}

// Statyczna baza wszystkich umiejętności w grze.
public static class SkillDatabase
{
    public static readonly SkillDefinition[] All = new[]
    {
        // ── WARRIOR ─────────────────────────────────────────────
        new SkillDefinition(
            "warrior_t1", "Siła Bruta", "+5 obrażeń",
            SkillBranch.Warrior, 1, SkillEffectType.Damage, 5f),

        new SkillDefinition(
            "warrior_t2", "Żelazna Skóra", "+8% pancerza",
            SkillBranch.Warrior, 2, SkillEffectType.Armor, 0.08f,
            "warrior_t1"),

        new SkillDefinition(
            "warrior_t3", "Berserker", "+0.3 szybkości ataku",
            SkillBranch.Warrior, 3, SkillEffectType.AttackSpeed, 0.3f,
            "warrior_t2"),

        new SkillDefinition(
            "warrior_t4", "Śmiertelny Cios", "+10% szansy na kryta",
            SkillBranch.Warrior, 4, SkillEffectType.CritChance, 0.10f,
            "warrior_t3"),

        // ── HUNTER ──────────────────────────────────────────────
        new SkillDefinition(
            "hunter_t1", "Zwinna Noga", "+0.8 prędkości ruchu",
            SkillBranch.Hunter, 1, SkillEffectType.MoveSpeed, 0.8f),

        new SkillDefinition(
            "hunter_t2", "Wytrzymałość", "+30 max staminy",
            SkillBranch.Hunter, 2, SkillEffectType.MaxStamina, 30f,
            "hunter_t1"),

        new SkillDefinition(
            "hunter_t3", "Precyzja", "+8% szansy na kryta",
            SkillBranch.Hunter, 3, SkillEffectType.CritChance, 0.08f,
            "hunter_t2"),

        new SkillDefinition(
            "hunter_t4", "Błyskawica", "+0.4 szybkości ataku",
            SkillBranch.Hunter, 4, SkillEffectType.AttackSpeed, 0.4f,
            "hunter_t3"),

        // ── SURVIVOR ────────────────────────────────────────────
        new SkillDefinition(
            "survivor_t1", "Twardziel", "+40 max HP",
            SkillBranch.Survivor, 1, SkillEffectType.MaxHp, 40f),

        new SkillDefinition(
            "survivor_t2", "Regeneracja", "+1.5 HP/s",
            SkillBranch.Survivor, 2, SkillEffectType.HpRegen, 1.5f,
            "survivor_t1"),

        new SkillDefinition(
            "survivor_t3", "Forteca", "+7% pancerza",
            SkillBranch.Survivor, 3, SkillEffectType.Armor, 0.07f,
            "survivor_t2"),

        new SkillDefinition(
            "survivor_t4", "Ostatnia Wola", "+60 max HP",
            SkillBranch.Survivor, 4, SkillEffectType.MaxHp, 60f,
            "survivor_t3"),
    };

    public static SkillDefinition? GetById(string id)
    {
        foreach (var skill in All)
            if (skill.id == id) return skill;
        return null;
    }
}
