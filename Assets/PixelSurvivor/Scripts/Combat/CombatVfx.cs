using System.Collections.Generic;
using UnityEngine;

namespace PixelSurvivor
{
    // Short pixel bursts make hits readable without obscuring a crowded arena.
    public static class CombatVfx
    {
        static int frame=-1, hitsThisFrame, trailsThisFrame;
        static readonly Stack<CombatVfxInstance> hitPool=new Stack<CombatVfxInstance>();
        static readonly Stack<CombatVfxInstance> bloodPool=new Stack<CombatVfxInstance>();

        static CombatVfxInstance Rent(Stack<CombatVfxInstance> pool,int kind,GameManager game,string name,Vector3 position)
        {
            CombatVfxInstance effect=null;
            while(pool.Count>0 && effect==null)effect=pool.Pop();
            if(effect==null)
            {
                var root=new GameObject(name);
                effect=root.AddComponent<CombatVfxInstance>();
            }
            effect.gameObject.name=name;
            effect.transform.SetParent(game.RuntimeRoot,false);
            effect.transform.position=position;
            effect.transform.localScale=Vector3.one;
            effect.gameObject.SetActive(true);
            effect.PoolKind=kind;
            return effect;
        }

        internal static void Recycle(CombatVfxInstance effect,int kind)
        {
            effect.gameObject.SetActive(false);
            effect.transform.SetParent(null,false);
            var pool=kind==1?bloodPool:hitPool;
            if(pool.Count<(kind==1?4:32))pool.Push(effect);
            else Object.Destroy(effect.gameObject);
        }

        static void CountFrame()
        {
            if(frame==Time.frameCount)return;
            frame=Time.frameCount;hitsThisFrame=0;trailsThisFrame=0;
        }

        public static void Hit(GameManager game,Vector3 position,float radius,bool defeated,bool special,Color? weaponColor=null)
        {
            if(game==null || game.Campaign==null || game.Campaign.Data.whiteSprite==null)return;
            CountFrame();
            if(!defeated && hitsThisFrame>=2)return;
            if(defeated && hitsThisFrame>=4)return;
            hitsThisFrame++;
            var effect=Rent(hitPool,0,game,defeated?"Defeat burst":"Hit burst",position);
            Color tint=weaponColor.HasValue?Color.Lerp(weaponColor.Value,Color.white,defeated ? .28f : .12f):
                defeated?new Color(1f,.72f,.26f):special?
                new Color(.45f,1f,.88f):new Color(1f,.93f,.62f);
            effect.SetupBurst(game.Campaign.Data.whiteSprite,game.Campaign.Data.ringSprite,
                tint,Mathf.Clamp(radius,0.4f,1.5f),defeated);
        }

        public static void Trail(GameManager game,Vector3 position,Color tint)
        {
            if(game==null || game.Campaign==null || game.Campaign.Data.whiteSprite==null)return;
            CountFrame();if(trailsThisFrame>=8)return;
            trailsThisFrame++;
            var root=new GameObject("Projectile afterglow");
            root.transform.SetParent(game.RuntimeRoot,false);root.transform.position=position;
            var effect=root.AddComponent<CombatVfxInstance>();
            effect.SetupTrail(game.Campaign.Data.whiteSprite,tint);
        }

        public static void PlayerHurt(GameManager game,Vector3 position)
        {
            if(game==null || game.Campaign==null || game.Campaign.Data.whiteSprite==null)return;
            Rent(bloodPool,1,game,"Player blood flecks",position).SetupBlood(game.Campaign.Data.whiteSprite);
        }

        public static void Trail(GameManager game,Transform source,SpriteRenderer visual,Color tint)
        {
            if(game==null || visual==null || visual.sprite==null)return;
            CountFrame();if(trailsThisFrame>=12)return;
            trailsThisFrame++;
            var root=new GameObject("Projectile silhouette");
            root.transform.SetParent(game.RuntimeRoot,false);
            root.transform.SetPositionAndRotation(source.position,source.rotation);
            var effect=root.AddComponent<CombatVfxInstance>();
            effect.SetupSilhouette(visual.sprite,tint,source.localScale.x);
        }
    }

    public sealed class CombatVfxInstance : MonoBehaviour
    {
        SpriteRenderer[] sparks;
        Vector3[] velocities;
        Color[] bloodColors;
        SpriteRenderer ring;
        Color tint;
        float age,lifetime,ringSize;
        bool trail,blood;
        internal int PoolKind=-1;

