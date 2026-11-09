using System;
using UnityEngine;
using UnityEngine.UI;
namespace PixelSurvivor
{
    // Screen-edge anchors keep combat readable even when the editor Game view changes aspect.
    [DefaultExecutionOrder(200)]
    public sealed class CompactHUD : MonoBehaviour
    {
        CampaignController c; RectTransform root,health; Image expFill,hpFill;
        Text level,timer,kills,gold; readonly Image[] weapons=new Image[7],passives=new Image[7];
        readonly Image[] weaponFrames=new Image[7],passiveFrames=new Image[7];
        readonly Text[] weaponLevels=new Text[7],passiveLevels=new Text[7];
        Sprite coinSprite,killSprite; Texture2D coinTexture,killTexture;
        static readonly Color Border=new Color(.12f,.22f,.29f,.92f),Mint=new Color(.38f,.95f,.78f);
        public void Initialize(CampaignController owner,Action pause,Action<int> weapon,Action<int> passive)
        {
            c=owner;root=(RectTransform)transform;
            var band=Panel("Combat HUD backdrop",root,new Vector2(.5f,1),new Vector2(0,-65),new Vector2(0,130),new Color(.025f,.05f,.09f,1f));
            band.rectTransform.anchorMin=new Vector2(0,1);band.rectTransform.anchorMax=Vector2.one;
            band.rectTransform.pivot=new Vector2(.5f,1);band.rectTransform.anchoredPosition=Vector2.zero;
            band.rectTransform.sizeDelta=new Vector2(0,130);
            var exp=Panel("Top experience",root,new Vector2(.5f,1),Vector2.zero,new Vector2(0,24),Border);
            exp.rectTransform.anchorMin=new Vector2(0,1);exp.rectTransform.anchorMax=Vector2.one;exp.rectTransform.pivot=new Vector2(.5f,1);
            expFill=Panel("Experience fill",exp.transform,new Vector2(0,.5f),Vector2.zero,Vector2.zero,new Color(.25f,.68f,.95f));
            Stretch(expFill.rectTransform);expFill.sprite=c.Data.whiteSprite;expFill.type=Image.Type.Filled;expFill.fillMethod=Image.FillMethod.Horizontal;
            level=Label("Level",exp.transform,"LV1",new Vector2(.5f,.5f),Vector2.zero,new Vector2(150,24),18);
            timer=Label("Single timer",root,"00:00:00",new Vector2(.5f,1),new Vector2(0,-52),new Vector2(260,46),32);
            for(int i=0;i<7;i++)
            {
                int index=i;weapons[i]=Slot("Attack slot "+i,34+i*48,-54,()=>weapon(index),out weaponFrames[i],out weaponLevels[i]);
                passives[i]=Slot("Passive slot "+i,34+i*48,-102,()=>passive(index),out passiveFrames[i],out passiveLevels[i]);
            }
            coinSprite=Glyph(new[]{"...####...","..######..",".##++++##.","##++##++##","##++#+++##","##++##++##",".##++++##.","..######..","...####..."},new Color(1,.78f,.22f),out coinTexture);
            killSprite=Glyph(new[]{"..######..",".########.","###.##.###","###.##.###",".########.","..##..##..","..######..","...#..#...",".........."},new Color(.9f,.91f,.85f),out killTexture);
            var killIcon=Panel("Kill icon",root,new Vector2(1,1),new Vector2(-238,-54),new Vector2(26,26),Color.white);killIcon.sprite=killSprite;
            kills=Label("Kill count",root,"0",new Vector2(1,1),new Vector2(-180,-54),new Vector2(85,35),23);
            var coin=Panel("Gold icon",root,new Vector2(1,1),new Vector2(-112,-54),new Vector2(26,26),Color.white);coin.sprite=coinSprite;
            gold=Label("Run gold",root,"0",new Vector2(1,1),new Vector2(-52,-54),new Vector2(90,35),23);
            var button=Panel("Pause",root,new Vector2(1,1),new Vector2(-42,-102),new Vector2(60,36),Border);
            button.raycastTarget=true;button.gameObject.AddComponent<Button>().onClick.AddListener(()=>pause());
            button.gameObject.AddComponent<ButtonHoverFeedback>();
            Label("Pause symbol",button.transform,"Ⅱ",new Vector2(.5f,.5f),Vector2.zero,new Vector2(50,32),22);
            health=Panel("Player foot health",root,new Vector2(.5f,.5f),Vector2.zero,new Vector2(48,7),new Color(.04f,.05f,.07f,.95f)).rectTransform;
            hpFill=Panel("Health fill",health,new Vector2(.5f,.5f),Vector2.zero,new Vector2(44,3),new Color(.95f,.3f,.4f));
            hpFill.sprite=c.Data.whiteSprite;hpFill.type=Image.Type.Filled;hpFill.fillMethod=Image.FillMethod.Horizontal;
        }
        Image Slot(string name,float x,float y,Action action,out Image frame,out Text badge)
        {
            frame=Panel(name,root,new Vector2(0,1),new Vector2(x,y),new Vector2(42,42),Border);
            frame.raycastTarget=true;frame.gameObject.AddComponent<Button>().onClick.AddListener(()=>action());
            frame.gameObject.AddComponent<ButtonHoverFeedback>();
            var inset=Panel("Inset",frame.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(38,38),new Color(.035f,.075f,.13f));
            var accent=Panel("Sigil edge",frame.transform,new Vector2(.5f,1),new Vector2(0,-2),new Vector2(36,2),new Color(.38f,.95f,.78f,.65f));
            badge=Label("Equipment level",frame.transform,"",new Vector2(1,0),new Vector2(-9,9),new Vector2(17,17),14);
            badge.color=new Color(1,.85f,.45f);
            var icon=Panel("Equipment icon",frame.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(29,29),Color.white);
            icon.transform.SetSiblingIndex(2);return icon;
        }
        void LateUpdate()
        {
            if(c==null)return;
            level.text="LV"+c.Game.Level.Level;
            expFill.fillAmount=(float)c.Game.Level.Experience/Mathf.Max(1,c.Game.Level.RequiredEXP);
            timer.text=SurvivalRules.Clock(c.Stage.Elapsed);kills.text=c.Game.KillCount.ToString();gold.text=c.RunGold.ToString();
            for(int i=0;i<7;i++)
            {
                bool has=i<c.Equipment.Weapons.Count;weapons[i].enabled=has;
                weaponFrames[i].color=has&&c.Equipment.CanEvolve(c.Equipment.Weapons[i])?new Color(.32f,.9f,.68f):has&&c.Equipment.Weapons[i].evolved?new Color(1,.72f,.34f):has?new Color(.26f,.55f,.64f):new Color(.12f,.22f,.29f,.28f);
                weaponLevels[i].text=has?c.Equipment.Weapons[i].level.ToString():"";
                if(has){var w=c.Equipment.Weapons[i];weapons[i].sprite=w.evolved?w.data.evolvedIcon:w.data.icon;}
                has=i<c.Equipment.Passives.Count;passives[i].enabled=has;passiveFrames[i].color=has?new Color(.58f,.42f,.74f):new Color(.12f,.22f,.29f,.28f);passiveLevels[i].text=has?c.Equipment.Passives[i].level.ToString():"";if(has)passives[i].sprite=c.Equipment.Passives[i].data.icon;
            }
            bool show=DisplayOptions.HealthBar&&c.Game.State!=RunState.GameOver;
            health.gameObject.SetActive(show);
            if(show)
            {
                Vector3 screen=c.Game.View.WorldToScreenPoint(c.Game.Player.transform.position+Vector3.down*.62f);
                Vector2 point;RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,null,out point);
                health.anchoredPosition=point;hpFill.fillAmount=c.Game.Health.CurrentHP/Mathf.Max(1,c.Game.Stats.MaxHP);
            }
        }
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static Image Panel(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));var r=(RectTransform)go.transform;r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;
        }
        static Text Label(string name,Transform parent,string text,Vector2 anchor,Vector2 pos,Vector2 size,int fontSize)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));var r=(RectTransform)go.transform;r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;
            var t=go.GetComponent<Text>();t.font=KoreanText.Font;t.text=text;t.fontSize=fontSize;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=14;t.resizeTextMaxSize=fontSize;return t;
        }
        static Sprite Glyph(string[] rows,Color color,out Texture2D texture)
        {
            int w=rows[0].Length,h=rows.Length;texture=new Texture2D(w,h,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)texture.SetPixel(x,h-1-y,rows[y][x]=='.'?Color.clear:rows[y][x]=='+'?Color.Lerp(color,Color.white,.45f):color);
            texture.Apply();return Sprite.Create(texture,new Rect(0,0,w,h),Vector2.one*.5f,16);
        }
        void OnDestroy(){if(coinSprite!=null)Destroy(coinSprite);if(killSprite!=null)Destroy(killSprite);if(coinTexture!=null)Destroy(coinTexture);if(killTexture!=null)Destroy(killTexture);}
    }
}

