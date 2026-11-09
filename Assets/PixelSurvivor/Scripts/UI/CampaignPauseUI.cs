using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
namespace PixelSurvivor
{
    public sealed partial class CampaignUI
    {
        InputActionReference cancelAction;
        enum PausePage { Information, Options, Recipes }
        PausePage pausePage;
        public void HandlePauseInput()
        {
            if((c.Game.IsPlaying || c.Game.State==RunState.Paused) && Keyboard.current!=null && Keyboard.current.mKey.wasPressedThisFrame)
            { c.Game.ReturnToMenu();return; }
            if(cancelAction==null)
            {
                var input=Object.FindAnyObjectByType<InputSystemUIInputModule>();
                if(input!=null)cancelAction=input.cancel;
            }
            if(cancelAction==null || !cancelAction.action.WasPressedThisFrame())return;
            if(c.Game.IsPlaying)ShowPause();
            else if(c.Game.State==RunState.Paused)
            {
                if(pausePage!=PausePage.Information)ShowPauseInformation();else ResumePause();
            }
            // ESC cannot dismiss a mandatory level-up, chest or result screen.
        }
        public void ShowPause()
        {
            if(!c.Game.IsPlaying)return;
            c.Game.TogglePause();ShowPauseInformation();
        }
        void ResumePause()
        {
            if(c.Game.State!=RunState.Paused)return;
            HideModal();c.Game.TogglePause();
            if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);
        }
        void ShowPauseInformation()
        {
            pausePage=PausePage.Information;
            NewModal("일시정지",c.Character.title.Split('·')[0].Trim()+"  ·  LV"+c.Game.Level.Level+"  ·  "+SurvivalRules.Clock(c.Stage.Elapsed));
            var plate=Box("Character information",modal.transform,new Vector2(-545,-10),new Vector2(390,450),Panel);
            var frames=RedesignedSprites.Direction(c.CharacterIndex,0);
            var portrait=frames!=null && frames.Length>0?frames[0]:c.Character.portrait;
            Icon(plate.transform,portrait,0,158,90,Color.white);
            TextAt(plate.transform,"캐릭터 정보",0,90,350,42,27,Mint);
            var g=c.Game;
            string stats="체력  "+Mathf.CeilToInt(g.Health.CurrentHP)+" / "+g.Stats.MaxHP.ToString("0")+
                "\n기본 공격력  "+g.Stats.Damage.ToString("0.#")+"\n이동속도  "+g.Stats.MoveSpeed.ToString("0.00")+
                "\n공격 배율  "+c.DamageFactor.ToString("0.00")+"×"+
                "\n재사용 대기시간  "+c.CooldownFactor.ToString("0.00")+"×"+
                "\n공격 범위  "+c.AreaFactor.ToString("0.00")+"×"+
                "\n지속시간  "+c.DurationFactor.ToString("0.00")+"×"+
                "\n추가 투사체  +"+c.ExtraProjectiles+"\n방어력  "+c.Bonus(StatKind.Armor).ToString("0.#")+
                "\n치명타 확률  "+(Mathf.Clamp01(c.Bonus(StatKind.Critical))*100).ToString("0.#")+"%";
            TextAt(plate.transform,stats,0,-63,350,285,20,Color.white);
            TextAt(modal.transform,"공격형 기술",240,205,950,40,27,Mint);
            TextAt(modal.transform,"강화형 기술",240,15,950,40,27,Mint);
            for(int i=0;i<7;i++)
            {
                float x=-168+i*136;
                var w=i<c.Equipment.Weapons.Count?c.Equipment.Weapons[i]:null;
                EquipmentTile(x,120,w==null?null:w.evolved?w.data.evolvedIcon:w.data.icon,w==null?"":w.Name,
                    w==null?0:w.level,w==null?0:w.Maximum,w!=null&&w.evolved);
                var p=i<c.Equipment.Passives.Count?c.Equipment.Passives[i]:null;
                EquipmentTile(x,-70,p==null?null:p.data.icon,p==null?"":p.data.title,
                    p==null?0:p.level,p==null?0:p.data.maxLevel,false);
            }
            TextAt(modal.transform,"밝은 칸: 현재 레벨    빈 칸: 남은 성장 단계",240,-185,950,45,20,Muted);
            ButtonAt(modal.transform,"옵션",-590,-310,220,65,ShowOptions);
            ButtonAt(modal.transform,"조합법 열기",-285,-310,280,65,ShowRecipes);
            ButtonAt(modal.transform,"메인 메뉴 (M)",75,-310,280,65,()=>c.Game.ReturnToMenu());
            ButtonAt(modal.transform,"계속하기",480,-310,320,65,ResumePause);
            SelectFirst(modal);
        }
        void EquipmentTile(float x,float y,Sprite sprite,string title,int current,int maximum,bool evolved)
        {
            var tile=Box("Equipment level tile",modal.transform,new Vector2(x,y),new Vector2(126,136),Panel);
            if(sprite==null){TextAt(tile.transform,"—",0,10,100,50,25,Muted);return;}
            Icon(tile.transform,sprite,0,33,48,Color.white);
            TextAt(tile.transform,title,0,-14,120,42,17,Color.white);
            LevelPips(tile.transform,current,maximum,-50);
            if(evolved)TextAt(tile.transform,"진화",42,52,42,22,14,new Color(1,.82f,.3f));
        }
        void LevelPips(Transform parent,int current,int maximum,float y)
        {
            maximum=Mathf.Max(1,maximum);
            // Wrap Inspector-customized maxima rather than growing beyond a card.
            int columns=Mathf.Min(10,maximum);float step=Mathf.Min(17,108f/columns);
            for(int i=0;i<maximum;i++)
            {
                int col=i%columns,row=i/columns;
                var outer=Box("Level "+(i+1),parent,new Vector2((col-(columns-1)*.5f)*step,y-row*10),new Vector2(step-3,8),new Color(.35f,.43f,.48f));
                Box("Level light",outer.transform,Vector2.zero,new Vector2(step-5,6),i<current?Mint:new Color(.045f,.07f,.1f));
            }
        }
        void ShowOptions()
        {
            pausePage=PausePage.Options;NewModal("옵션","설정은 즉시 적용됩니다.");
            ButtonAt(modal.transform,"사물 피격 효과  "+(DisplayOptions.HitEffects?"켜짐":"꺼짐"),0,135,650,70,()=>{DisplayOptions.HitEffects=!DisplayOptions.HitEffects;ShowOptions();});
            ButtonAt(modal.transform,"캐릭터 발밑 체력바  "+(DisplayOptions.HealthBar?"켜짐":"꺼짐"),0,35,650,70,()=>{DisplayOptions.HealthBar=!DisplayOptions.HealthBar;ShowOptions();});
            ButtonAt(modal.transform,"전투 효과음  "+(DisplayOptions.Sound?"켜짐":"꺼짐"),0,-65,650,70,()=>{DisplayOptions.Sound=!DisplayOptions.Sound;ShowOptions();});
#if UNITY_EDITOR
            TextAt(modal.transform,"전체 화면 전환은 Windows 실행 파일에서 사용할 수 있습니다.",0,-165,1250,55,22,Muted);
#else
            ButtonAt(modal.transform,"전체 화면 / 창 모드 전환",0,-165,650,70,()=>{Screen.fullScreen=!Screen.fullScreen;});
#endif
            ButtonAt(modal.transform,"뒤로",-240,-300,360,65,ShowPauseInformation);
            ButtonAt(modal.transform,"계속하기",240,-300,360,65,ResumePause);SelectFirst(modal);
        }
        void ShowRecipes()
        {
            pausePage=PausePage.Recipes;NewModal("조합법","공격형 최대 레벨 + 조합 강화형 보유 → 보물상자에서 진화 · 강화형은 소모되지 않습니다.");
            for(int i=0;i<c.Data.weapons.Length;i++)
            {
                var w=c.Data.weapons[i];float x=i%2==0?-375:375,y=165-(i/2)*112;
                var row=Box("Recipe "+i,modal.transform,new Vector2(x,y),new Vector2(720,100),Panel);
                Icon(row.transform,w.icon,-315,8,42,Color.white);
                TextAt(row.transform,"+",-268,8,25,32,24,Muted);
                if(w.evolutionPartner!=null)Icon(row.transform,w.evolutionPartner.icon,-220,8,42,Color.white);
                TextAt(row.transform,"→",-170,8,35,32,24,Mint);
                Icon(row.transform,w.evolvedIcon,-120,8,46,Color.white);
                TextAt(row.transform,w.evolvedTitle,105,27,365,35,22,Mint);
                TextAt(row.transform,w.title+" + "+(w.evolutionPartner!=null?w.evolutionPartner.title:"미설정"),105,-8,365,32,17,Color.white);
                TextAt(row.transform,c.Equipment.EvolutionStatus(w).Replace("\n"," · "),30,-37,620,24,15,Muted);
            }
            ButtonAt(modal.transform,"뒤로",-240,-330,360,65,ShowPauseInformation);
            ButtonAt(modal.transform,"계속하기",240,-330,360,65,ResumePause);SelectFirst(modal);
        }
    }
}
