using UnityEngine;
namespace PixelSurvivor
{
    [RequireComponent(typeof(PlayerStats))]
    public sealed class AutoAttack : MonoBehaviour
    {
        [SerializeField] Projectile projectilePrefab;
        [SerializeField, Min(1)] float range = 24;
        GameManager game;
        PlayerStats stats;
        float cooldown;
        public Projectile ProjectilePrefab => projectilePrefab;
        public void SetProjectile(Projectile prefab) => projectilePrefab = prefab;
        public void Initialize(GameManager owner) { game = owner; stats = GetComponent<PlayerStats>(); }
        public void ResetWeapon() => cooldown = 0;
        void Update()
        {
            if (game == null || !game.IsPlaying || projectilePrefab == null) return;
            cooldown -= Time.deltaTime;
            if (cooldown > 0) return;
            Enemy nearest = null;
            float distance = range * range;
            foreach (Enemy enemy in game.Spawner.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                Vector3 v = game.View.WorldToViewportPoint(enemy.transform.position);
                if (v.z <= 0 || v.x < 0 || v.x > 1 || v.y < 0 || v.y > 1) continue;
                float sqr = ((Vector2)(enemy.transform.position - transform.position)).sqrMagnitude;
                if (sqr < distance) { nearest = enemy; distance = sqr; }
            }
            if (nearest == null) { cooldown = 0; return; }
            Vector2 direction = ((Vector2)(nearest.transform.position - transform.position)).normalized;
            if (direction.sqrMagnitude < 0.001f) direction = Vector2.up;
            Projectile shot = Instantiate(projectilePrefab, transform.position, Quaternion.identity, game.RuntimeRoot);
            shot.Initialize(game, direction, stats.Damage, stats.ProjectileSpeed);
            cooldown = stats.AttackInterval;
        }
    }
}
