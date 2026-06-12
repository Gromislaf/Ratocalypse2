// ============================================================
//  IDamageable.cs
//  Ratpocalypse — Core/IDamageable.cs
//
//  Interfejs dla wszystkiego co mo¿e otrzymaæ obra¿enia:
//  wrogów, gracza, obiektów zniszczalnych.
//
//  Dziêki interfejsowi system walki nie musi wiedzieæ
//  czy trafi³ szczura, gracza czy beczkê — po prostu wo³a
//  TakeDamage() i obiekt sam wie co z tym zrobiæ.
// ============================================================

using UnityEngine;

public interface IDamageable
{
    /// <summary>Czy cel jeszcze ¿yje?</summary>
    bool IsAlive { get; }

    /// <summary>
    /// Zadaj obra¿enia celowi.
    /// </summary>
    /// <param name="damage">Iloœæ obra¿eñ (przed redukcj¹ przez cel)</param>
    /// <param name="isCritical">Czy to trafienie krytyczne (do efektów wizualnych)</param>
    /// <param name="knockbackDirection">Kierunek odrzutu (Vector3.zero = brak)</param>
    void TakeDamage(float damage, bool isCritical = false, Vector3 knockbackDirection = default);
}

// ============================================================
//  IHealable — opcjonalny interfejs dla celów które mo¿na leczyæ
// ============================================================

public interface IHealable
{
    void Heal(float amount);
}

// ============================================================
//  IStatusEffectable — cel który mo¿e dostaæ efekt statusu
// ============================================================

public interface IStatusEffectable
{
    void ApplyStatusEffect(StatusEffectType effectType, float duration, float damagePerTick = 0f);
}