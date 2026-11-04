using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace PixelSurvivor
{
    // Builds the expansion UI on a separate canvas, preserving the existing HUD and scene objects.
    public sealed class CampaignUI
    {
        readonly CampaignController c;
        Canvas canvas;Font font;GameObject menu,characters,stages,shop,hud,modal;
        Button characterNext,stageStart;
        readonly List<Image> weaponIcons=new List<Image>(),passiveIcons=new List<Image>();
        Text selection,gold,status,notice,boss,slotInfo,stageInfo;
        readonly List<Button> characterButtons=new List<Button>(),stageButtons=new List<Button>();
        readonly List<Text> weaponSlots=new List<Text>(),passiveSlots=new List<Text>();
        Text practiceLabel;float noticeUntil;
        static readonly Color Ink=new Color(0.025f,0.05f,0.09f,0.98f), Panel=new Color(0.08f,0.15f,0.22f,1), Mint=new Color(0.38f,0.95f,0.78f), Muted=new Color(0.6f,0.68f,0.75f);
        public CampaignUI(CampaignController owner){c=owner;}
        public void Build()
        {
            font=KoreanText.Font;
            var root=new GameObject("Campaign Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=50;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=0.5f;
            menu=Overlay("Main Menu");characters=Overlay("Character Select");stages=Overlay("Stage Select");shop=Overlay("Permanent Upgrades");hud=Box("Equipment HUD",root.transform,Vector2.zero,Vector2.zero,Color.clear).gameObject;
            Stretch(hud.GetComponent<RectTransform>());hud.GetComponent<Image>().raycastTarget=false;
            TextAt(menu.transform,"픽셀 서바이버",0,245,1400,100,64,Mint);
            TextAt(menu.transform,"무한 생존",0,155,1400,60,28,Muted);
            TextAt(menu.transform,"장비를 조합하고 보스를 쓰러뜨리며 오래 살아남으세요.",0,85,1400,55,24,Color.white);
            ButtonAt(menu.transform,"시작",0,-30,450,75,()=>ShowCharacters(true));
            ButtonAt(menu.transform,"영구 강화",0,-130,450,75,()=>ShowShop());
            ButtonAt(menu.transform,"종료",0,-230,450,65,()=>Quit());
            var practice=ButtonAt(menu.transform,"",0,-340,1200,50,()=>{c.TogglePractice();RefreshMenu();});practiceLabel=practice.GetComponentInChildren<Text>();
            gold=TextAt(menu.transform,"",0,360,1000,45,25,new Color(1,0.85f,0.35f));
            TextAt(characters.transform,"캐릭터 선택",0,365,1450,70,44,Mint);
            selection=TextAt(characters.transform,"함께할 캐릭터를 선택하세요.",0,295,1450,55,23,Muted);
            for(int i=0;i<c.Data.characters.Length;i++)
            {
                int id=i;var data=c.Data.characters[i];float x=-608+i*304;
                var b=ButtonAt(characters.transform,"",x,0,288,490,()=>{c.SelectCharacter(id);RefreshMenu();});b.name="Character "+i+" Button";characterButtons.Add(b);
                Icon(b.transform,data.portrait,0,165,96,Color.white);
                TextAt(b.transform,data.title,0,90,270,65,24,Mint);
                TextAt(b.transform,data.role+"\n체력 "+data.maxHP+"    공격력 "+data.damage+"\n이동속도 "+data.moveSpeed+"\n초당 기본 공격 "+(1/data.attackInterval).ToString("0.00")+"\n\n시작 무기\n"+data.startingWeapon.title+"\n\n"+data.passiveDescription,0,-65,264,270,19,Color.white);
            }
            ButtonAt(characters.transform,"뒤로",-360,-345,320,65,()=>ShowMenu());
            characterNext=ButtonAt(characters.transform,"다음: 스테이지 선택",360,-345,440,65,()=>ShowStages());characterNext.interactable=false;
            TextAt(stages.transform,"스테이지 선택",0,355,1400,70,46,Mint);
            for(int i=0;i<c.Data.stages.Length;i++)
            {int id=i;var b=ButtonAt(stages.transform,c.Data.stages[i].title,-480+i*480,190,440,100,()=>{c.SelectStage(id);RefreshMenu();});stageButtons.Add(b);}
            stageInfo=TextAt(stages.transform,"",0,-30,1400,260,25,Color.white);
            ButtonAt(stages.transform,"뒤로",-360,-300,320,65,()=>ShowCharacters(false));
            stageStart=ButtonAt(stages.transform,"전투 시작",360,-300,440,75,()=>c.Game.BeginRun());
            for(int i=0;i<7;i++)
            {
                int slot=i;float x=-660+i*220;
                var wb=ButtonAt(hud.transform,"빈 슬롯",x,-285,210,70,()=>InspectWeapon(slot));
                weaponSlots.Add(wb.GetComponentInChildren<Text>());weaponSlots[i].fontSize=16;weaponSlots[i].resizeTextMinSize=14;weaponSlots[i].resizeTextMaxSize=16;
                weaponIcons.Add(Icon(wb.transform,null,-78,0,36,Color.white));OffsetSlotText(weaponSlots[i]);
                var pb=ButtonAt(hud.transform,"빈 슬롯",x,-365,210,60,()=>InspectPassive(slot));
                passiveSlots.Add(pb.GetComponentInChildren<Text>());passiveSlots[i].fontSize=16;passiveSlots[i].resizeTextMinSize=14;passiveSlots[i].resizeTextMaxSize=16;
                passiveIcons.Add(Icon(pb.transform,null,-78,0,36,Color.white));OffsetSlotText(passiveSlots[i]);
            }
            status=TextAt(hud.transform,"",-90,315,1180,50,20,Mint);
            boss=TextAt(hud.transform,"",0,270,1000,40,22,new Color(1,0.5f,0.4f));
            notice=TextAt(hud.transform,"",0,220,1400,45,24,Color.white);
            slotInfo=TextAt(hud.transform,"장비 슬롯을 누르면 진화 조건을 볼 수 있습니다.",0,-210,1450,65,18,Muted);
            ButtonAt(hud.transform,"일시정지",690,315,170,40,()=>ShowPause());
            shop.SetActive(false);hud.SetActive(false);characters.SetActive(false);stages.SetActive(false);
        }
        void RefreshMenu()
        {
            for(int i=0;i<characterButtons.Count;i++)
                characterButtons[i].GetComponent<Image>().color=c.HasCharacterSelection && i==c.CharacterIndex?new Color(0.15f,0.42f,0.35f):Panel;
            for(int i=0;i<stageButtons.Count;i++)
            {
                bool unlocked=i<c.Save.Progress.unlockedStages;stageButtons[i].interactable=unlocked;
                stageButtons[i].GetComponentInChildren<Text>().text=c.Data.stages[i].title+(unlocked?"":"  [잠김]");
                stageButtons[i].GetComponent<Image>().color=i==c.StageIndex?new Color(0.15f,0.42f,0.35f):Panel;
            }
            selection.text=c.HasCharacterSelection?"선택: "+c.Character.title:"함께할 캐릭터를 선택하세요.";
            characterNext.interactable=c.HasCharacterSelection;stageStart.interactable=c.HasCharacterSelection;
            stageInfo.text=c.StageData.description+"\n\n캐릭터: "+c.Character.title+"\n최고 생존 기록: "+SurvivalRules.Clock(c.Save.Best(c.Character.id,c.StageData.id))+"\n\n제한시간 없이 생존하세요. 보스는 10분마다 등장합니다.\n20분 보스를 처치하면 다음 구역이 열리고 현재 전투는 계속됩니다.";
            gold.text="보유 골드  "+c.Save.Progress.gold;
            practiceLabel.text=c.Practice?"빠른 테스트 켜짐 · 보스 30초 간격 · 골드/기록/해금 저장 안 함":"빠른 테스트 꺼짐 · 무한 생존";
        }
        void Only(GameObject screen)
        {HideModal();menu.SetActive(screen==menu);characters.SetActive(screen==characters);stages.SetActive(screen==stages);shop.SetActive(screen==shop);hud.SetActive(screen==hud);}
        public void ShowMenu(){Only(menu);RefreshMenu();SelectFirst(menu);}
        void ShowCharacters(bool fresh){if(fresh)c.ClearCharacterSelection();Only(characters);RefreshMenu();SelectFirst(characters);}
        void ShowStages(){if(!c.HasCharacterSelection)return;Only(stages);RefreshMenu();SelectFirst(stages);}
        public void ShowGameplay(){Only(hud);slotInfo.text="공격형 7칸 / 강화형 7칸 · 슬롯을 누르면 진화 정보를 볼 수 있습니다.";Tick();}
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
        static void OffsetSlotText(Text text){text.rectTransform.anchoredPosition=new Vector2(23,0);text.rectTransform.sizeDelta=new Vector2(153,62);}
        public void Tick()
        {
            if(hud==null)return;
            for(int i=0;i<7;i++)
            {
                if(i<c.Equipment.Weapons.Count){var w=c.Equipment.Weapons[i];weaponSlots[i].text=w.Name+"\n"+(w.evolved?"진화 완료":w.level>=w.Maximum?"최대":"레벨 "+w.level)+(c.Equipment.CanEvolve(w)?" · 진화 가능":"");weaponSlots[i].color=c.Equipment.CanEvolve(w)?Mint:Color.white;}
                else weaponSlots[i].text="공격형 "+(i+1)+"\n빈 슬롯";
                weaponIcons[i].gameObject.SetActive(i<c.Equipment.Weapons.Count);
                if(i<c.Equipment.Weapons.Count){var w=c.Equipment.Weapons[i];weaponIcons[i].sprite=w.evolved?w.data.evolvedIcon:w.data.icon;}
                if(i<c.Equipment.Passives.Count){var p=c.Equipment.Passives[i];passiveSlots[i].text=p.data.title+"\n"+(p.level>=p.data.maxLevel?"최대":"레벨 "+p.level);}
                else passiveSlots[i].text="강화형 "+(i+1)+"\n빈 슬롯";
                passiveIcons[i].gameObject.SetActive(i<c.Equipment.Passives.Count);
                if(i<c.Equipment.Passives.Count)passiveIcons[i].sprite=c.Equipment.Passives[i].data.icon;
            }
            status.text=c.StageData.title+"  |  "+"생존 시간 "+SurvivalRules.Clock(c.Stage.Elapsed)+"  |  획득 골드 "+c.RunGold+(c.Practice?"  [테스트 · 저장 안 함]":"");
            var b=c.Stage.CurrentBoss;boss.text=b!=null && b.IsAlive?(b.IsFinalBoss?"해금 보스":"보스")+"  체력 "+Mathf.CeilToInt(b.CurrentHP)+" / "+Mathf.CeilToInt(b.MaxHP):"";
            if(Time.unscaledTime>noticeUntil)notice.text="";
        }
        public void Notify(string message){if(notice==null)return;notice.text=message;noticeUntil=Time.unscaledTime+4;}
        void InspectWeapon(int index)
        {if(index<c.Equipment.Weapons.Count){var w=c.Equipment.Weapons[index];slotInfo.text=w.Name+" - "+c.Equipment.EvolutionStatus(w.data).Replace("\n","  |  ");}}
        void InspectPassive(int index)
        {if(index<c.Equipment.Passives.Count){var p=c.Equipment.Passives[index];slotInfo.text=p.data.title+": "+p.data.description+"  |  "+c.Equipment.Synergy(p.data).Replace("\n","  /  ");}}
        public void ShowChoices(EquipmentChoice[] choices)
        {
            NewModal("레벨 상승 · 장비 선택", "공격형 "+c.Equipment.Weapons.Count+" / 7     강화형 "+c.Equipment.Passives.Count+" / 7");
            for(int i=0;i<choices.Length;i++)
            {
                int index=i;var choice=choices[i];float x=(i-(choices.Length-1)*0.5f)*470;
                var button=ButtonAt(modal.transform,"",x,-20,440,430,()=>c.Game.SelectUpgrade(index));
                var root=button.transform;
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
                TextAt(root,title,0,135,410,85,27,Mint);TextAt(root,description,0,35,405,110,20,Color.white);
                TextAt(root,condition,0,-100,400,145,19,choice.kind==EquipmentChoiceKind.Weapon&&!owned?Muted:Mint);
                if(icon!=null)Icon(root,icon,-175,185,38,Color.white);
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
            NewModal("게임 종료",c.StageData.title+" / "+c.Character.title);
            TextAt(modal.transform,"생존 시간  "+SurvivalRules.Clock(c.Stage.Elapsed)+"\n도달 레벨  "+c.Game.Level.Level+"    총 처치  "+c.Game.KillCount+
                "\n엘리트 처치  "+c.EliteKills+"    보스 처치  "+c.BossKills+"\n획득 골드  "+c.RunGold+
                "\n최고 생존 기록  "+SurvivalRules.Clock(c.Save.Best(c.Character.id,c.StageData.id))+"\n"+
                (c.Practice?"테스트 전투의 골드, 기록, 해금은 저장되지 않습니다.":"골드와 성장 기록을 저장했습니다. 전투 장비는 초기화됩니다."),0,10,1400,350,28,Color.white);
            ButtonAt(modal.transform,"메인 메뉴",-260,-280,420,70,()=>c.Game.ReturnToMenu());
            ButtonAt(modal.transform,"다시 시작",260,-280,420,70,()=>c.Game.BeginRun());SelectFirst(modal);
        }
        void ShowPause()
        {
            if(!c.Game.IsPlaying)return;c.Game.TogglePause();NewModal("일시정지","전투가 일시정지되었습니다.");
            ButtonAt(modal.transform,"계속하기",0,0,400,80,()=>{HideModal();c.Game.TogglePause();});SelectFirst(modal);
        }
        void ShowShop()
        {
            Only(shop);
            for(int i=shop.transform.childCount-1;i>=0;i--){var go=shop.transform.GetChild(i).gameObject;go.SetActive(false);Object.Destroy(go);}
            TextAt(shop.transform,"영구 강화",0,370,1300,65,44,Mint);
            TextAt(shop.transform,"보유 골드 "+c.Save.Progress.gold+"  |  강화는 다음 전투부터 적용됩니다.",0,305,1400,50,23,Color.white);
            TextAt(shop.transform,"모든 캐릭터 공통",-380,250,700,45,26,Mint);TextAt(shop.transform,c.Character.title.ToUpper(),380,250,700,45,26,Mint);
            for(int i=0;i<c.Data.commonUpgrades.Length;i++)UpgradeRow(c.Data.commonUpgrades[i],"common.",-380,185-i*64);
            for(int i=0;i<c.Character.upgrades.Length;i++)UpgradeRow(c.Character.upgrades[i],c.Character.id+".",380,185-i*80);
            ButtonAt(shop.transform,"강화할 캐릭터 변경",380,-235,500,50,()=>{c.SelectCharacter((c.CharacterIndex+1)%c.Data.characters.Length);ShowShop();});
            ButtonAt(shop.transform,"뒤로",380,-330,360,60,()=>ShowMenu());SelectFirst(shop);
        }
        void UpgradeRow(PermanentUpgrade upgrade,string prefix,float x,float y)
        {
            int level=c.Save.Level(prefix+upgrade.id);bool max=level>=upgrade.maxLevel;int cost=upgrade.Cost(level);
            string effect=KoreanText.StatEffect(upgrade.bonus);
            var button=ButtonAt(shop.transform,upgrade.title+"  "+level+"/"+upgrade.maxLevel+"   "+(max?"최대":"골드 "+cost)+"\n"+effect,x,y,700,58,()=>{c.Save.Purchase(prefix,upgrade);ShowShop();});
            button.GetComponentInChildren<Text>().fontSize=19;button.GetComponentInChildren<Text>().resizeTextMinSize=16;button.GetComponentInChildren<Text>().resizeTextMaxSize=19;button.interactable=!max && c.Save.Progress.gold>=cost;
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
        {var image=Box(title+" Button",parent,new Vector2(x,y),new Vector2(w,h),Panel);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);TextAt(b.transform,title,0,0,w-16,h-8,24,Color.white);return b;}
        Image Icon(Transform parent,Sprite sprite,float x,float y,float size,Color color)
        {var image=Box("Icon",parent,new Vector2(x,y),Vector2.one*size,color);image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;}
        static void SelectFirst(GameObject root)
        {if(EventSystem.current!=null){var b=root.GetComponentInChildren<Button>();EventSystem.current.SetSelectedGameObject(b!=null?b.gameObject:null);}}
    }
}
