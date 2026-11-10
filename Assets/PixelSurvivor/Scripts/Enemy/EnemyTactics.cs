using UnityEngine;

namespace PixelSurvivor
{
    public enum EnemyTacticKind { Charger, EmberCaster }

    // Distinct attacks for the two additional normal monsters. Bosses and elites keep their own behavior.
    public sealed class EnemyTactics : MonoBehaviour
    {
        [SerializeField] EnemyTacticKind kind;
        GameManager game;
        Enemy owner;
        SpriteRenderer warning;
        Vector2 lockedDirection;
        float timer;
        int phase; // 0: approach/cooldown, 1: warning, 2: charge

        public EnemyTacticKind Kind => kind;
        public bool IsTelegraphing => phase==1;
        public bool IsCommitting => phase!=0;
        public void Configure(EnemyTacticKind value) => kind=value;

        public void Initialize(GameManager source,Enemy enemy)
        {
            game=source;owner=enemy;phase=0;
            timer=kind==EnemyTacticKind.Charger?Random.Range(.4f,1.1f):Random.Range(.8f,1.6f);
            if(game.Campaign==null || game.Campaign.Data.ringSprite==null)return;
            var cue=new GameObject(kind==EnemyTacticKind.Charger?"Charge warning":"Ember warning");
            cue.transform.SetParent(transform,false);
            cue.transform.localScale=Vector3.one*(kind==EnemyTacticKind.Charger?1.7f:1.35f);
            warning=cue.AddComponent<SpriteRenderer>();
            warning.sprite=game.Campaign.Data.ringSprite;
            warning.sortingOrder=6;
            warning.enabled=false;
        }

        public Vector2 Velocity(Vector2 toPlayer,float moveSpeed,float damage,float dt)
        {
            Vector2 pursuit=toPlayer.normalized*moveSpeed;
            if(game==null || game.Campaign==null || owner.IsElite || owner.IsBoss)return pursuit;
            timer-=dt;
            if(phase==1)
            {
                if(warning!=null)
                {
                    float progress=kind==EnemyTacticKind.Charger?1-timer/.55f:1-timer/.65f;
                    warning.color=kind==EnemyTacticKind.Charger?
                        new Color(.65f,1f,.38f,Mathf.Lerp(.35f,.95f,progress)):
                        new Color(1f,.38f,.12f,Mathf.Lerp(.35f,.95f,progress));
                }
                if(timer>0)return Vector2.zero;
                if(warning!=null)warning.enabled=false;
                if(kind==EnemyTacticKind.EmberCaster)
                {FireEmber(damage);phase=0;timer=2.4f;return Vector2.zero;}
                phase=2;timer=.42f;
            }
            if(phase==2)
            {
                if(timer>0)return lockedDirection*Mathf.Max(7.5f,moveSpeed*4f);
                phase=0;timer=2.2f;
            }
            if(timer<=0 && toPlayer.sqrMagnitude<(kind==EnemyTacticKind.Charger?49f:81f))
            {
                phase=1;timer=kind==EnemyTacticKind.Charger?.55f:.65f;
                lockedDirection=toPlayer.normalized;
                if(warning!=null)warning.enabled=true;
                return Vector2.zero;
            }
            if(kind==EnemyTacticKind.Charger)return pursuit;
            float distance=toPlayer.magnitude;
            if(distance>6.2f)return pursuit;
            if(distance<3.8f)return -pursuit*.8f;
            Vector2 side=new Vector2(-toPlayer.y,toPlayer.x).normalized;
            return side*moveSpeed*.4f;
        }

        void FireEmber(float contactDamage)
        {
            if(!game.IsPlaying || owner==null || !owner.IsAlive)return;
            var campaign=game.Campaign;
            var go=new GameObject("Ember shot");
            go.transform.SetParent(game.RuntimeRoot,false);
            go.transform.position=transform.position;
            go.transform.localScale=Vector3.one*1.25f;
            var sr=go.AddComponent<SpriteRenderer>();
            sr.sprite=PixelCombatArt.Projectile(WeaponPattern.Seeker)??campaign.Data.projectileSprite;
            sr.color=new Color(1f,.65f,.35f);
            sr.sortingOrder=15;
            go.AddComponent<BossProjectile>().Initialize(campaign,owner,lockedDirection,7f,contactDamage*.8f,3f);
        }
    }
}
