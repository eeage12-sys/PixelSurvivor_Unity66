using UnityEngine;
namespace PixelSurvivor
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] float hitRadius = 0.13f;
        [SerializeField, Min(0.1f)] float lifetime = 4;
        GameManager game;
        Vector2 direction;
        float damage, speed, age;
        bool spent;
        public void Initialize(GameManager owner, Vector2 heading, float power, float velocity)
        { game = owner; direction = heading.normalized; damage = power; speed = velocity; age = 0; spent = false; }
        void Update()
        {
            if (game == null || !game.IsPlaying || spent) return;
            float dt = Time.deltaTime;
            Vector2 from = transform.position, to = from + direction * speed * dt;
            Enemy hit = null;
            float first = float.PositiveInfinity;
            foreach (Enemy enemy in game.Spawner.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                if (GameRules.SegmentCircle(from, to, enemy.transform.position, hitRadius + enemy.Radius, out float fraction) && fraction < first)
                { first = fraction; hit = enemy; }
            }
            if (hit != null)
            {
                spent = true;
                hit.TakeDamage(damage);
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            transform.position = to;
            age += dt;
            if (age >= lifetime) { spent = true; Destroy(gameObject); }
        }
    }
}
