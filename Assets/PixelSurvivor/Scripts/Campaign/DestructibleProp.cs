using UnityEngine;
namespace PixelSurvivor
{
    public sealed class DestructibleProp : MonoBehaviour
    {
        CampaignController c;float hp;bool broken;
        SpriteRenderer visual;Color baseColor;Vector3 baseScale;float hitTime;
        public void Initialize(CampaignController owner){c=owner;hp=c.Data.propHP;visual=GetComponent<SpriteRenderer>();if(visual!=null)baseColor=visual.color;baseScale=transform.localScale;}
        public void TakeDamage(float amount)
        {
            if(c==null || !c.Game.IsPlaying || broken || amount<=0)return;
            hp-=amount;hitTime=.16f;PropHitEffect.Spawn(c,transform.position,hp<=0);if(hp>0)return;
            broken=true;c.Field.BreakProp(this,transform.position);gameObject.SetActive(false);Destroy(gameObject);
        }
        void Update()
        {
            if(visual==null)return;
            hitTime=Mathf.Max(0,hitTime-Time.deltaTime);
            bool flash=hitTime>0 && DisplayOptions.HitEffects;
            visual.color=flash?new Color(2,2,2,1):baseColor;
            transform.localScale=baseScale*(flash?1+.12f*Mathf.Sin(hitTime*70):1);
        }
        void OnDisable(){if(c!=null)c.Field.Props.Remove(this);}
    }
}
