using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace PixelSurvivor
{
    // Builds the expansion UI on a separate canvas, preserving the existing HUD and scene objects.
    public sealed partial class CampaignUI
    {
        readonly CampaignController c;
        Canvas canvas;Font font;GameObject menu,characters,stages,shop,codex,hud,modal;
        int codexPage;
        Button characterNext,stageStart;
        
        Text selection,gold,notice,boss,stageInfo;
        CompactHUD compactHud;
        readonly List<Button> characterButtons=new List<Button>(),stageButtons=new List<Button>();
        
        Text practiceLabel,previewLabel;float noticeUntil;
        int shopConstellation=-2;
        static readonly string[] ConstellationNames={"새벽의 검자리","질풍의 제비자리","불굴의 방패자리","비전의 고리자리","그림자의 낫자리"};
        static readonly Color Ink=new Color(0.025f,0.05f,0.09f,0.98f), Panel=new Color(0.08f,0.15f,0.22f,1), Mint=new Color(0.38f,0.95f,0.78f), Muted=new Color(0.6f,0.68f,0.75f);
        public CampaignUI(CampaignController owner){c=owner;}
        public void Build()
        {
            font=KoreanText.Font;
            var root=new GameObject("Campaign Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=0.5f;
            root.AddComponent<FitCanvasViewport>();
            menu=Overlay("Main Menu");characters=Overlay("Character Select");stages=Overlay("Stage Select");shop=Overlay("Permanent Upgrades");codex=Overlay("Equipment Codex");hud=Box("Equipment HUD",root.transform,Vector2.zero,Vector2.zero,Color.clear).gameObject;
            Stretch(hud.GetComponent<RectTransform>());hud.GetComponent<Image>().raycastTarget=false;
            var backdrop=Resources.Load<Texture2D>("MenuBackdrop");
            if(backdrop!=null)
            {
                var background=Rect("Pixel Survivor title art",menu.transform,0,0,1600,900);
                Stretch(background);
                var art=background.gameObject.AddComponent<RawImage>();
                art.texture=backdrop;
                art.raycastTarget=false;
            }
            const float lobbyX=300;
            TextAt(menu.transform,"픽셀 서바이버",lobbyX,245,850,100,64,Mint);
            TextAt(menu.transform,"무한 생존",lobbyX,155,850,60,28,Muted);
            TextAt(menu.transform,"장비를 조합하고 보스를 쓰러뜨리며 오래 살아남으세요.",lobbyX,85,850,55,24,Color.white);
            ButtonAt(menu.transform,"시작",lobbyX,-30,450,75,()=>ShowCharacters(true));
            ButtonAt(menu.transform,"영구 강화",lobbyX,-130,450,75,()=>ShowShop());
            ButtonAt(menu.transform,"도감",lobbyX,-230,450,65,()=>ShowCodex(0));
            ButtonAt(menu.transform,"종료",lobbyX,-305,450,58,()=>Quit());
            var practice=ButtonAt(menu.transform,"",lobbyX,-385,850,40,()=>{c.TogglePractice();RefreshMenu();});practiceLabel=practice.GetComponentInChildren<Text>();
            gold=TextAt(menu.transform,"",lobbyX,360,850,45,25,new Color(1,0.85f,0.35f));
            TextAt(characters.transform,"캐릭터 선택",0,365,1450,70,44,Mint);
            selection=TextAt(characters.transform,"함께할 캐릭터를 선택하세요.",0,295,1450,55,23,Muted);
            for(int i=0;i<c.Data.characters.Length;i++)
            {
                int id=i;var data=c.Data.characters[i];float x=-608+i*304;
                var b=ButtonAt(characters.transform,"",x,0,288,490,()=>{c.SelectCharacter(id);RefreshMenu();});b.name="Character "+i+" Button";characterButtons.Add(b);
                var redesigned=RedesignedSprites.Direction(i,0);
                Icon(b.transform,redesigned!=null?redesigned[0]:data.portrait,0,160,125,Color.white);
                TextAt(b.transform,CharacterName(data),0,90,270,65,24,Mint);
                TextAt(b.transform,"체력 "+data.maxHP+"    공격력 "+data.damage+"\n이동속도 "+data.moveSpeed+"\n초당 기본 공격 "+(1/data.attackInterval).ToString("0.00"),0,12,264,100,18,Color.white);
                var weaponFrame=Box("Starting weapon frame",b.transform,new Vector2(0,-93),new Vector2(260,73),new Color(.035f,.10f,.17f,.92f));weaponFrame.raycastTarget=false;
                if(data.startingWeapon.icon!=null)Icon(b.transform,data.startingWeapon.icon,-92,-93,54,Color.white);
                TextAt(b.transform,"시작 무기\n"+data.startingWeapon.title,30,-93,185,65,18,new Color(.95f,.86f,.57f));
                TextAt(b.transform,data.passiveDescription,0,-190,264,100,18,Muted);
            }
            ButtonAt(characters.transform,"뒤로",-360,-345,320,65,()=>ShowMenu());
            characterNext=ButtonAt(characters.transform,"다음: 스테이지 선택",360,-345,440,65,()=>ShowStages());characterNext.interactable=false;
            TextAt(stages.transform,"스테이지 선택",0,355,1400,70,46,Mint);
            for(int i=0;i<c.Data.stages.Length;i++)
            {int id=i;var b=ButtonAt(stages.transform,c.Data.stages[i].title,-480+i*480,190,440,100,()=>{c.SelectStage(id);RefreshMenu();});stageButtons.Add(b);}
            stageInfo=TextAt(stages.transform,"",0,-30,1400,260,25,Color.white);
            var preview=ButtonAt(stages.transform,"",0,-210,720,50,()=>{c.ToggleStagePreview();RefreshMenu();});previewLabel=preview.GetComponentInChildren<Text>();
            ButtonAt(stages.transform,"뒤로",-360,-300,320,65,()=>ShowCharacters(false));
            stageStart=ButtonAt(stages.transform,"전투 시작",360,-300,440,75,()=>c.Game.BeginRun());
            compactHud=hud.AddComponent<CompactHUD>();
            compactHud.Initialize(c,()=>ShowPause(),InspectWeapon,InspectPassive);
            boss=TextAt(hud.transform,"",0,-120,850,40,22,new Color(1,0.5f,0.4f));
            PinTop(boss.rectTransform);
            notice=TextAt(hud.transform,"",0,-165,1250,60,22,Color.white);PinTop(notice.rectTransform);
            shop.SetActive(false);codex.SetActive(false);hud.SetActive(false);characters.SetActive(false);stages.SetActive(false);
        }
        void RefreshMenu()
        {
            for(int i=0;i<characterButtons.Count;i++)
                characterButtons[i].GetComponent<Image>().color=c.HasCharacterSelection && i==c.CharacterIndex?new Color(0.15f,0.42f,0.35f):Panel;
            for(int i=0;i<stageButtons.Count;i++)
            {
                bool unlocked=i<c.Save.Progress.unlockedStages;stageButtons[i].interactable=unlocked || c.PreviewStages;
                stageButtons[i].GetComponentInChildren<Text>().text=c.Data.stages[i].title+(unlocked?"":c.PreviewStages?"  [체험]":"  [잠김]");
                stageButtons[i].GetComponent<Image>().color=i==c.StageIndex?new Color(0.15f,0.42f,0.35f):Panel;
            }
            selection.text=c.HasCharacterSelection?"선택: "+CharacterName(c.Character):"함께할 캐릭터를 선택하세요.";
            characterNext.interactable=c.HasCharacterSelection;stageStart.interactable=c.HasCharacterSelection;
            stageInfo.text=c.StageData.description+"\n\n캐릭터: "+CharacterName(c.Character)+"\n최고 생존 기록: "+SurvivalRules.Clock(c.Save.Best(c.Character.id,c.StageData.id))+"\n\n제한시간 없이 생존하세요. 보스는 10분마다 등장합니다.\n20분 보스를 처치하면 다음 구역이 열리고 현재 전투는 계속됩니다."+(c.PreviewStages?"\n체험 전투는 골드·기록·해금이 저장되지 않습니다.":"");
            gold.text="보유 골드  "+c.Save.Progress.gold;
            practiceLabel.text=c.Practice?"빠른 테스트 켜짐 · 보스 30초 간격 · 골드/기록/해금 저장 안 함":"빠른 테스트 꺼짐 · 무한 생존";
            previewLabel.text=c.PreviewStages?"스테이지 체험 켜짐 · 저장하지 않음":"잠긴 스테이지 체험하기";
        }
        void Only(GameObject screen)
        {HideModal();menu.SetActive(screen==menu);characters.SetActive(screen==characters);stages.SetActive(screen==stages);shop.SetActive(screen==shop);codex.SetActive(screen==codex);hud.SetActive(screen==hud);}
        public void ShowMenu(){Only(menu);RefreshMenu();SelectFirst(menu);}
        void ShowCharacters(bool fresh){if(fresh)c.ClearCharacterSelection();Only(characters);RefreshMenu();SelectFirst(characters);}
        void ShowStages(){if(!c.HasCharacterSelection)return;Only(stages);RefreshMenu();SelectFirst(stages);}
        public void ShowGameplay(){Only(hud);Tick();}
        void Quit()
        {
            c.Save.Save();
#if UNITY_EDITOR
            Notify("Unity 상단 재생 버튼을 눌러 종료하세요.");
            gold.text="Unity 편집기에서는 상단 재생 버튼을 눌러 종료하세요.";
#else
            Application.Quit();
#endif
        }
        static void PinTop(RectTransform r){r.anchorMin=r.anchorMax=new Vector2(0.5f,1);}
        public void Tick()
        {
            if(hud==null)return;
            var b=c.Stage.CurrentBoss;boss.text=b!=null && b.IsAlive?(b.IsFinalBoss?"해금 보스":"보스")+"  "+Mathf.CeilToInt(b.CurrentHP)+" / "+Mathf.CeilToInt(b.MaxHP):"";
            if(Time.unscaledTime>noticeUntil)notice.text="";
        }
        public void Notify(string message){if(notice==null)return;notice.text=message;noticeUntil=Time.unscaledTime+4;}
        void InspectWeapon(int index){if(index<c.Equipment.Weapons.Count)ShowPause();}
        void InspectPassive(int index){if(index<c.Equipment.Passives.Count)ShowPause();}
        public void ShowChoices(EquipmentChoice[] choices)
        {
            CombatAudio.LevelUp();
            NewModal("레벨 상승 · 장비 선택", "공격형 "+c.Equipment.Weapons.Count+" / 7     강화형 "+c.Equipment.Passives.Count+" / 7");
            for(int i=0;i<choices.Length;i++)
            {
                int index=i;var choice=choices[i];float x=(i-(choices.Length-1)*0.5f)*470;
                var button=ButtonAt(modal.transform,"",x,-20,440,430,()=>c.Game.SelectUpgrade(index));
                var root=button.transform;
                button.GetComponent<Image>().color=new Color(.055f,.105f,.17f,.98f);
                var top=Box("Card accent",root,new Vector2(0,207),new Vector2(428,5),choice.kind==EquipmentChoiceKind.Weapon?new Color(.35f,.9f,1):new Color(.82f,.61f,1));top.raycastTarget=false;
                var iconFrame=Box("Equipment sigil",root,new Vector2(0,135),new Vector2(90,90),new Color(.13f,.24f,.33f));iconFrame.raycastTarget=false;
                string title="",description="",condition="";Sprite icon=null,partnerIcon=null;bool owned=false;
                if(choice.kind==EquipmentChoiceKind.Weapon)
                {
                    var w=c.Equipment.Find(choice.weapon);title=choice.weapon.title+"\n"+(w==null?"새 공격형 장비":"레벨 "+w.level+" > "+(w.level+1));
                    description=choice.weapon.description+"\n레벨에 따라 공격 성능이 강화됩니다.";
                    condition=c.Equipment.EvolutionStatus(choice.weapon);icon=choice.weapon.icon;
                    var partner=choice.weapon.evolutionPartner;partnerIcon=partner!=null?partner.icon:null;owned=partner!=null && c.Equipment.Find(partner)!=null;
                }
                else if(choice.kind==EquipmentChoiceKind.Passive)
                {
                    var p=c.Equipment.Find(choice.passive);title=choice.passive.title+"\n"+(p==null?"새 강화형 장비":"레벨 "+p.level+" > "+(p.level+1));
                    description=choice.passive.description+"\n강화형 장비는 진화하지 않습니다.";
                    condition="진화 조합\n"+c.Equipment.Synergy(choice.passive);icon=choice.passive.icon;
                }
                TextAt(root,choice.kind==EquipmentChoiceKind.Weapon?"공격형":"강화형",0,182,390,35,19,Muted);
                TextAt(root,title,0,52,410,90,30,Mint);
                TextAt(root,description,0,-48,405,112,22,Color.white);
                TextAt(root,condition,0,-155,400,95,17,choice.kind==EquipmentChoiceKind.Weapon&&!owned?Muted:Mint);
                if(icon!=null)Icon(root,icon,0,135,70,Color.white);
                if(partnerIcon!=null)Icon(root,partnerIcon,165,-175,40,owned?Color.white:Color.gray);
            }
            SelectFirst(modal);
        }
        public void ShowChest(List<string> results,bool bossChest,bool final)
        {
            NewModal(bossChest?"보스 보물상자 · 보상 3회":results.Count>1?"대박! 보상 ×"+results.Count:"엘리트 보물상자","매 보상마다 진화 조건을 확인합니다. 조합에 쓰인 강화형 장비는 유지됩니다.");
            TextAt(modal.transform,string.Join("\n\n",results),0,10,1300,410,27,Color.white);
            ButtonAt(modal.transform,"계속하기",0,-300,420,70,()=>c.CloseChest());SelectFirst(modal);
        }
        public void ShowResult(bool won)
        {
            NewModal("게임 종료",c.StageData.title+" / "+CharacterName(c.Character));
            TextAt(modal.transform,"생존 시간  "+SurvivalRules.Clock(c.Stage.Elapsed)+"\n도달 레벨  "+c.Game.Level.Level+"    총 처치  "+c.Game.KillCount+
                "\n엘리트 처치  "+c.EliteKills+"    보스 처치  "+c.BossKills+"\n획득 골드  "+c.RunGold+
                "\n최고 생존 기록  "+SurvivalRules.Clock(c.Save.Best(c.Character.id,c.StageData.id))+"\n"+
                (c.NoProgress?"체험 전투의 골드, 기록, 해금은 저장되지 않습니다.":"골드와 성장 기록을 저장했습니다. 전투 장비는 초기화됩니다."),0,10,1400,350,28,Color.white);
            ButtonAt(modal.transform,"메인 메뉴",-260,-280,420,70,()=>c.Game.ReturnToMenu());
            ButtonAt(modal.transform,"다시 시작",260,-280,420,70,()=>c.Game.BeginRun());SelectFirst(modal);
        }
        static string CharacterName(CharacterData data){return data.title.Split('·')[0].Trim();}
        void ShowShop()
        {
            Only(shop);shopConstellation=-2;RefreshShop();
        }
        void ShowConstellation(int index)
        {
            shopConstellation=index;RefreshShop();
        }
        void RefreshShop()
        {
            for(int i=shop.transform.childCount-1;i>=0;i--){var go=shop.transform.GetChild(i).gameObject;go.SetActive(false);Object.Destroy(go);}
            TextAt(shop.transform,"영구 강화",0,365,1350,70,44,Mint);
            TextAt(shop.transform,"보유 골드  "+c.Save.Progress.gold+"    ·    강화 효과는 다음 전투부터 적용됩니다.",0,305,1400,48,23,Color.white);
            if(shopConstellation==-2)DrawShopSelection();else DrawConstellation();
            SelectFirst(shop);
        }
        void DrawShopSelection()
        {
            TextAt(shop.transform,"계시 또는 캐릭터의 별자리를 선택하세요",0,230,1200,48,27,Muted);
            for(int i=-1;i<c.Data.characters.Length;i++)
            {
                int index=i;float x=-625+(i+1)*250;
                var card=ButtonAt(shop.transform,"",x,0,220,340,()=>ShowConstellation(index));
                card.name=i<0?"Revelation":"Character constellation "+i;
                card.GetComponent<Image>().color=new Color(.07f,.13f,.23f,1);
                if(i<0)
                {
                    var star=Box("North star",card.transform,new Vector2(0,65),new Vector2(76,76),new Color(1,.82f,.38f));
                    star.raycastTarget=false;star.rectTransform.localRotation=Quaternion.Euler(0,0,45);
                    TextAt(card.transform,"계시",0,-80,205,40,25,Mint);
                    TextAt(card.transform,"모든 영웅에게 내리는 힘",0,-130,205,44,17,Muted);
                }
                else
                {
                    var character=c.Data.characters[i];var frames=RedesignedSprites.Direction(i,0);
                    Sprite portrait=frames!=null && frames.Length>0?frames[0]:character.portrait;
                    if(portrait!=null)Icon(card.transform,portrait,0,60,145,Color.white);
                    TextAt(card.transform,CharacterName(character),0,-82,205,45,24,Mint);
                    TextAt(card.transform,ConstellationNames[i],0,-130,205,40,17,Muted);
                }
            }
            ButtonAt(shop.transform,"메인 메뉴",0,-340,360,65,ShowMenu);
        }
        void ShowCodex(int page,int slice=0)
        {
            codexPage=page;Only(codex);
            for(int i=codex.transform.childCount-1;i>=0;i--){var go=codex.transform.GetChild(i).gameObject;go.SetActive(false);Object.Destroy(go);}
            TextAt(codex.transform,"도감",0,365,1200,65,46,Mint);
            string[] tabs={"공격형 장비","강화형 장비","진화 무기"};
            int total=c.Data.weapons.Length+c.Data.passives.Length+c.Data.weapons.Length;
            int discovered=0;
            foreach(var w in c.Data.weapons) {if(c.Save.Discovered("weapon."+w.id))discovered++;if(c.Save.Discovered("evolution."+w.id))discovered++;}
            foreach(var p in c.Data.passives)if(c.Save.Discovered("passive."+p.id))discovered++;
            TextAt(codex.transform,"모든 장비와 진화 효과  ·  "+discovered+" / "+total,0,305,1200,40,22,Muted);
            for(int t=0;t<3;t++)
            {
                int target=t;var tab=ButtonAt(codex.transform,tabs[t],-340+t*340,235,300,56,()=>ShowCodex(target));
                tab.GetComponent<Image>().color=t==page?new Color(.16f,.42f,.40f):Panel;
            }
            int count=page==1?c.Data.passives.Length:c.Data.weapons.Length;
            int pages=Mathf.CeilToInt(count/8f);slice=Mathf.Clamp(slice,0,Mathf.Max(0,pages-1));
            for(int i=slice*8;i<Mathf.Min(count,(slice+1)*8);i++)
            {
                int local=i-slice*8,col=local%4,row=local/4;float x=-555+col*370,y=80-row*236;
                WeaponData w=page==1?null:c.Data.weapons[i];PassiveEquipmentData p=page==1?c.Data.passives[i]:null;
                string id=page==1?"passive."+p.id:(page==2?"evolution.":"weapon.")+w.id;
                bool known=c.Save.Discovered(id);
                var card=Box("Codex "+id,codex.transform,new Vector2(x,y),new Vector2(340,208),new Color(.065f,.12f,.19f,1));card.raycastTarget=false;
                var rail=Box("Codex accent",card.transform,new Vector2(0,102),new Vector2(336,4),known?Mint:new Color(.17f,.21f,.27f));rail.raycastTarget=false;
                Sprite sprite=page==1?p.icon:page==2?(w.evolvedIcon!=null?w.evolvedIcon:w.icon):w.icon;
                var frame=Box("Icon frame",card.transform,new Vector2(-118,35),new Vector2(88,88),new Color(.12f,.24f,.30f));frame.raycastTarget=false;
                if(sprite!=null)Icon(card.transform,sprite,-118,35,75,known?Color.white:Color.black);
                TextAt(card.transform,known?(page==1?p.title:page==2?w.evolvedTitle:w.title):"???",32,55,205,52,23,known?Mint:Muted);
                TextAt(card.transform,known?(page==2?"진화 조합: "+w.title+" + "+w.evolutionPartner.title+"\n위력 ×"+w.evolvedDamage.ToString("0.0")+" · 범위 ×"+w.evolvedArea.ToString("0.0"):(page==1?p.description:w.description)):"아직 발견하지 못했습니다.",0,-52,305,92,17,known?Color.white:Muted);
            }
            if(pages>1)
            {
                int previous=slice-1,next=slice+1;
                if(slice>0)ButtonAt(codex.transform,"이전",-220,-290,180,48,()=>ShowCodex(page,previous));
                TextAt(codex.transform,(slice+1)+" / "+pages,0,-290,150,44,20,Muted);
                if(next<pages)ButtonAt(codex.transform,"다음",220,-290,180,48,()=>ShowCodex(page,next));
            }
            ButtonAt(codex.transform,"메인 메뉴",0,-356,360,60,ShowMenu);SelectFirst(codex);
        }
        void DrawConstellation()
        {
            bool common=shopConstellation<0;
            CharacterData character=common?null:c.Data.characters[shopConstellation];
            PermanentUpgrade[] upgrades=common?c.Data.commonUpgrades:character.upgrades;
            string prefix=common?"common.":character.id+".";
            string title=common?"계시":ConstellationNames[shopConstellation];
            Color accent=StarColor(shopConstellation);
            TextAt(shop.transform,title,0,235,1300,52,32,accent);
            TextAt(shop.transform,common?"모든 캐릭터에게 적용되는 계시":CharacterName(character)+" · 고유 강화",0,195,1300,36,20,Muted);
            if(!common)
            {
                var emblem=Resources.Load<Texture2D>("Constellations/Character"+shopConstellation);
                if(emblem!=null)
                {
                    var art=Rect("Constellation emblem",shop.transform,0,-15,500,445).gameObject.AddComponent<RawImage>();
                    art.texture=emblem;art.color=new Color(1,1,1,.82f);art.raycastTarget=false;
                }
            }
            else
            {
                for(int ring=0;ring<3;ring++)
                {
                    var halo=Box("Revelation halo",shop.transform,Vector2.zero,Vector2.one*(210+ring*110),new Color(1,.77f,.36f,.09f-ring*.02f));
                    halo.raycastTarget=false;
                }
                TextAt(shop.transform,"✦",0,0,155,155,100,new Color(1,.87f,.52f));
            }
            Vector2[] points=StarPoints(shopConstellation);
            for(int i=0;i<36;i++)
            {
                float x=-720+(i*197)%1440,y=-225+(i*113)%450;
                var dot=Box("Distant star",shop.transform,new Vector2(x,y),Vector2.one*(i%5==0?4:2),new Color(accent.r,accent.g,accent.b,.28f));
                dot.raycastTarget=false;
            }
            for(int i=0;i<points.Length-1;i++)ShopLine(common?Vector2.zero:points[i],points[i+1],accent);
            if(common)ShopLine(Vector2.zero,points[0],accent);
            if(shopConstellation==2 || shopConstellation==3)ShopLine(points[points.Length-1],points[0],accent);
            for(int i=0;i<upgrades.Length && i<points.Length;i++)ShopNode(upgrades[i],prefix,points[i],accent);
            ButtonAt(shop.transform,"강화 선택",-230,-345,330,65,ShowShop);
            ButtonAt(shop.transform,"메인 메뉴",230,-345,330,65,ShowMenu);
        }
        static Color StarColor(int index)
        {
            switch(index)
            {
                case 0:return new Color(.45f,.95f,1f);
                case 1:return new Color(.55f,1f,.65f);
                case 2:return new Color(1f,.78f,.35f);
                case 3:return new Color(.75f,.58f,1f);
                case 4:return new Color(1f,.48f,.68f);
                default:return new Color(1f,.88f,.52f);
            }
        }
        static Vector2[] StarPoints(int index)
        {
            switch(index)
            {
                case 0:return new[]{new Vector2(-480,105),new Vector2(-480,-125),new Vector2(480,-125),new Vector2(480,105)};
                case 1:return new[]{new Vector2(-490,-110),new Vector2(-465,125),new Vector2(465,125),new Vector2(490,-110)};
                case 2:return new[]{new Vector2(-480,120),new Vector2(480,120),new Vector2(480,-120),new Vector2(-480,-120)};
                case 3:return new[]{new Vector2(-490,120),new Vector2(490,120),new Vector2(490,-120),new Vector2(-490,-120)};
                case 4:return new[]{new Vector2(-480,-115),new Vector2(-480,115),new Vector2(480,115),new Vector2(480,-115)};
                default:return new[]{new Vector2(-420,145),new Vector2(-540,0),new Vector2(-420,-145),new Vector2(0,-180),new Vector2(420,-145),new Vector2(540,0),new Vector2(420,145)};
            }
        }
        void ShopLine(Vector2 from,Vector2 to,Color accent)
        {
            Vector2 delta=to-from;
            var line=Box("Constellation link",shop.transform,(from+to)*.5f,new Vector2(delta.magnitude,3),new Color(accent.r,accent.g,accent.b,.38f));
            line.raycastTarget=false;line.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
        }
        void ShopNode(PermanentUpgrade upgrade,string prefix,Vector2 point,Color accent)
        {
            int level=c.Save.Level(prefix+upgrade.id),cost=upgrade.Cost(level);
            bool max=level>=upgrade.maxLevel,affordable=c.Save.Progress.gold>=cost;
            var button=ButtonAt(shop.transform,"",point.x,point.y,225,114,()=>{if(c.Save.Purchase(prefix,upgrade))RefreshShop();});
            button.name="Star "+upgrade.id;
            button.GetComponent<Image>().color=max?new Color(.15f,.25f,.27f):affordable?new Color(.08f,.19f,.29f):new Color(.07f,.12f,.18f);
            button.interactable=!max && affordable;
            var edge=Box("Luminous node border",button.transform,Vector2.zero,new Vector2(221,110),new Color(accent.r,accent.g,accent.b,max?.28f:.12f));edge.raycastTarget=false;edge.transform.SetAsFirstSibling();
            var star=Box("Star",button.transform,new Vector2(-90,12),new Vector2(27,27),accent);
            star.raycastTarget=false;star.rectTransform.localRotation=Quaternion.Euler(0,0,45);
            TextAt(button.transform,upgrade.title,17,34,175,34,20,accent);
            TextAt(button.transform,"레벨 "+level+"/"+upgrade.maxLevel+"  ·  "+(max?"완성":"골드 "+cost),17,3,175,26,16,Color.white);
            TextAt(button.transform,KoreanText.StatEffect(upgrade.bonus),17,-34,188,32,15,Muted);
        }
        void NewModal(string title,string subtitle)
        {HideModal();modal=Overlay("Campaign Modal");TextAt(modal.transform,title,0,340,1450,80,45,Mint);TextAt(modal.transform,subtitle,0,270,1450,65,22,Muted);}
        public void HideModal(){if(modal!=null){modal.SetActive(false);Object.Destroy(modal);modal=null;}}
        GameObject Overlay(string name){var image=Box(name,canvas.transform,Vector2.zero,Vector2.zero,Ink);Stretch(image.rectTransform);return image.gameObject;}
        RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
        {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0.5f,0.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        Image Box(string name,Transform parent,Vector2 position,Vector2 size,Color color)
        {var r=Rect(name,parent,position.x,position.y,size.x,size.y);var image=r.gameObject.AddComponent<Image>();image.color=color;return image;}
        Text TextAt(Transform parent,string content,float x,float y,float w,float h,int size,Color color)
        {var r=Rect("Text",parent,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=content;t.fontSize=size;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(14,size-4);t.resizeTextMaxSize=size;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        Button ButtonAt(Transform parent,string title,float x,float y,float w,float h,UnityEngine.Events.UnityAction action)
        {var image=Box(title+" Button",parent,new Vector2(x,y),new Vector2(w,h),Panel);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);b.gameObject.AddComponent<ButtonHoverFeedback>();TextAt(b.transform,title,0,0,w-16,h-8,24,Color.white);return b;}
        Image Icon(Transform parent,Sprite sprite,float x,float y,float size,Color color)
        {var image=Box("Icon",parent,new Vector2(x,y),Vector2.one*size,color);image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;}
        static void SelectFirst(GameObject root)
        {if(EventSystem.current!=null){var b=root.GetComponentInChildren<Button>();EventSystem.current.SetSelectedGameObject(b!=null?b.gameObject:null);}}
    }

    // Clear pointer and keyboard focus feedback on every campaign button.
    internal sealed class ButtonHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        Button button;
        Outline outline;
        Vector3 originalScale;
        bool pointer, selected;

        void Awake()
        {
            button=GetComponent<Button>();
            // Unity's programmatic first selection must not look like a mouse hover.
            var colors=button.colors;
            colors.selectedColor=colors.normalColor;
            colors.highlightedColor=colors.normalColor;
            button.colors=colors;
            originalScale=transform.localScale;
            outline=gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(.38f,1f,.82f,1f);
            outline.effectDistance=new Vector2(3,-3);
            outline.useGraphicAlpha=false;
            outline.enabled=false;
        }

        void Update()
        {
            if(button!=null && !button.interactable && (pointer || selected))
            {pointer=false;selected=false;Refresh();}
        }

        public void OnPointerEnter(PointerEventData eventData){pointer=true;Refresh();}
        public void OnPointerExit(PointerEventData eventData){pointer=false;Refresh();}
        public void OnSelect(BaseEventData eventData)
        {
            // AxisEventData is sent by deliberate keyboard/gamepad navigation.
            // SelectFirst uses plain BaseEventData when a page opens.
            selected=eventData is AxisEventData;
            Refresh();
        }
        public void OnDeselect(BaseEventData eventData){selected=false;Refresh();}
        void OnDisable(){pointer=false;selected=false;Refresh();}

        void Refresh()
        {
            bool active=button!=null && button.interactable && (pointer || selected);
            if(outline!=null)outline.enabled=active;
            transform.localScale=originalScale*(active?1.035f:1f);
        }
    }

}
