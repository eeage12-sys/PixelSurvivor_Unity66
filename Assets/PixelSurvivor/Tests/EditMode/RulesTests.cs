using NUnit.Framework;
using UnityEngine;
namespace PixelSurvivor.Tests
{
    public sealed class RulesTests
    {
        [TestCase(1, 5)] [TestCase(2, 8)] [TestCase(10, 32)]
        public void ExperienceThreshold(int level, int expected) => Assert.AreEqual(expected, GameRules.RequiredExperience(level));
        [TestCase(0, 1.5f)] [TestCase(59.99f, 1.5f)] [TestCase(60, 1.2f)]
        [TestCase(120, 0.9f)] [TestCase(180, 0.7f)] [TestCase(240, 0.5f)] [TestCase(300, 0.5f)]
        public void SpawnStages(float elapsed, float expected)
        { Assert.AreEqual(expected, GameRules.SpawnInterval(elapsed, new[] { 1.5f, 1.2f, 0.9f, 0.7f, 0.5f }), 0.0001f); }
        [Test] public void DiagonalInputNeverExceedsUnitLength()
        { Assert.AreEqual(1, GameRules.Movement(Vector2.one).magnitude, 0.0001f); Assert.AreEqual(Vector2.zero, GameRules.Movement(Vector2.zero)); }
        [Test] public void FastProjectileHitsEvenWhenEndpointsMiss()
        {
            Assert.IsTrue(GameRules.SegmentCircle(new Vector2(-10, 0), new Vector2(10, 0), Vector2.zero, 0.5f, out float fraction));
            Assert.AreEqual(0.475f, fraction, 0.0001f);
            Assert.IsFalse(GameRules.SegmentCircle(new Vector2(-10, 2), new Vector2(10, 2), Vector2.zero, 0.5f, out _));
        }
        [Test] public void OverlapHitsAndBehindSegmentMisses()
        {
            Assert.IsTrue(GameRules.SegmentCircle(Vector2.zero, Vector2.zero, Vector2.zero, 1, out _));
            Assert.IsFalse(GameRules.SegmentCircle(Vector2.one, Vector2.one, Vector2.zero, 0.5f, out _));
            Assert.IsFalse(GameRules.SegmentCircle(new Vector2(2, 0), new Vector2(4, 0), Vector2.zero, 0.5f, out _));
        }
        [Test] public void UpgradesRespectAttackFloorAndNeverDuplicate()
        {
            var go = new GameObject("Stats test");
            try
            {
                var stats = go.AddComponent<PlayerStats>(); stats.ResetStats();
                stats.Apply(UpgradeKind.Damage); Assert.AreEqual(12, stats.Damage, 0.001f);
                stats.Apply(UpgradeKind.MoveSpeed); Assert.AreEqual(5.5f, stats.MoveSpeed, 0.001f);
                stats.Apply(UpgradeKind.MaxHP); Assert.AreEqual(120, stats.MaxHP, 0.001f);
                stats.Apply(UpgradeKind.ProjectileSpeed); Assert.AreEqual(9.6f, stats.ProjectileSpeed, 0.001f);
                for (int i = 0; i < 80; i++) stats.Apply(UpgradeKind.AttackSpeed);
                Assert.AreEqual(0.12f, stats.AttackInterval, 0.0001f);
                for (int i = 0; i < 100; i++)
                {
                    var roll = UpgradeSystem.Roll(stats);
                    Assert.AreEqual(3, roll.Length); Assert.AreEqual(3, new System.Collections.Generic.HashSet<UpgradeKind>(roll).Count);
                    CollectionAssert.DoesNotContain(roll, UpgradeKind.AttackSpeed);
                }
                stats.ResetStats(); Assert.AreEqual(100, stats.MaxHP); Assert.AreEqual(10, stats.Damage);
            }
            finally { Object.DestroyImmediate(go); }
        }
        [TestCase(0, "00:00")] [TestCase(59.9f, "00:59")] [TestCase(300, "05:00")]
        public void ClockFormatting(float elapsed, string expected) => Assert.AreEqual(expected, GameRules.Clock(elapsed));
    }
}
