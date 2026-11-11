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
            get {var a=c.Game.Player.GetComponent<PlayerSpriteAnimator>();return a!=null?a.AimDirection:Vector2.down;}
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
            if(d.pattern!=WeaponPattern.Storm)
                PixelAttackFlash.Spawn(c,d.pattern,
                    d.pattern==WeaponPattern.Whip?origin+direction*area*.45f:origin,direction,area,d.color);
            switch(d.pattern)
            {
                case WeaponPattern.Whip:
                    HurtCircle(origin,area,damage,knock,direction,0.15f,d.color);
                    Zone(origin+direction*area*0.45f,area*0.65f,0,0.18f,d.color,false,false,0);
                    if(count>1)HurtCircle(origin,area*0.8f,damage*0.6f,knock,-direction,0.15f,d.color);
                    break;
                case WeaponPattern.Orbit:
                    Zone(origin,area,damage,Mathf.Max(0.4f,duration),d.color,true,true,knock);break;
                case WeaponPattern.Storm:
                    for(int i=0;i<count;i++)
                    {
                        Vector2 spot=i==0?target:target+Random.insideUnitCircle*area*1.5f;
                        HurtCircle(spot,area,damage,knock,Vector2.zero,-1,d.color);
                        PixelAttackFlash.Spawn(c,d.pattern,spot,Vector2.down,area,d.color);
                        Zone(spot,area,0,0.3f,d.color,false,false,0);
                    }break;
                case WeaponPattern.Nova:
                    HurtCircle(origin,area,damage,knock,Vector2.zero,-1,d.color);Zone(origin,area,0,0.5f,d.color,false,false,0);break;
                case WeaponPattern.Cross:
                    for(int i=0;i<Mathf.Max(4,count);i++)
                    {
                        float angle=2*Mathf.PI*i/Mathf.Max(4,count);
                        Shoot(origin,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),damage,d.projectileSpeed,Mathf.Max(1,duration),pierce,area,d.color,d.pattern,knock);
                    }break;
                case WeaponPattern.Trap:
                    PixelAttackFlash.Spawn(c,d.pattern,target,Vector2.up,area,d.color);
                    Zone(target,area,damage,Mathf.Max(1,duration),d.color,false,false,knock);break;
                case WeaponPattern.PrismRay:
                    int rays=Mathf.Min(3,Mathf.Max(1,count));
                    for(int ray=0;ray<rays;ray++)
                    {
                        float angle=(ray-(rays-1)*.5f)*15*Mathf.Deg2Rad;
                        Vector2 heading=new Vector2(direction.x*Mathf.Cos(angle)-direction.y*Mathf.Sin(angle),
                            direction.x*Mathf.Sin(angle)+direction.y*Mathf.Cos(angle));
                        for(int step=1;step<=4;step++)
                        {
                            Vector2 point=origin+heading*(step*area*.55f);
                            HurtCircle(point,area*.38f,damage*.65f,knock,Vector2.zero,-1,d.color);
                            PixelAttackFlash.Spawn(c,d.pattern,point,heading,area*.4f,d.color);
                        }
                    }
                    break;
                case WeaponPattern.Bloom:
                    HurtCircle(target,area*.85f,damage,knock,Vector2.zero,-1,d.color);
                    PixelAttackFlash.Spawn(c,d.pattern,target,Vector2.up,area,d.color);
                    int petals=Mathf.Max(4,count+3);
                    for(int i=0;i<petals;i++)
                    {
                        float angle=2*Mathf.PI*i/petals;
                        Shoot(target,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),damage*.45f,d.projectileSpeed,
                            Mathf.Max(.35f,duration),pierce,area*.65f,d.color,d.pattern,knock);
                    }
                    break;
                case WeaponPattern.GravityWell:
                    HurtCircle(target,area*.55f,damage*.55f,0,Vector2.zero,-1,d.color);
                    PixelAttackFlash.Spawn(c,d.pattern,target,Vector2.up,area,d.color);
                    Zone(target,area,damage*.32f,Mathf.Max(1f,duration),d.color,false,false,-Mathf.Max(.15f,knock));
                    break;
                case WeaponPattern.Comet:
                    for(int i=0;i<count;i++)
                    {
                        float angle=(i-(count-1)*.5f)*10*Mathf.Deg2Rad;
                        Vector2 heading=new Vector2(direction.x*Mathf.Cos(angle)-direction.y*Mathf.Sin(angle),
                            direction.x*Mathf.Sin(angle)+direction.y*Mathf.Cos(angle));
                        Shoot(origin,heading,damage,d.projectileSpeed,Mathf.Max(.5f,duration),0,area,d.color,d.pattern,knock);
                    }
                    break;
                default:
                    if(d.pattern==WeaponPattern.Dagger || d.pattern==WeaponPattern.Spear)direction=Facing;
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
            var sr=go.AddComponent<SpriteRenderer>();
            var pixel=PixelCombatArt.Projectile(pattern);
            sr.sprite=pixel!=null?pixel:c.Data.projectileSprite;
            sr.color=pixel!=null?Color.white:color;sr.sortingOrder=14;
            float readability=pattern==WeaponPattern.Dagger?1.65f:pattern==WeaponPattern.Bolt?1.25f:1f;
            go.transform.localScale=Vector3.one*Mathf.Clamp(area*readability,0.65f,3.5f);
            go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            go.AddComponent<ArsenalProjectile>().Initialize(c,direction,damage,speed,duration,pierce,0.14f*Mathf.Clamp(area,0.5f,3),pattern,knock,color);
        }
        public void HurtCircle(Vector2 center,float radius,float damage,float knock,Vector2 forward,float dotLimit,Color? hitColor=null)
        {
            if(!c.Game.IsPlaying)return;
            var enemies=new List<Enemy>(c.Game.Spawner.ActiveEnemies);
            foreach(var e in enemies)
            {
                if(e==null || !e.IsAlive)continue;Vector2 delta=(Vector2)e.transform.position-center;
                if(delta.magnitude>radius+e.Radius || (forward!=Vector2.zero && Vector2.Dot(delta.normalized,forward)<dotLimit))continue;
                e.TakeDamage(c.RollDamage(damage),hitColor);e.Knockback(delta,knock);
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
