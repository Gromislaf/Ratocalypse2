// ============================================================
//  DamageInfoTests.cs
//  Ratocalypse.Tests — EditMode
//
//  Testy automatyczne dla DamageInfo i PlayerHealthComponent
//  (T1–T4, T8–T10 z planu zadania #1).
//
//  STAN: Gotowe do uruchomienia po zadaniu #26
//  (Assembly Definitions + struktura testów).
//
//  Powod blokady: Kod gry kompiluje sie do predefined assembly
//  "Assembly-CSharp", ktorej nie mozna referencjonowac z asmdef
//  testow. Po zadaniu #26 (Ratocalypse.Runtime.asmdef) ta bariera
//  zostanie usunieta i testy uruchomia sie bez zmian w ich logice.
//
//  T5, T6, T7 wymagaja singletonow runtime (GameManager) —
//  oznaczone jako RECZNE w checklistce dla autora.
// ============================================================

// Blokada kompilacji do czasu zadania #26 (usun ten blok po stworzeniu Ratocalypse.Runtime.asmdef)
#if RATOCALYPSE_TESTS_ENABLED

using System.Reflection;
using NUnit.Framework;
using Ratocalypse;
using UnityEngine;

namespace Ratocalypse.Tests
{
    /// <summary>
    /// Testy struktury DamageInfo — poprawnosc pol i konstruktora.
    /// </summary>
    public class DamageInfoStructTests
    {
        [Test]
        public void DamageInfo_AllFieldsSetCorrectly()
        {
            var go = new GameObject("source");
            var info = new DamageInfo(
                value: 15f,
                type: DamageType.Physical,
                isCritical: true,
                knockback: Vector3.forward,
                source: go
            );

            Assert.AreEqual(15f, info.value);
            Assert.AreEqual(DamageType.Physical, info.type);
            Assert.IsTrue(info.isCritical);
            Assert.AreEqual(Vector3.forward, info.knockback);
            Assert.AreEqual(go, info.source);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DamageInfo_SourceNull_Allowed()
        {
            // DamageInfo pozwala na source == null (PlayerHealthComponent loguje wtedy warning)
            var info = new DamageInfo(10f, DamageType.Physical, false, Vector3.zero, null);
            Assert.IsNull(info.source);
        }

        [Test]
        public void DamageInfo_IsReadonlyStruct_NotReferenceType()
        {
            var info = new DamageInfo(5f, DamageType.Physical, false, Vector3.zero, null);
            Assert.IsTrue(info.GetType().IsValueType, "DamageInfo musi byc struct (value type)");
        }
    }

    /// <summary>
    /// Testy PlayerHealthComponent.TakeDamage — 7 krokow GDD-CMB-006.
    /// Izolacja: PlayerStats jako CreateInstance (ScriptableObject), PlayerHealthComponent
    /// dodany do tymczasowego GameObject; pole 'stats' ustawiane przez reflection.
    /// </summary>
    public class PlayerHealthComponentTests
    {
        private GameObject _go;
        private PlayerHealthComponent _health;
        private PlayerStats _stats;

        [SetUp]
        public void SetUp()
        {
            // Czyscimy EventBus przed kazdym testem
            EventBus.Clear();

            _stats = ScriptableObject.CreateInstance<PlayerStats>();
            _stats.baseMaxHp = 100f;
            _stats.baseArmor = 0f;
            _stats.hpRegenPerSecond = 0f;
            _stats.InitializeForNewGame();

            _go = new GameObject("PlayerTestObject");
            _health = _go.AddComponent<PlayerHealthComponent>();

            // Ustawiamy prywatne pole 'stats' przez reflection
            var field = typeof(PlayerHealthComponent)
                .GetField("stats", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "Pole 'stats' nie znalezione w PlayerHealthComponent — refleksja nie zadziala");
            field.SetValue(_health, _stats);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_stats);
        }

        // --------------------------------------------------------
        // T1: Normalny atak, brak pancerza, brak invincibility
        // --------------------------------------------------------
        [Test]
        public void T1_NormalAttack_NoArmor_HpReducedByDamage()
        {
            float initialHp = _stats.currentHp; // 100f
            int damageEventCount = 0;
            float receivedAmount = 0f;

            EventBus.Subscribe<OnPlayerDamaged>(e =>
            {
                damageEventCount++;
                receivedAmount = e.amount;
            });

            var info = new DamageInfo(30f, DamageType.Physical, false, Vector3.zero,
                new GameObject("enemy"));
            ((IDamageable)_health).TakeDamage(info);

            Assert.AreEqual(1, damageEventCount, "T1: OnPlayerDamaged powinien byc wyslany dokladnie raz");
            Assert.AreEqual(initialHp - 30f, _stats.currentHp, 0.01f,
                "T1: HP powinno spasc o 30");
            Assert.AreEqual(30f, receivedAmount, 0.01f,
                "T1: Zdarzenie powinno zawierac kwote 30");
        }

        // --------------------------------------------------------
        // T2: Atak gdy IsInvincible=true — brak efektu
        // --------------------------------------------------------
        [Test]
        public void T2_IsInvincible_NoDamageNoEvent()
        {
            _health.IsInvincible = true;
            float initialHp = _stats.currentHp;
            int damageEventCount = 0;

            EventBus.Subscribe<OnPlayerDamaged>(_ => damageEventCount++);

            var info = new DamageInfo(50f, DamageType.Physical, false, Vector3.zero,
                new GameObject("enemy"));
            ((IDamageable)_health).TakeDamage(info);

            Assert.AreEqual(initialHp, _stats.currentHp, 0.001f,
                "T2: HP nie powinno spasc podczas invincibility");
            Assert.AreEqual(0, damageEventCount,
                "T2: OnPlayerDamaged nie powinien byc wyslany podczas invincibility");
        }

