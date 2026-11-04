using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class ArsenalProjectile : MonoBehaviour
    {
        CampaignController c;Vector2 direction;float damage,speed,duration,age,radius,knock;int remaining;
        WeaponPattern pattern;
        readonly HashSet<Enemy> hits=new HashSet<Enemy>();
        readonly HashSet<DestructibleProp> propHits=new HashSet<DestructibleProp>();
        public void Initialize(CampaignController owner,Vector2 heading,float power,float velocity,float life,int pierce,float hitRadius,WeaponPattern kind,float force)
        {c=owner;direction=heading.normalized;damage=power;speed=velocity;duration=life;remaining=1+Mathf.Max(0,pierce);radius=hitRadius;pattern=kind;knock=force;}
        void Update()
        {
            if(c==null || !c.Game.IsPlaying)return;
            age+=Time.deltaTime;if(age>=duration){Destroy(gameObject);return;}
            if(pattern==WeaponPattern.Seeker)
            {var target=c.Arsenal.Nearest(transform.position);if(target!=null)direction=((Vector2)(target.transform.position-transform.position)).normalized;}
            if(pattern==WeaponPattern.Scythe && age>duration*0.5f)direction=((Vector2)(c.Game.Player.transform.position-transform.position)).normalized;
            Vector2 from=transform.position,to=from+direction*speed*Time.deltaTime;
            while(remaining>0)
            {
                Enemy enemy=null;DestructibleProp prop=null;float first=float.PositiveInfinity;
                foreach(var e in c.Game.Spawner.ActiveEnemies)
                    if(e!=null && e.IsAlive && !hits.Contains(e) && GameRules.SegmentCircle(from,to,e.transform.position,radius+e.Radius,out float f) && f<first){enemy=e;prop=null;first=f;}
                foreach(var p in c.Field.Props)
                    if(p!=null && !propHits.Contains(p) && GameRules.SegmentCircle(from,to,p.transform.position,radius+0.5f,out float f) && f<first){prop=p;enemy=null;first=f;}
                if(enemy==null && prop==null)break;
                if(enemy!=null){hits.Add(enemy);enemy.TakeDamage(c.RollDamage(damage));enemy.Knockback(direction,knock);}
                else {propHits.Add(prop);prop.TakeDamage(damage);}
                remaining--;
            }
            transform.position=to;
            if(remaining<=0)Destroy(gameObject);
        }
    }
}
