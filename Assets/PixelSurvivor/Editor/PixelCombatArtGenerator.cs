using System.IO;
using UnityEditor;
using UnityEngine;

namespace PixelSurvivor
{
    // Rebuilds deliberately low-resolution, point-filtered combat sprites.
    public static class PixelCombatArtGenerator
    {
        const string Root="Assets/PixelSurvivor/Resources/PixelCombat";
        sealed class Pixels
        {
            public readonly int size;
            readonly Color32[] data;
            public Pixels(int width){size=width;data=new Color32[width*width];}
            public void Dot(int x,int y,Color color,int radius=0)
            {
                Color32 c=color;
                for(int yy=y-radius;yy<=y+radius;yy++)for(int xx=x-radius;xx<=x+radius;xx++)
                    if(xx>=0 && yy>=0 && xx<size && yy<size)data[yy*size+xx]=c;
            }
            public void Rect(int x,int y,int w,int h,Color color)
            {for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)Dot(xx,yy,color);}
            public void Line(int x0,int y0,int x1,int y1,Color color,int radius=0)
            {
                int dx=Mathf.Abs(x1-x0),dy=-Mathf.Abs(y1-y0),sx=x0<x1?1:-1,sy=y0<y1?1:-1,err=dx+dy;
                while(true)
                {
                    Dot(x0,y0,color,radius);
                    if(x0==x1 && y0==y1)break;
                    int e=2*err;if(e>=dy){err+=dy;x0+=sx;}if(e<=dx){err+=dx;y0+=sy;}
                }
            }
            public void Ring(int cx,int cy,int radius,Color color,int width=1)
            {
                for(int y=cy-radius-1;y<=cy+radius+1;y++)for(int x=cx-radius-1;x<=cx+radius+1;x++)
                {
                    float d=Vector2.Distance(new Vector2(x,y),new Vector2(cx,cy));
                    if(Mathf.Abs(d-radius)<width*.55f)Dot(x,y,color);
                }
            }
            public Texture2D Texture()
            {
                var t=new Texture2D(size,size,TextureFormat.RGBA32,false);
                t.filterMode=FilterMode.Point;t.SetPixels32(data);t.Apply();return t;
            }
        }

        static readonly Color Dark=new Color(.025f,.045f,.09f);
        static Color Bright(Color c)=>Color.Lerp(c,Color.white,.5f);
        static Color Shade(Color c)=>Color.Lerp(c,Color.black,.55f);

