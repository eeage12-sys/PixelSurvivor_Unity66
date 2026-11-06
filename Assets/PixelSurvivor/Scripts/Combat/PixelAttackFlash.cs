using UnityEngine;

namespace PixelSurvivor
{
    // A short, patterned pixel hit shape at the moment each weapon fires.
    public sealed class PixelAttackFlash : MonoBehaviour
    {
        static int frame=-1,shown;
        SpriteRenderer visual;
        float age,lifetime;
        Vector3 initialScale;

        public static void Spawn(CampaignController campaign,WeaponPattern pattern,Vector2 position,Vector2 direction,float area)
        {
            if(campaign==null || campaign.Game==null)return;
            var sprite=PixelCombatArt.Flash(pattern);
            if(sprite==null)return;
            if(frame!=Time.frameCount){frame=Time.frameCount;shown=0;}
            if(shown>=4)return;
            shown++;
            var go=new GameObject(pattern+" pixel attack");
            go.transform.SetParent(campaign.Game.RuntimeRoot,false);
            go.transform.position=position;
            if(direction.sqrMagnitude>.01f)
                go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            var sr=go.AddComponent<SpriteRenderer>();
            sr.sprite=sprite;sr.sortingOrder=16;
            var effect=go.AddComponent<PixelAttackFlash>();
            effect.visual=sr;
            effect.lifetime=pattern==WeaponPattern.Storm ? .36f : pattern==WeaponPattern.Nova ? .42f : .24f;
            effect.initialScale=Vector3.one*Mathf.Clamp(area*1.2f,.75f,3.2f);
            go.transform.localScale=effect.initialScale*.72f;
        }

        void Update()
        {
            age+=Time.deltaTime;
            float p=Mathf.Clamp01(age/lifetime);
            transform.localScale=initialScale*Mathf.Lerp(.72f,1.24f,p);
            visual.color=new Color(1,1,1,1-p);
            if(age>=lifetime)Destroy(gameObject);
        }
    }
}
