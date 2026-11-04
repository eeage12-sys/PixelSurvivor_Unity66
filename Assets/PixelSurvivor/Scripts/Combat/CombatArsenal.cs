using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class CombatArsenal
    {
        readonly CampaignController c;
        readonly Dictionary<WeaponData,float> cooldowns=new Dictionary<WeaponData,float>();
        public CombatArsenal(CampaignController owner){c=owner;}
        public void Reset(){cooldowns.Clear();}
        public Vector2 Facing
        {
            get {var a=c.Game.Player.GetComponent<PlayerSpriteAnimator>();int f=a!=null?a.Facing:0;
                return f==1?Vector2.up:f==2?Vector2.left:f==3?Vector2.right:Vector2.down;}
        }
        public Enemy Nearest(Vector2 origin,float range=24)
        {
            Enemy result=null;float best=range*range;
            foreach(var e in c.Game.Spawner.ActiveEnemies)
            {
                if(e==null || !e.IsAlive)continue;Vector3 v=c.Game.View.WorldToViewportPoint(e.transform.position);
                if(v.x<0 || v.x>1 || v.y<0 || v.y>1)continue;
                float d=((Vector2)e.transform.position-origin).sqrMagnitude;
                if(d<best){best=d;result=e;}
            }
            return result;
        }
        public bool Target(Vector2 origin,out Vector2 target)
        {
            var e=Nearest(origin);if(e!=null){target=e.transform.position;return true;}
            var prop=c.Field.NearestProp(origin,14);if(prop!=null){target=prop.transform.position;return true;}
            target=origin+Facing;return false;
        }
        public void Tick(float dt)
        {
            foreach(var state in c.Equipment.Weapons)
            {
                float remaining;cooldowns.TryGetValue(state.data,out remaining);remaining-=dt;
                if(remaining>0){cooldowns[state.data]=remaining;continue;}
                Vector2 origin=c.Game.Player.transform.position,target;
                if(!Target(origin,out target)){cooldowns[state.data]=0;continue;}
                Fire(state,origin,target);
                var d=state.data;
                cooldowns[d]=Mathf.Max(0.12f,d.cooldown*Mathf.Max(0.2f,1-(state.level-1)*d.cooldownPerLevel)*c.CooldownFactor*(state.evolved?d.evolvedCooldown:1));
            }
        }
        void Fire(WeaponState state,Vector2 origin,Vector2 target)
        {
            var d=state.data;int level=state.level-1;
            float damage=d.damage*(1+level*d.damagePerLevel)*c.DamageFactor*(state.evolved?d.evolvedDamage:1);
            float area=d.area*(1+level*d.areaPerLevel)*c.AreaFactor*(state.evolved?d.evolvedArea:1);
            float duration=d.duration*c.DurationFactor;
            int count=d.projectileCount+c.ExtraProjectiles+(d.extraProjectileEvery>0?level/d.extraProjectileEvery:0)+(state.evolved?d.evolvedExtraProjectiles:0);
            int pierce=d.penetration+(state.evolved?d.evolvedPenetration:0);
            float knock=d.knockback*(1+c.Bonus(StatKind.Knockback));
            Vector2 direction=(target-origin).normalized;if(direction.sqrMagnitude<0.001f)direction=Facing;
            switch(d.pattern)
            {
                case WeaponPattern.Whip:
                    HurtCircle(origin,area,damage,knock,direction,0.15f);
                    Zone(origin+direction*area*0.45f,area*0.65f,0,0.18f,d.color,false,false,0);
                    if(count>1)HurtCircle(origin,area*0.8f,damage*0.6f,knock,-direction,0.15f);
                    break;
                case WeaponPattern.Orbit:
                    Zone(origin,area,damage,Mathf.Max(0.4f,duration),d.color,true,true,knock);break;
                case WeaponPattern.Storm:
                    for(int i=0;i<count;i++)
                    {
                        Vector2 spot=i==0?target:target+Random.insideUnitCircle*area*1.5f;
                        HurtCircle(spot,area,damage,knock,Vector2.zero,-1);Zone(spot,area,0,0.3f,d.color,false,false,0);
                    }break;
                case WeaponPattern.Nova:
                    HurtCircle(origin,area,damage,knock,Vector2.zero,-1);Zone(origin,area,0,0.5f,d.color,false,false,0);break;
                default:
                    if(d.pattern==WeaponPattern.Dagger)direction=Facing;
                    for(int i=0;i<count;i++)
                    {
                        float angle=(i-(count-1)*0.5f)*12*Mathf.Deg2Rad;
                        Vector2 heading=new Vector2(direction.x*Mathf.Cos(angle)-direction.y*Mathf.Sin(angle),direction.x*Mathf.Sin(angle)+direction.y*Mathf.Cos(angle));
                        Shoot(origin,heading,damage,d.projectileSpeed,Mathf.Max(1,duration),pierce,area,d.color,d.pattern,knock);
                    }break;
            }
        }
        public void Shoot(Vector2 origin,Vector2 direction,float damage,float speed,float duration,int pierce,float area,Color color,WeaponPattern pattern,float knock)
        {
            var go=new GameObject(pattern.ToString());go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=origin;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.projectileSprite;sr.color=color;sr.sortingOrder=14;
            go.transform.localScale=Vector3.one*Mathf.Clamp(area,0.5f,3);
            go.AddComponent<ArsenalProjectile>().Initialize(c,direction,damage,speed,duration,pierce,0.14f*Mathf.Clamp(area,0.5f,3),pattern,knock);
        }
        public void HurtCircle(Vector2 center,float radius,float damage,float knock,Vector2 forward,float dotLimit)
        {
            if(!c.Game.IsPlaying)return;
            var enemies=new List<Enemy>(c.Game.Spawner.ActiveEnemies);
            foreach(var e in enemies)
            {
                if(e==null || !e.IsAlive)continue;Vector2 delta=(Vector2)e.transform.position-center;
                if(delta.magnitude>radius+e.Radius || (forward!=Vector2.zero && Vector2.Dot(delta.normalized,forward)<dotLimit))continue;
                e.TakeDamage(c.RollDamage(damage));e.Knockback(delta,knock);
            }
            foreach(var p in c.Field.Props.ToArray())
            {
                if(p==null)continue;Vector2 delta=(Vector2)p.transform.position-center;
                if(delta.magnitude<=radius+0.5f && (forward==Vector2.zero || Vector2.Dot(delta.normalized,forward)>=dotLimit))p.TakeDamage(damage);
            }
        }
        public void Zone(Vector2 center,float radius,float damage,float duration,Color color,bool follow,bool ring,float knock)
        {
            var go=new GameObject(ring?"Orbit field":"Attack flash");go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=center;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.ringSprite;sr.color=color;sr.sortingOrder=4;
            go.AddComponent<WeaponZone>().Initialize(c,radius,damage,duration,follow,ring,knock);
        }
    }
}
