using UnityEngine;

namespace PixelSurvivor
{
    // Moves only the rendered sprite, so a hit never changes collision or targeting coordinates.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class HitFeedbackVisual : MonoBehaviour
    {
        SpriteRenderer source,display;
        float remaining,duration,amplitude;
        float phase;
        Color flashColor;
        public bool IsShaking => remaining>0;
        public SpriteRenderer Display => display;

        void Awake()
        {
            source=GetComponent<SpriteRenderer>();
            phase=Random.Range(0f,20f);
            var child=new GameObject("Hit feedback sprite");
            child.transform.SetParent(transform,false);
            display=child.AddComponent<SpriteRenderer>();
            CopyAppearance();
            source.enabled=false;
        }

        public void Play(float seconds,float distance,Color color)
        {
            duration=Mathf.Max(.01f,seconds);
            remaining=duration;
            amplitude=Mathf.Max(0,distance);
            flashColor=color;
            if(display!=null)display.color=color;
        }

        void LateUpdate()
        {
            if(source==null || display==null)return;
            CopyAppearance();
            if(remaining<=0)
            {display.transform.localPosition=Vector3.zero;return;}
            remaining=Mathf.Max(0,remaining-Time.unscaledDeltaTime);
            float strength=remaining/duration;
            float x=Mathf.Sin(Time.unscaledTime*115f+phase)*amplitude*strength;
            float y=Mathf.Cos(Time.unscaledTime*91f+phase)*amplitude*.6f*strength;
            display.transform.localPosition=new Vector3(x,y,0);
            display.color=Color.Lerp(source.color,flashColor,Mathf.Clamp01(strength*1.35f));
        }

        void CopyAppearance()
        {
            display.sprite=source.sprite;
            display.color=source.color;
            display.flipX=source.flipX;
            display.flipY=source.flipY;
            display.sortingLayerID=source.sortingLayerID;
            display.sortingOrder=source.sortingOrder;
        }
    }
}
