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
        Color tint=Color.white;
        public bool IsElite {get;private set;}
        public bool IsBoss {get;private set;}
        public bool IsFinalBoss {get;private set;}
        public void ConfigureEncounter(float healthScale,float speedScale,float damageScale,bool elite,bool boss,bool final)
        {
            hp*=Mathf.Max(0.1f,healthScale);CurrentHP=hp;moveSpeed*=Mathf.Max(0.1f,speedScale);contactDamage*=Mathf.Max(0.1f,damageScale);
            IsElite=elite;IsBoss=boss;IsFinalBoss=final;
            float size=boss ? 2.8f : elite ? 1.4f : 1;
            transform.localScale=Vector3.one*size;radius*=size;
            tint=boss ? new Color(1,0.45f,0.45f) : elite ? new Color(0.45f,1,0.85f) : tint;
            visual.color=tint;
        }
        public void Knockback(Vector2 direction,float force)
        {
            if(!IsAlive)return;
            Vector2 from=transform.position;Vector2 to=from+direction.normalized*force*(IsBoss?0.1f:1);
            if(game.Campaign!=null)to=game.Campaign.Supports.ResolveMove(from,to,radius);
            transform.position=to;
        }
        public bool IsAlive { get; private set; }
        public float CurrentHP { get; private set; }
        public float Radius => radius;
        public float MaxHP => hp;
        public float MoveSpeed => moveSpeed;
        public float ContactDamage => contactDamage;
        public void ConfigureStats(float health, float speed, float damage, float hitRadius)
        { hp = Mathf.Max(1, health); moveSpeed = Mathf.Max(0, speed); contactDamage = Mathf.Max(0, damage); radius = Mathf.Max(0.01f, hitRadius); }
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
            Vector2 from=transform.position,to=from+velocity*Time.deltaTime;
            if(game.Campaign!=null)to=game.Campaign.Supports.ResolveMove(from,to,radius);
            transform.position=to;
            float reach = radius + game.Player.Radius;
            if (((Vector2)(game.Player.transform.position - transform.position)).sqrMagnitude <= reach * reach)
                game.Health.TakeDamage(contactDamage);
            flash = Mathf.Max(0, flash - Time.deltaTime);
            visual.color = flash > 0 ? new Color(1, 0.5f, 0.4f) : tint;
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
                if(game.Campaign!=null)gem.SetExperience(IsBoss?30:IsElite?8:1);
            }
            if(game.Campaign!=null)game.Campaign.Stage.EnemyDied(this,game.Bounds.Clamp(transform.position,0.2f));
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        void OnDisable() { IsAlive = false; if (spawner != null) spawner.Unregister(this); }
    }
}
