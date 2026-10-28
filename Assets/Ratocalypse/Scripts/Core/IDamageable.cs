// ============================================================
//  IDamageable.cs
//  Ratpocalypse � Core/IDamageable.cs
//
//  Interfejs dla wszystkiego co mo�e otrzyma� obra�enia:
//  wrog�w, gracza, obiekt�w zniszczalnych.
//
//  Dzi�ki interfejsowi system walki nie musi wiedzie�
//  czy trafi� szczura, gracza czy beczk� � po prostu wo�a
//  TakeDamage() i obiekt sam wie co z tym zrobi�.
// ============================================================

using Ratocalypse;

public interface IDamageable
{
    /// <summary>Czy cel jeszcze �yje?</summary>
    bool IsAlive { get; }

    /// <summary>
    /// Zadaj obra�enia celowi.
    /// </summary>
    /// <param name="info">Dane obra�e� (warto��, typ, krytyk, odrzut, �r�d�o)</param>
    void TakeDamage(DamageInfo info);
}

// ============================================================
//  IHealable � opcjonalny interfejs dla cel�w kt�re mo�na leczy�
// ============================================================

public interface IHealable
{
    void Heal(float amount);
}

// ============================================================
//  IStatusEffectable � cel kt�ry mo�e dosta� efekt statusu
// ============================================================

public interface IStatusEffectable
{
    void ApplyStatusEffect(StatusEffectType effectType, float duration, float damagePerTick = 0f);
}