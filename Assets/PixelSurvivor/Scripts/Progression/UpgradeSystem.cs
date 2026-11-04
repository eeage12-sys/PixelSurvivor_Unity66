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
                case UpgradeKind.Damage: return "힘의 핵";
                case UpgradeKind.AttackSpeed: return "고속 회로";
                case UpgradeKind.MoveSpeed: return "가벼운 발걸음";
                case UpgradeKind.MaxHP: return "생명의 껍질";
                default: return "신속한 볼트";
            }
        }
        public static string Description(UpgradeKind kind, PlayerStats stats)
        {
            switch (kind)
            {
                case UpgradeKind.Damage: return $"공격력 +20%\n{stats.Damage:0.#} > {stats.Damage * 1.2f:0.#}";
                case UpgradeKind.AttackSpeed: return $"공격 간격 -15%\n{stats.AttackInterval:0.00}s > {Mathf.Max(GameRules.MinimumAttackInterval, stats.AttackInterval * 0.85f):0.00}s";
                case UpgradeKind.MoveSpeed: return $"이동속도 +10%\n{stats.MoveSpeed:0.#} > {stats.MoveSpeed * 1.1f:0.#}";
                case UpgradeKind.MaxHP: return $"최대 체력 +20 / 체력 20 회복\n{stats.MaxHP:0} > {stats.MaxHP + 20:0}";
                default: return $"투사체 속도 +20%\n{stats.ProjectileSpeed:0.#} > {stats.ProjectileSpeed * 1.2f:0.#}";
            }
        }
    }
}
