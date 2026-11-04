using UnityEngine;
namespace PixelSurvivor
{
    public sealed class PropHitEffect : MonoBehaviour
    {
        SpriteRenderer[] pieces;Vector3[] velocities;float age,lifetime;
        public static void Spawn(CampaignController c,Vector3 position,bool destroyed)
        {
            if(!DisplayOptions.HitEffects)return;
            var go=new GameObject(destroyed?"Prop break sparks":"Prop hit sparks");go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=position;
            var effect=go.AddComponent<PropHitEffect>();effect.lifetime=destroyed?.38f:.22f;
            int count=destroyed?8:5;effect.pieces=new SpriteRenderer[count];effect.velocities=new Vector3[count];
            for(int i=0;i<count;i++)
            {
                var part=new GameObject("Spark");part.transform.SetParent(go.transform,false);
                var sr=part.AddComponent<SpriteRenderer>();sr.sprite=c.Data.whiteSprite;sr.sortingOrder=15;
                sr.color=destroyed?new Color(1,.65f,.22f):new Color(1,.94f,.65f);
                // White.png is one pixel at 32 pixels/unit: scale converts to world dimensions.
                part.transform.localScale=Vector3.one*(destroyed?3:2);
                float angle=(i/(float)count)*Mathf.PI*2;
                effect.velocities[i]=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*(destroyed?2.4f:1.8f);
                effect.pieces[i]=sr;
            }
        }
        void Update()
        {
            age+=Time.deltaTime;float alpha=1-Mathf.Clamp01(age/lifetime);
            for(int i=0;i<pieces.Length;i++)
            {pieces[i].transform.localPosition+=velocities[i]*Time.deltaTime;var color=pieces[i].color;color.a=alpha;pieces[i].color=color;}
            if(age>=lifetime)Destroy(gameObject);
        }
    }
}
