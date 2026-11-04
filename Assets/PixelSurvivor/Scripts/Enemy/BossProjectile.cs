using UnityEngine;
namespace PixelSurvivor
{
    public sealed class BossProjectile : MonoBehaviour
    {
        CampaignController c;Enemy owner;Vector2 velocity;float damage,life;
        public void Initialize(CampaignController campaign,Enemy enemy,Vector2 direction,float speed,float power,float duration)
        {c=campaign;owner=enemy;velocity=direction.normalized*speed;damage=power;life=duration;}
        void Update()
        {
            if(c==null || owner==null || !owner.IsAlive){Destroy(gameObject);return;}
            if(!c.Game.IsPlaying)return;
            Vector2 from=transform.position,to=from+velocity*Time.deltaTime,point=c.Game.Player.transform.position;
            Vector2 delta=to-from;float t=delta.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(point-from,delta)/delta.sqrMagnitude):0;
            if(Vector2.Distance(from+t*delta,point)<=c.Game.Player.Radius+0.12f)
            {c.Game.Health.TakeDamage(damage);Destroy(gameObject);return;}
            transform.position=to;life-=Time.deltaTime;if(life<=0)Destroy(gameObject);
        }
    }
}
