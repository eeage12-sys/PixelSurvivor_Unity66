using UnityEngine;
namespace PixelSurvivor
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Enemy : MonoBehaviour
    {
        [SerializeField, Min(1)] float hp = 20;
        [SerializeField, Min(0)] float moveSpeed = 2;
        [SerializeField, Min(0)] float contactDamage = 10;
        [SerializeField, Min(0.01f)] float radius = 0.38f;
        [SerializeField] ExperienceGem gemPrefab;
        GameManager game;
        EnemySpawner spawner;
        SpriteRenderer visual;
        float flash;
        public bool IsAlive { get; private set; }
        public float CurrentHP { get; private set; }
        public float Radius => radius;
        public ExperienceGem GemPrefab => gemPrefab;
        public void SetGem(ExperienceGem prefab) => gemPrefab = prefab;
        public void Initialize(GameManager owner, EnemySpawner source)
        { game = owner; spawner = source; CurrentHP = hp; IsAlive = true; visual = GetComponent<SpriteRenderer>(); }
        void Update()
        {
            if (game == null || !game.IsPlaying || !IsAlive) return;
            Vector2 offset = game.Player.transform.position - transform.position;
            Vector2 velocity = offset.normalized * moveSpeed;
            // Gentle separation, no physics-body jitter. Maximum 120 enemies by default.
            Vector2 separation = Vector2.zero;
            foreach (Enemy other in spawner.ActiveEnemies)
            {
                if (other == this || other == null || !other.IsAlive) continue;
                Vector2 away = transform.position - other.transform.position;
                float sqr = away.sqrMagnitude;
                if (sqr > 0.0001f && sqr < 0.49f) separation += away.normalized * (0.7f - Mathf.Sqrt(sqr));
            }
            velocity += Vector2.ClampMagnitude(separation * 2, moveSpeed * 0.65f);
            transform.position += (Vector3)(velocity * Time.deltaTime);
            float reach = radius + game.Player.Radius;
            if (((Vector2)(game.Player.transform.position - transform.position)).sqrMagnitude <= reach * reach)
                game.Health.TakeDamage(contactDamage);
            flash = Mathf.Max(0, flash - Time.deltaTime);
            visual.color = flash > 0 ? new Color(1, 0.5f, 0.4f) : Color.white;
        }
        public void TakeDamage(float amount)
        {
            if (!IsAlive || game == null || !game.IsPlaying || amount <= 0) return;
            CurrentHP = Mathf.Max(0, CurrentHP - amount); flash = 0.08f;
            if (CurrentHP > 0) return;
            IsAlive = false; // Guard against double kills/drop in the same frame.
            game.RegisterKill();
            if (gemPrefab != null)
            {
                Vector2 position = game.Bounds.Clamp(transform.position, 0.2f);
                ExperienceGem gem = Instantiate(gemPrefab, position, Quaternion.identity, game.RuntimeRoot);
                gem.Initialize(game);
            }
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        void OnDisable() { IsAlive = false; if (spawner != null) spawner.Unregister(this); }
    }
}
