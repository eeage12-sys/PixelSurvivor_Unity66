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
        bool motionEnabled,airborne;
        float motionPhase,motionStrength;
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

        public void ConfigureMotion(bool flies)
        {
            motionEnabled=true;
            airborne=flies;
            motionPhase=Random.Range(0f,Mathf.PI*2f);
        }

        public void Move(Vector2 velocity,float deltaTime)
        {
            if(!motionEnabled)return;
            float target=Mathf.Clamp01(velocity.magnitude/2f);
            motionStrength=Mathf.MoveTowards(motionStrength,target,deltaTime*5f);
            motionPhase+=deltaTime*(airborne?7f:9f)*Mathf.Lerp(.55f,1.3f,target);
            if(Mathf.Abs(velocity.x)>.12f)source.flipX=velocity.x<0;
        }

        void LateUpdate()
        {
            if(source==null || display==null)return;
            CopyAppearance();
            float beat=Mathf.Sin(motionPhase);
            float bob=motionEnabled ? (airborne ? beat*.055f : Mathf.Abs(beat)*.035f)*motionStrength : 0f;
            float sway=motionEnabled ? Mathf.Cos(motionPhase)*.018f*motionStrength : 0f;
            float shakeX=0f,shakeY=0f;
            if(remaining>0)
            {
                remaining=Mathf.Max(0,remaining-Time.unscaledDeltaTime);
                float strength=remaining/duration;
                shakeX=Mathf.Sin(Time.unscaledTime*115f+phase)*amplitude*strength;
                shakeY=Mathf.Cos(Time.unscaledTime*91f+phase)*amplitude*.6f*strength;
                display.color=Color.Lerp(source.color,flashColor,Mathf.Clamp01(strength*1.35f));
            }
            display.transform.localPosition=new Vector3(sway+shakeX,bob+shakeY,0);
            display.transform.localRotation=Quaternion.Euler(0,0,beat*(airborne?2f:2.8f)*motionStrength);
            float squash=beat*motionStrength*(airborne?.045f:.025f);
            display.transform.localScale=new Vector3(1f+squash,1f-squash,1f);
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