        static void Motif(Pixels p,int kind,Color color,bool passive)
        {
            Color light=Bright(color),dark=Shade(color);
            int k=passive?kind+12:kind;
            switch(k)
            {
                case 0: // crackling bolt
                    p.Line(15,21,8,12,dark,2);p.Line(8,12,14,12,dark,2);p.Line(14,12,8,3,dark,2);
                    p.Line(15,21,8,12,color,1);p.Line(8,12,14,12,color,1);p.Line(14,12,8,3,color,1);
                    p.Line(13,19,10,13,light);p.Line(12,11,9,5,light);break;
                case 1: // wind dagger
                    p.Line(5,5,18,20,dark,2);p.Line(7,7,17,19,new Color(.76f,.88f,1),1);
                    p.Line(9,9,17,18,Color.white);p.Line(4,9,9,4,new Color(1,.73f,.28f),1);
                    p.Dot(5,5,light,1);break;
                case 2: // luminous whip
                    p.Line(4,5,8,5,dark,2);p.Line(8,5,12,8,color,1);
                    p.Line(12,8,16,12,color,1);p.Line(16,12,19,18,light,1);
                    p.Line(19,18,16,21,Color.white);p.Dot(5,5,new Color(.75f,.42f,.2f),2);break;
                case 3: // orbital sphere
                    p.Ring(12,12,7,dark,3);p.Ring(12,12,7,color,1);
                    p.Dot(12,19,light,2);p.Dot(5,11,light,1);p.Dot(19,12,light,1);p.Dot(12,12,Color.white,2);break;
                case 4: // crescent scythe
                    p.Line(7,3,16,20,dark,2);p.Line(7,3,16,20,new Color(.60f,.37f,.48f),1);
                    p.Line(16,20,9,20,light,2);p.Line(9,20,5,16,light,1);
                    p.Line(5,16,6,13,Color.white);p.Dot(7,3,color,1);break;
                case 5: // storm cloud and bolt
                    p.Rect(5,17,14,3,dark);p.Rect(8,20,9,2,color);p.Rect(5,16,14,2,color);
                    p.Line(13,16,8,9,light,1);p.Line(8,9,13,9,light,1);p.Line(13,9,9,3,Color.white,1);break;
                case 6: // pulse nova
                    for(int i=0;i<8;i++)
                    {float a=i*Mathf.PI/4;p.Line(12+Mathf.RoundToInt(Mathf.Cos(a)*4),12+Mathf.RoundToInt(Mathf.Sin(a)*4),
                        12+Mathf.RoundToInt(Mathf.Cos(a)*9),12+Mathf.RoundToInt(Mathf.Sin(a)*9),color,1);}
                    p.Dot(12,12,light,3);p.Dot(12,12,Color.white,1);break;
                case 7: // seeking flame
                    p.Line(12,3,7,10,dark,2);p.Line(7,10,9,18,color,2);
                    p.Line(9,18,15,21,color,2);p.Line(15,21,18,12,color,1);
                    p.Line(18,12,12,3,light,1);p.Dot(12,11,Color.white,2);break;
                case 8: // ember fan
                    for(int i=0;i<5;i++){int x=5+i*3;p.Line(12,4,x,19,color,1);p.Dot(x,19,light,1);}p.Dot(12,5,Color.white,2);break;
                case 9: // frost cross
                    p.Line(12,3,12,21,color,2);p.Line(3,12,21,12,color,2);
                    p.Line(5,5,19,19,light);p.Line(5,19,19,5,light);p.Dot(12,12,Color.white,2);break;
                case 10: // thorn trap
                    p.Ring(12,12,7,color,2);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;p.Line(12+Mathf.RoundToInt(Mathf.Cos(a)*7),12+Mathf.RoundToInt(Mathf.Sin(a)*7),12+Mathf.RoundToInt(Mathf.Cos(a)*11),12+Mathf.RoundToInt(Mathf.Sin(a)*11),light,1);}p.Dot(12,12,dark,3);break;
                case 11: // moon spear
                    p.Line(12,3,12,18,color,2);p.Line(12,22,12,7,light);p.Line(12,22,5,13,light,1);p.Line(12,22,19,13,light,1);p.Dot(12,4,dark,2);break;
                case 12: // might crest
                    p.Rect(7,5,10,13,dark);p.Rect(9,8,6,8,color);
                    p.Line(12,4,12,20,light,1);p.Line(5,14,19,14,light,1);break;
                case 13: // seed of life
                    p.Dot(9,15,color,3);p.Dot(15,15,color,3);p.Dot(12,11,color,4);
                    p.Line(12,8,12,4,dark,1);p.Dot(12,15,light,2);break;
                case 14: // shield
                    p.Line(5,19,12,21,color,1);p.Line(12,21,19,19,color,1);
                    p.Line(5,19,7,9,color,1);p.Line(19,19,17,9,color,1);
                    p.Line(7,9,12,4,light,1);p.Line(17,9,12,4,light,1);p.Dot(12,14,Color.white,2);break;
                case 15: // area lens
                    p.Ring(11,14,6,color,2);p.Ring(11,14,3,light,1);
                    p.Line(15,9,20,4,dark,2);p.Line(15,9,20,4,light,1);break;
                case 16: // eternal candle
                    p.Rect(9,4,7,12,color);p.Rect(10,5,2,10,light);
                    p.Line(12,16,12,21,new Color(1,.69f,.24f),2);p.Dot(12,21,Color.white);break;
                case 17: // wind boots
                    p.Rect(7,12,6,9,dark);p.Rect(9,8,7,7,color);p.Rect(9,5,11,4,dark);
                    p.Line(5,17,11,17,light);p.Line(3,12,8,12,light);break;
                case 18: // magnetic core
                    p.Ring(12,12,7,color,2);p.Rect(9,17,6,5,Dark);
                    p.Dot(5,17,light,2);p.Dot(19,17,light,2);p.Dot(12,9,Color.white,2);break;
                case 19: // twin prism
                    p.Line(12,21,5,10,color,1);p.Line(12,21,19,10,color,1);
                    p.Line(5,10,12,3,color,1);p.Line(19,10,12,3,color,1);
                    p.Line(12,21,12,3,light);p.Line(5,10,19,10,light);p.Dot(12,11,Color.white,1);break;
                case 20: // iron guard
                    p.Line(5,20,19,20,color,1);p.Line(5,20,7,8,color,1);p.Line(19,20,17,8,color,1);
                    p.Line(7,8,12,3,light,1);p.Line(17,8,12,3,light,1);p.Rect(10,10,4,8,light);break;
                case 21: // keen eye
                    p.Line(3,12,12,18,color,1);p.Line(12,18,21,12,color,1);p.Line(3,12,12,6,color,1);p.Line(12,6,21,12,color,1);p.Dot(12,12,light,3);p.Dot(12,12,Dark,1);break;
                case 22: // soul flask
                    p.Rect(8,5,8,11,dark);p.Rect(9,6,6,8,color);p.Rect(10,17,4,5,light);p.Dot(12,10,Color.white,2);break;
                case 23: // scholar star
                    p.Rect(4,5,7,14,color);p.Rect(13,5,7,14,color);p.Line(12,6,12,19,light);p.Dot(12,20,Color.white,2);break;
            }
        }

        static Texture2D Icon(int kind,Color color,bool passive,bool evolved)
        {
            var p=new Pixels(24);
            p.Rect(1,1,22,22,Dark);
            Color border=evolved?new Color(1,.75f,.3f):Shade(color);
            p.Rect(1,1,22,1,border);p.Rect(1,22,22,1,border);
            p.Rect(1,1,1,22,border);p.Rect(22,1,1,22,border);
            p.Dot(3,20,Bright(color));p.Dot(20,3,Bright(color));
            Motif(p,kind,color,passive);
            if(evolved){p.Dot(3,3,Color.white);p.Dot(20,20,Color.white);p.Line(3,12,5,12,color);p.Line(19,12,21,12,color);}
            return p.Texture();
        }

        static Texture2D Projectile(int kind,Color color)
        {
            var p=new Pixels(24);Motif(p,kind,color,false);return p.Texture();
        }

        static Texture2D Flash(int kind,Color color)
        {
            var p=new Pixels(48);Color light=Bright(color);
            if(kind==2)
            {
                // One solid crescent across the attack direction.
                for(int j=0;j<8;j++)
                {
                    float a=(-.85f+j*.24f)*Mathf.PI;
                    int x=24+Mathf.RoundToInt(Mathf.Cos(a)*18),y=24+Mathf.RoundToInt(Mathf.Sin(a)*18);
                    int x2=24+Mathf.RoundToInt(Mathf.Cos(a+.24f*Mathf.PI)*18);
                    int y2=24+Mathf.RoundToInt(Mathf.Sin(a+.24f*Mathf.PI)*18);
                    p.Line(x,y,x2,y2,j<3?light:color,1);
                }
            }
            else if(kind==5)
            {
                p.Line(27,46,17,28,light,2);p.Line(17,28,25,28,light,2);
                p.Line(25,28,18,7,Color.white,2);p.Line(18,7,29,19,color,1);
                p.Line(29,19,23,2,light,1);
            }
            else if(kind==6 || kind==3)
            {
                p.Ring(24,24,kind==6?15:19,color,2);
                for(int i=0;i<8;i++){float a=i*Mathf.PI/4;p.Line(24+Mathf.RoundToInt(Mathf.Cos(a)*8),24+Mathf.RoundToInt(Mathf.Sin(a)*8),
                    24+Mathf.RoundToInt(Mathf.Cos(a)*21),24+Mathf.RoundToInt(Mathf.Sin(a)*21),i%2==0?light:color,1);}
                p.Dot(24,24,Color.white,3);
            }
            else
            {
                for(int i=0;i<8;i++){float a=i*Mathf.PI/4;p.Line(24+Mathf.RoundToInt(Mathf.Cos(a)*4),24+Mathf.RoundToInt(Mathf.Sin(a)*4),
                    24+Mathf.RoundToInt(Mathf.Cos(a)*15),24+Mathf.RoundToInt(Mathf.Sin(a)*15),color,1);}
                p.Dot(24,24,Color.white,4);
            }
            return p.Texture();
        }

        static Sprite Save(string name,Texture2D texture,float pixelsPerUnit)
        {
            Directory.CreateDirectory(Root);
            string path=Root+"/"+name+".png";
            File.WriteAllBytes(path,texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency=true;
            importer.spritePixelsPerUnit=pixelsPerUnit;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("Tools/Pixel Survivor/Rebuild Pixel Combat Art")]
        public static void Rebuild()
        {
            var campaign=AssetDatabase.LoadAssetAtPath<CampaignData>("Assets/PixelSurvivor/Data/Campaign.asset");
            if(campaign==null)throw new System.InvalidOperationException("Campaign asset not found");
            for(int i=0;i<campaign.weapons.Length;i++)
            {
                var w=campaign.weapons[i];if(w==null)continue;
                w.icon=Save("Weapon"+i,Icon((int)w.pattern,w.color,false,false),100);
                w.evolvedIcon=Save("Evolved"+i,Icon((int)w.pattern,Bright(w.color),false,true),100);
                Save("Projectile"+i,Projectile((int)w.pattern,w.color),64);
                Save("Flash"+i,Flash((int)w.pattern,w.color),48);
                EditorUtility.SetDirty(w);
            }
            Color[] colors={new Color(1,.55f,.38f),new Color(.54f,1,.62f),new Color(.55f,.80f,1),
                new Color(.74f,.65f,1),new Color(1,.75f,.42f),new Color(.50f,1,.87f),
                new Color(.74f,.90f,1),new Color(1,.57f,.86f),new Color(.65f,.8f,1),
                new Color(1,.72f,.46f),new Color(.74f,.55f,1),new Color(.52f,1,.8f)};
            for(int i=0;i<campaign.passives.Length;i++)
            {
                var p=campaign.passives[i];if(p==null)continue;
                p.icon=Save("Passive"+i,Icon(i,colors[i%colors.Length],true,false),100);
                EditorUtility.SetDirty(p);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