        // --------------------------------------------------------
        // T3: Smiertelne trafienie — currentHp=0, kolejnosc zdarzen
        // --------------------------------------------------------
        [Test]
        public void T3_LethalHit_HpZeroAndOrderOfEvents()
        {
            _stats.currentHp = 20f;
            var eventOrder = new System.Collections.Generic.List<string>();

            EventBus.Subscribe<OnPlayerDamaged>(_ => eventOrder.Add("Damaged"));
            EventBus.Subscribe<OnPlayerDied>(_ => eventOrder.Add("Died"));

            var info = new DamageInfo(100f, DamageType.Physical, false, Vector3.zero,
                new GameObject("enemy"));
            ((IDamageable)_health).TakeDamage(info);

            Assert.AreEqual(0f, _stats.currentHp, 0.001f,
                "T3: currentHp powinno byc 0 po smiertelnym trafieniu");
            Assert.AreEqual(2, eventOrder.Count,
                "T3: Oczekiwano dokladnie 2 zdarzenia (Damaged + Died)");
            Assert.AreEqual("Damaged", eventOrder[0],
                "T3: OnPlayerDamaged powinno poprzedzac OnPlayerDied");
            Assert.AreEqual("Died", eventOrder[1],
                "T3: OnPlayerDied powinno byc ostatnim zdarzeniem");
        }

        // --------------------------------------------------------
        // T4: source == null — LogWarning, obradzenia stosowane
        // --------------------------------------------------------
        [Test]
        public void T4_SourceNull_DamageAppliedWithWarning()
        {
            float initialHp = _stats.currentHp;
            int damageEventCount = 0;

            EventBus.Subscribe<OnPlayerDamaged>(_ => damageEventCount++);

            // source = null
            var info = new DamageInfo(20f, DamageType.Physical, false, Vector3.zero, null);

            // Unity LogWarning nie rzuca wyjatku; test sprawdza tylko ze obradzenia zostaly zadane
            ((IDamageable)_health).TakeDamage(info);

            Assert.AreEqual(initialHp - 20f, _stats.currentHp, 0.01f,
                "T4: Obradzenia powinny byc zastosowane mimo source==null");
            Assert.AreEqual(1, damageEventCount,
                "T4: OnPlayerDamaged powinien byc wyslany mimo source==null");
        }

        // --------------------------------------------------------
        // T8: TotalArmor=0.5, obradzenia 10 → reduced=5
        // --------------------------------------------------------
        [Test]
        public void T8_HalfArmor_DamageReduced()
        {
            _stats.baseArmor = 0.5f;
            float initialHp = _stats.currentHp;
            float receivedAmount = 0f;

            EventBus.Subscribe<OnPlayerDamaged>(e => receivedAmount = e.amount);

            var info = new DamageInfo(10f, DamageType.Physical, false, Vector3.zero,
                new GameObject("enemy"));
            ((IDamageable)_health).TakeDamage(info);

            Assert.AreEqual(5f, receivedAmount, 0.001f,
                "T8: 10 * (1-0.5) = 5 po redukcji pancerzem");
            Assert.AreEqual(initialHp - 5f, _stats.currentHp, 0.01f,
                "T8: HP powinno spasc o 5");
        }

        // --------------------------------------------------------
        // T9: TotalArmor=0.5, obradzenia 1 → reduced=0.5 → min 1
        // --------------------------------------------------------
        [Test]
        public void T9_HalfArmor_MinimumOneDamage()
        {
            _stats.baseArmor = 0.5f;
            float initialHp = _stats.currentHp;
            float receivedAmount = 0f;

            EventBus.Subscribe<OnPlayerDamaged>(e => receivedAmount = e.amount);

            var info = new DamageInfo(1f, DamageType.Physical, false, Vector3.zero,
                new GameObject("enemy"));
            ((IDamageable)_health).TakeDamage(info);

            Assert.AreEqual(1f, receivedAmount, 0.001f,
                "T9: 1 * (1-0.5) = 0.5 ale minimum to 1 — Mathf.Max(reduced, 1f)");
            Assert.AreEqual(initialHp - 1f, _stats.currentHp, 0.01f,
                "T9: HP powinno spasc o minimum 1");
        }

        // --------------------------------------------------------
        // T10: value=0 → reduced=0 → min 1, HP spada o 1
        // --------------------------------------------------------
        [Test]
        public void T10_ZeroDamage_MinimumOneDamageApplied()
        {
            float initialHp = _stats.currentHp;
            float receivedAmount = 0f;

            EventBus.Subscribe<OnPlayerDamaged>(e => receivedAmount = e.amount);

            var info = new DamageInfo(0f, DamageType.Physical, false, Vector3.zero,
                new GameObject("enemy"));
            ((IDamageable)_health).TakeDamage(info);

            Assert.AreEqual(1f, receivedAmount, 0.001f,
                "T10: 0 * (1-0) = 0 ale minimum to 1 — Mathf.Max(reduced, 1f)");
            Assert.AreEqual(initialHp - 1f, _stats.currentHp, 0.01f,
                "T10: HP powinno spasc o minimum 1 nawet przy value=0");
        }
    }
}

#endif // RATOCALYPSE_TESTS_ENABLED
