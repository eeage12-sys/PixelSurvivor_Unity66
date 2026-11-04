using UnityEngine;
namespace PixelSurvivor
{
    public sealed class PlayerStats : MonoBehaviour
    {
        [Header("Starting values (reset each run)")]
        [SerializeField, Min(1)] float maxHP = 100;
        [SerializeField, Min(0.1f)] float moveSpeed = 5;
        [SerializeField, Min(0.1f)] float damage = 10;
        [SerializeField, Min(GameRules.MinimumAttackInterval)] float attackInterval = 0.8f;
        [SerializeField, Min(0.1f)] float projectileSpeed = 8;
        public float MaxHP { get; private set; }
        public float MoveSpeed { get; private set; }
        public float Damage { get; private set; }
        public float AttackInterval { get; private set; }
        public float ProjectileSpeed { get; private set; }
        public void ResetStats()
        {
            MaxHP = Mathf.Max(1, maxHP); MoveSpeed = Mathf.Max(0.1f, moveSpeed);
            Damage = Mathf.Max(0.1f, damage);
            AttackInterval = Mathf.Max(GameRules.MinimumAttackInterval, attackInterval);
            ProjectileSpeed = Mathf.Max(0.1f, projectileSpeed);
        }
        public void SetRunValues(float health,float movement,float power,float interval,float projectile)
        { MaxHP=Mathf.Max(1,health);MoveSpeed=Mathf.Max(0.1f,movement);Damage=Mathf.Max(0.1f,power);AttackInterval=Mathf.Max(GameRules.MinimumAttackInterval,interval);ProjectileSpeed=Mathf.Max(0.1f,projectile); }
        public void Apply(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage: Damage *= 1.2f; break;
                case UpgradeKind.AttackSpeed: AttackInterval = Mathf.Max(GameRules.MinimumAttackInterval, AttackInterval * 0.85f); break;
                case UpgradeKind.MoveSpeed: MoveSpeed *= 1.1f; break;
                case UpgradeKind.MaxHP: MaxHP += 20; break;
                case UpgradeKind.ProjectileSpeed: ProjectileSpeed *= 1.2f; break;
            }
        }
    }
}
