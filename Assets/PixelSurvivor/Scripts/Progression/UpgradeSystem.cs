using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public enum UpgradeKind { Damage, AttackSpeed, MoveSpeed, MaxHP, ProjectileSpeed }
    public static class UpgradeSystem
    {
        public static UpgradeKind[] Roll(PlayerStats stats)
        {
            var pool = new List<UpgradeKind> { UpgradeKind.Damage, UpgradeKind.MoveSpeed, UpgradeKind.MaxHP, UpgradeKind.ProjectileSpeed };
            if (stats.AttackInterval > GameRules.MinimumAttackInterval + 0.0001f) pool.Add(UpgradeKind.AttackSpeed);
            // Fisher-Yates; choose without replacement. At least four candidates always exist.
            for (int i = pool.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); (pool[i], pool[j]) = (pool[j], pool[i]); }
            return new[] { pool[0], pool[1], pool[2] };
        }
        public static string Title(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage: return "POWER CORE";
                case UpgradeKind.AttackSpeed: return "RAPID CIRCUIT";
                case UpgradeKind.MoveSpeed: return "LIGHT STEPS";
                case UpgradeKind.MaxHP: return "VITAL SHELL";
                default: return "SWIFT BOLT";
            }
        }
        public static string Description(UpgradeKind kind, PlayerStats stats)
        {
            switch (kind)
            {
                case UpgradeKind.Damage: return $"Damage +20%\n{stats.Damage:0.#} > {stats.Damage * 1.2f:0.#}";
                case UpgradeKind.AttackSpeed: return $"Attack interval -15%\n{stats.AttackInterval:0.00}s > {Mathf.Max(GameRules.MinimumAttackInterval, stats.AttackInterval * 0.85f):0.00}s";
                case UpgradeKind.MoveSpeed: return $"Move speed +10%\n{stats.MoveSpeed:0.#} > {stats.MoveSpeed * 1.1f:0.#}";
                case UpgradeKind.MaxHP: return $"Max HP +20 / Heal 20\n{stats.MaxHP:0} > {stats.MaxHP + 20:0}";
                default: return $"Projectile speed +20%\n{stats.ProjectileSpeed:0.#} > {stats.ProjectileSpeed * 1.2f:0.#}";
            }
        }
    }
}
