using UnityEngine;

namespace PixelSurvivor
{
    // Short pixel bursts make hits readable without obscuring a crowded arena.
    public static class CombatVfx
    {
        static int frame=-1, hitsThisFrame, trailsThisFrame;

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
            var root=new GameObject(defeated?"Defeat burst":"Hit burst");
            root.transform.SetParent(game.RuntimeRoot,false);root.transform.position=position;
            var effect=root.AddComponent<CombatVfxInstance>();
            Color tint=weaponColor.HasValue?Color.Lerp(weaponColor.Value,Color.white,defeated ? .28f : .12f):
                defeated?new Color(1f,.72f,.26f):special?
                new Color(.45f,1f,.88f):new Color(1f,.93f,.62f);
            effect.SetupBurst(game.Campaign.Data.whiteSprite,game.Campaign.Data.ringSprite,
                tint,Mathf.Clamp(radius,0.4f,1.5f),defeated);
        }

        public static void Trail(GameManager game,Vector3 position,Color tint)
        {
            if(game==null || game.Campaign==null || game.Campaign.Data.whiteSprite==null)return;
            CountFrame();if(trailsThisFrame>=5)return;
            trailsThisFrame++;
            var root=new GameObject("Projectile afterglow");
            root.transform.SetParent(game.RuntimeRoot,false);root.transform.position=position;
            var effect=root.AddComponent<CombatVfxInstance>();
            effect.SetupTrail(game.Campaign.Data.whiteSprite,tint);
        }
    }

    public sealed class CombatVfxInstance : MonoBehaviour
    {
        SpriteRenderer[] sparks;
        Vector3[] velocities;
        SpriteRenderer ring;
        Color tint;
        float age,lifetime,ringSize;
        bool trail;

        public void SetupBurst(Sprite pixel,Sprite ringSprite,Color color,float size,bool defeated)
        {
            tint=color;lifetime=defeated?0.38f:0.26f;
            int count=defeated?10:7;
            sparks=new SpriteRenderer[count];velocities=new Vector3[count];
            for(int i=0;i<count;i++)
            {
                var part=new GameObject("Pixel spark");part.transform.SetParent(transform,false);
                var sprite=part.AddComponent<SpriteRenderer>();sprite.sprite=pixel;sprite.color=tint;sprite.sortingOrder=18;
                part.transform.localScale=Vector3.one*(defeated?5:4);
                float angle=(i/(float)count)*Mathf.PI*2+Time.time*7;
                velocities[i]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*size*(defeated?3.2f:2.1f);
                sparks[i]=sprite;
            }
            if(ringSprite==null)return;
            var halo=new GameObject("Impact ring");halo.transform.SetParent(transform,false);
            ring=halo.AddComponent<SpriteRenderer>();ring.sprite=ringSprite;ring.sortingOrder=17;
            ringSize=size*(defeated?2.5f:1.7f);
        }

        public void SetupTrail(Sprite pixel,Color color)
        {
            trail=true;tint=new Color(color.r,color.g,color.b,0.58f);lifetime=0.16f;
            sparks=new SpriteRenderer[1];velocities=new Vector3[1];
            var sprite=gameObject.AddComponent<SpriteRenderer>();sprite.sprite=pixel;sprite.color=tint;sprite.sortingOrder=13;
            transform.localScale=Vector3.one*4;sparks[0]=sprite;
        }

        void Update()
        {
            age+=Time.deltaTime;
            float remaining=1-Mathf.Clamp01(age/lifetime);
            for(int i=0;i<sparks.Length;i++)
            {
                if(!trail)sparks[i].transform.localPosition+=velocities[i]*Time.deltaTime;
                sparks[i].color=new Color(tint.r,tint.g,tint.b,tint.a*remaining);
            }
            if(trail)transform.localScale=Vector3.one*(2+2*remaining);
            if(ring!=null)
            {
                ring.transform.localScale=Vector3.one*ringSize*Mathf.Lerp(0.45f,1.3f,1-remaining);
                ring.color=new Color(tint.r,tint.g,tint.b,remaining*0.8f);
            }
            if(age>=lifetime)Destroy(gameObject);
        }
    }
}
