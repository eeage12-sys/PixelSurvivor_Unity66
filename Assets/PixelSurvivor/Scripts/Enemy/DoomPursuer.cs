using UnityEngine;

namespace PixelSurvivor
{
    // A unique clockwork silhouette. It is deliberately outside the Enemy damage system.
    public sealed class DoomPursuer : MonoBehaviour
    {
        static Sprite art;
        CampaignController campaign;
        SpriteRenderer body,halo;
        float age;
        public float Age => age;

        public void Initialize(CampaignController owner)
        {
            campaign=owner;
            body=gameObject.AddComponent<SpriteRenderer>();
            body.sprite=Sprite();body.sortingOrder=30;
            var ring=new GameObject("Doom clock halo");ring.transform.SetParent(transform,false);
            halo=ring.AddComponent<SpriteRenderer>();
            halo.sprite=owner.Data.ringSprite;
            halo.color=new Color(.82f,.19f,.95f,.65f);
            halo.sortingOrder=29;
            ring.transform.localScale=Vector3.one*3.8f;
        }

        void Update(){Advance(Time.deltaTime);}

        public void Advance(float dt)
        {
            if(campaign==null || !campaign.Game.IsPlaying)return;
            age+=Mathf.Max(0,dt);
            float pulse=.8f+.18f*Mathf.Sin(Time.time*9);
            transform.localScale=Vector3.one*(1f+.045f*Mathf.Sin(age*8f));
            if(halo!=null)
            {
                halo.transform.localRotation=Quaternion.Euler(0,0,age*105);
                halo.color=new Color(.82f,.19f,.95f,.42f+.22f*pulse);
            }
            if(body!=null)body.color=new Color(1,1,1,age<.9f?Mathf.Clamp01(age/.9f):1);
            if(age<.9f)return;
            Vector2 target=campaign.Game.Player.transform.position;
            float speed=Mathf.Max(9f,campaign.Game.Stats.MoveSpeed+4f+age*.85f);
            transform.position=Vector2.MoveTowards(transform.position,target,speed*dt);
            if(((Vector2)transform.position-target).sqrMagnitude<=1.1f*1.1f)
                campaign.Stage.CaughtByDoom();
        }

        static Sprite Sprite()
        {
            if(art!=null)return art;
            const int width=64,height=72;
            var pixels=new Color32[width*height];
            Color32 outline=new Color32(26,12,43,255),iron=new Color32(54,44,75,255);
            Color32 brass=new Color32(245,181,69,255),shadow=new Color32(135,77,77,255);
            Color32 cyan=new Color32(76,241,225,255),violet=new Color32(196,78,232,255);
            Color32 core=new Color32(12,31,51,255),red=new Color32(255,72,106,255);
            void Dot(int x,int y,Color32 color)
            {if(x>=0 && x<width && y>=0 && y<height)pixels[y*width+x]=color;}
            void Bar(int x0,int y0,int x1,int y1,Color32 color)
            {for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)Dot(x,y,color);}
            void Line(int x0,int y0,int x1,int y1,Color32 color,int thickness=1)
            {
                int dx=Mathf.Abs(x1-x0),dy=-Mathf.Abs(y1-y0),sx=x0<x1?1:-1,sy=y0<y1?1:-1,err=dx+dy;
                while(true)
                {
                    for(int ox=-thickness/2;ox<=thickness/2;ox++)for(int oy=-thickness/2;oy<=thickness/2;oy++)Dot(x0+ox,y0+oy,color);
                    if(x0==x1 && y0==y1)break;
                    int twice=2*err;
                    if(twice>=dy){err+=dy;x0+=sx;}
                    if(twice<=dx){err+=dx;y0+=sy;}
                }
            }
            // Four pointed mechanical legs and a forked crown make it unlike the living roster.
            Line(17,31,6,7,outline,5);Line(17,31,6,7,brass,2);
            Line(47,31,57,7,outline,5);Line(47,31,57,7,brass,2);
            Line(23,27,17,3,outline,5);Line(23,27,17,3,cyan,2);
            Line(41,27,47,3,outline,5);Line(41,27,47,3,cyan,2);
            Line(20,57,12,70,outline,5);Line(20,57,12,70,violet,2);
            Line(44,57,52,70,outline,5);Line(44,57,52,70,violet,2);
            Bar(29,63,35,71,outline);Bar(31,66,33,71,brass);
            // Jagged outer gear, dark clock face, and a luminous broken dial.
            for(int y=16;y<=64;y++)for(int x=7;x<=57;x++)
            {
                float dx=x-32,dy=y-40;
                float r=Mathf.Sqrt(dx*dx+dy*dy);
                if(r<=23.2f)Dot(x,y,outline);
                if(r<=20.5f)Dot(x,y,brass);
                if(r<=18.2f)Dot(x,y,iron);
                if(r<=15.5f)Dot(x,y,core);
                if(r>=13.1f && r<14.3f)Dot(x,y,violet);
            }
            Bar(3,38,9,43,outline);Bar(4,39,8,42,brass);
            Bar(55,38,61,43,outline);Bar(56,39,60,42,brass);
            Bar(29,14,35,19,outline);Bar(30,15,34,18,brass);
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;
                int x0=32+Mathf.RoundToInt(Mathf.Cos(a)*15),y0=40+Mathf.RoundToInt(Mathf.Sin(a)*15);
                int x1=32+Mathf.RoundToInt(Mathf.Cos(a)*19),y1=40+Mathf.RoundToInt(Mathf.Sin(a)*19);
                Line(x0,y0,x1,y1,i%3==0?cyan:brass,2);
            }
            Line(32,40,23,47,cyan,3);Line(32,40,39,36,red,3);
            Bar(28,36,36,44,outline);Bar(29,37,35,43,red);Bar(31,39,33,41,cyan);
            Dot(22,31,cyan);Dot(42,51,cyan);Dot(13,40,violet);Dot(51,40,violet);
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            texture.SetPixels32(pixels);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;texture.Apply();
            texture.name="Doom clock texture";
            art=UnityEngine.Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),22f,0,SpriteMeshType.FullRect);
            art.name="Doom clock pursuer";
            return art;
        }
    }
}