        public void SetupBurst(Sprite pixel,Sprite ringSprite,Color color,float size,bool defeated)
        {
            age=0;trail=false;blood=false;tint=color;lifetime=defeated?0.38f:0.26f;
            int count=defeated?10:7;
            if(sparks==null){sparks=new SpriteRenderer[count];velocities=new Vector3[count];}
            else if(sparks.Length<count)
            {System.Array.Resize(ref sparks,count);System.Array.Resize(ref velocities,count);}
            for(int i=0;i<count;i++)
            {
                var sprite=sparks[i];
                if(sprite==null)
                {
                    var part=new GameObject("Pixel spark");part.transform.SetParent(transform,false);
                    sprite=part.AddComponent<SpriteRenderer>();sparks[i]=sprite;
                }
                sprite.gameObject.SetActive(true);sprite.sprite=pixel;sprite.color=tint;sprite.sortingOrder=18;
                sprite.transform.localPosition=Vector3.zero;
                var partTransform=sprite.transform;
                partTransform.localScale=Vector3.one*(defeated?5:4);
                float angle=(i/(float)count)*Mathf.PI*2+Time.time*7;
                velocities[i]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*size*(defeated?3.2f:2.1f);
            }
            for(int i=count;i<sparks.Length;i++)if(sparks[i]!=null)sparks[i].gameObject.SetActive(false);
            if(ringSprite==null){if(ring!=null)ring.gameObject.SetActive(false);return;}
            if(ring==null)
            {var halo=new GameObject("Impact ring");halo.transform.SetParent(transform,false);ring=halo.AddComponent<SpriteRenderer>();}
            ring.gameObject.SetActive(true);ring.sprite=ringSprite;ring.sortingOrder=17;
            ringSize=size*(defeated?2.5f:1.7f);
        }

        public void SetupTrail(Sprite pixel,Color color)
        {
            trail=true;tint=new Color(color.r,color.g,color.b,0.58f);lifetime=0.16f;
            sparks=new SpriteRenderer[1];velocities=new Vector3[1];
            var sprite=gameObject.AddComponent<SpriteRenderer>();sprite.sprite=pixel;sprite.color=tint;sprite.sortingOrder=13;
            transform.localScale=Vector3.one*4;sparks[0]=sprite;
        }

        public void SetupBlood(Sprite pixel)
        {
            age=0;trail=false;blood=true;tint=new Color(1f,.09f,.13f,1f);lifetime=.48f;
            const int count=9;
            if(sparks==null){sparks=new SpriteRenderer[count];velocities=new Vector3[count];}
            else if(sparks.Length<count)
            {System.Array.Resize(ref sparks,count);System.Array.Resize(ref velocities,count);}
            if(bloodColors==null || bloodColors.Length<count)bloodColors=new Color[count];
            for(int i=0;i<count;i++)
            {
                var image=sparks[i];
                if(image==null)
                {var part=new GameObject("Blood pixel");part.transform.SetParent(transform,false);image=part.AddComponent<SpriteRenderer>();sparks[i]=image;}
                image.sprite=pixel;image.transform.localPosition=Vector3.zero;
                bloodColors[i]=i%3==0 ? new Color(1f,.42f,.36f,1f) : tint;
                image.color=bloodColors[i];
                image.sortingOrder=21;
                image.transform.localScale=Vector3.one*(i%3==0?5.5f:4f);
                float angle=(i/(float)count)*Mathf.PI*2+.32f;
                velocities[i]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*(1.4f+i*.18f);
            }
            for(int i=count;i<sparks.Length;i++)if(sparks[i]!=null)sparks[i].gameObject.SetActive(false);
        }

        public void SetupSilhouette(Sprite sprite,Color color,float scale)
        {
            trail=true;tint=new Color(color.r,color.g,color.b,0.42f);lifetime=.18f;
            sparks=new SpriteRenderer[1];velocities=new Vector3[1];
            var image=gameObject.AddComponent<SpriteRenderer>();
            image.sprite=sprite;image.color=tint;image.sortingOrder=12;
            transform.localScale=Vector3.one*scale;
            ringSize=scale;sparks[0]=image;
        }

        void Update()
        {
            float dt=blood?Time.unscaledDeltaTime:Time.deltaTime;
            age+=dt;
            float remaining=1-Mathf.Clamp01(age/lifetime);
            for(int i=0;i<sparks.Length;i++)
            {
                if(sparks[i]==null || !sparks[i].gameObject.activeSelf)continue;
                if(!trail)sparks[i].transform.localPosition+=velocities[i]*dt;
                if(blood)velocities[i].y-=3.2f*dt;
                Color color=blood && bloodColors!=null && i<bloodColors.Length ? bloodColors[i] : tint;
                sparks[i].color=new Color(color.r,color.g,color.b,color.a*remaining);
            }
            if(trail)transform.localScale=Vector3.one*(ringSize>0?ringSize*Mathf.Lerp(.7f,1f,remaining):2+2*remaining);
            if(ring!=null)
            {
                ring.transform.localScale=Vector3.one*ringSize*Mathf.Lerp(0.45f,1.3f,1-remaining);
                ring.color=new Color(tint.r,tint.g,tint.b,remaining*0.8f);
            }
            if(age>=lifetime)
            {if(PoolKind>=0)CombatVfx.Recycle(this,PoolKind);else Destroy(gameObject);}
        }
    }
}
