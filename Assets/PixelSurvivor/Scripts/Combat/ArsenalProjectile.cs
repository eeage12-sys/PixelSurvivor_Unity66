using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class ArsenalProjectile : MonoBehaviour
    {
        CampaignController c;Vector2 direction;float damage,speed,duration,age,radius,knock,trailClock;int remaining;
        SpriteRenderer visual;Color trailColor;
        WeaponPattern pattern;
        readonly HashSet<Enemy> hits=new HashSet<Enemy>();
        readonly HashSet<DestructibleProp> propHits=new HashSet<DestructibleProp>();
        public void Initialize(CampaignController owner,Vector2 heading,float power,float velocity,float life,int pierce,float hitRadius,WeaponPattern kind,float force,Color tint)
        {c=owner;direction=heading.normalized;damage=power;speed=velocity;duration=life;remaining=1+Mathf.Max(0,pierce);radius=hitRadius;pattern=kind;knock=force;trailColor=tint;visual=GetComponent<SpriteRenderer>();}
        void Update()
        {
            if(c==null || !c.Game.IsPlaying)return;
            age+=Time.deltaTime;
            bool returning=pattern==WeaponPattern.Scythe && age>=duration*0.5f;
            if(pattern!=WeaponPattern.Scythe && age>=duration){Destroy(gameObject);return;}
            if(pattern==WeaponPattern.Seeker)
            {var target=c.Arsenal.Nearest(transform.position);if(target!=null)direction=((Vector2)(target.transform.position-transform.position)).normalized;}
            Vector2 playerPosition=c.Game.Player.transform.position;
            if(returning)direction=(playerPosition-(Vector2)transform.position).normalized;
            if(pattern==WeaponPattern.Scythe)transform.Rotate(0,0,300*Time.deltaTime);
            else transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            Vector2 from=transform.position;
            float returnSpeed=returning?Mathf.Max(speed*1.25f,Vector2.Distance(from,playerPosition)*1.5f):speed;
            Vector2 to=returning?Vector2.MoveTowards(from,playerPosition,returnSpeed*Time.deltaTime):from+direction*speed*Time.deltaTime;
            trailClock+=Time.deltaTime;
            float trailInterval=pattern==WeaponPattern.Dagger || pattern==WeaponPattern.Bolt ? .045f : .08f;
            if(trailClock>=trailInterval)
            {trailClock=0;CombatVfx.Trail(c.Game,transform,visual,trailColor);}
            while(remaining>0)
            {
                Enemy enemy=null;DestructibleProp prop=null;float first=float.PositiveInfinity;
                foreach(var e in c.Game.Spawner.ActiveEnemies)
                    if(e!=null && e.IsAlive && !hits.Contains(e) && GameRules.SegmentCircle(from,to,e.transform.position,radius+e.Radius,out float f) && f<first){enemy=e;prop=null;first=f;}
                foreach(var p in c.Field.Props)
                    if(p!=null && !propHits.Contains(p) && GameRules.SegmentCircle(from,to,p.transform.position,radius+0.5f,out float f) && f<first){prop=p;enemy=null;first=f;}
                if(enemy==null && prop==null)break;
                if(enemy!=null){hits.Add(enemy);enemy.TakeDamage(c.RollDamage(damage),trailColor);enemy.Knockback(direction,knock);}
                else {propHits.Add(prop);prop.TakeDamage(damage);}
                if(pattern==WeaponPattern.Comet)
                {
                    Vector2 impact=Vector2.Lerp(from,to,first);
                    c.Arsenal.HurtCircle(impact,radius*5f,damage*.65f,knock,Vector2.zero,-1,trailColor);
                    PixelAttackFlash.Spawn(c,pattern,impact,direction,radius*5f,trailColor);
                }
                if(pattern==WeaponPattern.Bolt || pattern==WeaponPattern.Dagger)
                    PixelAttackFlash.Spawn(c,pattern,Vector2.Lerp(from,to,first),direction,.55f,trailColor);
                remaining--;
            }
            transform.position=to;
            if(returning && (to-playerPosition).sqrMagnitude<=0.18f*0.18f)
            {transform.position=playerPosition;Destroy(gameObject);}
            else if(pattern!=WeaponPattern.Scythe && remaining<=0)Destroy(gameObject);
        }
    }
}
