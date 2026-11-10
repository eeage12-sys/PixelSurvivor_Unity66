using UnityEngine;
namespace PixelSurvivor
{
    [RequireComponent(typeof(PlayerStats), typeof(SpriteRenderer))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] float invulnerabilitySeconds = 0.65f;
        GameManager game;
        PlayerStats stats;
        SpriteRenderer visual;
        float invulnerability;
        public float CurrentHP { get; private set; }
        public void Initialize(GameManager owner)
        { game = owner; stats = GetComponent<PlayerStats>(); visual = GetComponent<SpriteRenderer>(); }
        public void ResetHealth() { CurrentHP = stats.MaxHP; invulnerability = 0; visual.color = Color.white; }
        public void Heal(float amount) { CurrentHP = Mathf.Min(stats.MaxHP, CurrentHP + Mathf.Max(0, amount)); }
        public void TakeDamage(float damage)
        {
            if (game == null || !game.IsPlaying || invulnerability > 0 || damage <= 0) return;
            if(game.Campaign!=null && game.Campaign.Stage.DoomAppeared)return;
            if(game.Campaign!=null) damage=game.Campaign.IncomingDamage(damage);
            if(damage<=0) return;
            CurrentHP = Mathf.Max(0, CurrentHP - damage);
            CombatVfx.PlayerHurt(game,transform.position);
            var feedback=GetComponent<HitFeedbackVisual>();
            if(feedback==null)feedback=gameObject.AddComponent<HitFeedbackVisual>();
            feedback.Play(.22f,.11f,new Color(1f,.18f,.22f));
            CombatAudio.Hurt();
            invulnerability = invulnerabilitySeconds;
            if (CurrentHP <= 0) game.EndRun(false);
        }
        void Update()
        {
            if (game == null || !game.IsPlaying) return;
            invulnerability = Mathf.Max(0, invulnerability - Time.deltaTime);
            visual.color = invulnerability > 0 && Mathf.FloorToInt(invulnerability * 18) % 2 == 0
                ? new Color(1, 0.45f, 0.5f, 0.6f) : (game.Campaign!=null ? game.Campaign.Character.tint : Color.white);
        }
    }
}
