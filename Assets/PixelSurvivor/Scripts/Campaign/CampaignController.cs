using UnityEngine;
namespace PixelSurvivor
{
    public sealed class CampaignController : MonoBehaviour
    {
        [SerializeField] CampaignData data;
        public CampaignData Data => data;
        public bool IsConfigured => data!=null && data.IsConfigured;
        public GameManager Game {get;private set;}
        public EquipmentManager Equipment {get;private set;}
        public SaveManager Save {get;private set;}
        public TreasureChestSystem Chests {get;private set;}
        public StageDirector Stage {get;private set;}
        public CombatArsenal Arsenal {get;private set;}
        public FieldSystem Field {get;private set;}
        public SupportUnitManager Supports {get;private set;}
        public CampaignUI UI {get;private set;}
        public int CharacterIndex {get;private set;}
        public int StageIndex {get;private set;}
        public CharacterData Character => data.characters[CharacterIndex];
        public StageData StageData => data.stages[StageIndex];
        public int RunGold {get;private set;}
        public int LastGoldAward {get;private set;}
        public int EliteKills {get;private set;}
        public int BossKills {get;private set;}
        public bool HasCharacterSelection {get;private set;}
        public bool HasEquipmentChoices => Equipment.RollChoices(data).Length>0;
        public float BestTime => Mathf.Max(Save.Best(Character.id,StageData.id), Practice?0:Stage.Elapsed);
        public bool Practice {get;private set;}
        public float TimelineScale => Practice ? 20 : 1;
        float saveCountdown, expRemainder;
        EquipmentChoice[] choices;
        public void Configure(CampaignData configuration) {data=configuration;}
        public void Initialize(GameManager owner)
        {
            KoreanText.LocalizeData(data);
            Game=owner; Equipment=new EquipmentManager(); Save=new SaveManager(data);
            Chests=new TreasureChestSystem(this); Stage=new StageDirector(this);
            Arsenal=new CombatArsenal(this); Field=new FieldSystem(this); Supports=new SupportUnitManager(this);
            UI=new CampaignUI(this); UI.Build(); UI.ShowMenu();
        }
        // Used by automated tests or disposable sessions; does not touch the player's saved profile.
        public void UseTemporaryProgress(){if(Game.State==RunState.Menu)Save=new SaveManager(data,null);}
        public bool SelectStage(int index)
        { if(Game.State!=RunState.Menu || index<0 || index>=data.stages.Length || index>=Save.Progress.unlockedStages) return false; StageIndex=index;return true; }
        public void SelectCharacter(int index)
        { if(Game.State==RunState.Menu && index>=0 && index<data.characters.Length && Save.Progress.unlockedCharacters.Contains(data.characters[index].id)) {CharacterIndex=index;HasCharacterSelection=true;} }
        public void TogglePractice() { if(Game.State==RunState.Menu) Practice=!Practice; }
        public void BeginRun()
        {
            RunGold=0; expRemainder=0; EliteKills=0;BossKills=0;saveCountdown=15;
            var animator=Game.Player.GetComponent<PlayerSpriteAnimator>();
            var redesigned=RedesignedSprites.Direction(CharacterIndex,0);
            if(animator!=null)animator.Configure(redesigned??Character.downFrames,
                RedesignedSprites.Direction(CharacterIndex,1)??Character.upFrames,
                RedesignedSprites.Direction(CharacterIndex,2)??Character.leftFrames,
                RedesignedSprites.Direction(CharacterIndex,3)??Character.rightFrames);
            Game.Player.GetComponent<SpriteRenderer>().color=redesigned!=null?Color.white:Character.tint;
            Equipment.Reset(Character.startingWeapon); DiscoverWeapon(Character.startingWeapon); RefreshStats(false); Game.Health.ResetHealth();
            Stage.Begin(); Arsenal.Reset(); Field.Reset(); Supports.Reset(); UI.ShowGameplay();
        }
        public void Tick(float dt)
        {
            Stage.Tick(dt); if(!Game.IsPlaying) return;
            Arsenal.Tick(dt); if(!Game.IsPlaying) return;
            Field.Tick(dt); Supports.Tick(dt);
            saveCountdown-=dt;if(saveCountdown<=0){ if(!Practice) {Save.Record(Character.id,StageData.id,Stage.Elapsed);Save.Save();}saveCountdown=15; }
            UI.Tick();
        }
        public float Bonus(StatKind kind)
        {
            float value=Equipment.Bonus(kind);
            foreach(var b in Character.inherentBonuses) if(b.kind==kind) value+=b.amount;
            foreach(var u in data.commonUpgrades) if(u.bonus.kind==kind) value+=u.bonus.amount*Mathf.Min(u.maxLevel,Save.Level("common."+u.id));
            foreach(var u in Character.upgrades) if(u.bonus.kind==kind) value+=u.bonus.amount*Mathf.Min(u.maxLevel,Save.Level(Character.id+"."+u.id));
            return value;
        }
        public float DamageFactor
        {
            get
            {
                float missing=1-Mathf.Clamp01(Game.Health.CurrentHP/Game.Stats.MaxHP);
                return Game.Stats.Damage/10f*(1+missing*Bonus(StatKind.LowHPDamage));
            }
        }
        public float CooldownFactor => Mathf.Clamp(Game.Stats.AttackInterval/0.8f,0.12f,3);
        public float AreaFactor => Mathf.Max(0.2f,1+Bonus(StatKind.Area));
        public float DurationFactor => Mathf.Max(0.2f,1+Bonus(StatKind.Duration));
        public int ExtraProjectiles => Mathf.Max(0,Mathf.FloorToInt(Bonus(StatKind.Projectiles)));
        public float PickupFactor => Mathf.Max(0.2f,1+Bonus(StatKind.Pickup));
        public void RefreshStats(bool healAddedHP)
        {
            float previous=Game.Stats.MaxHP;
            Game.Stats.SetRunValues(Character.maxHP+Bonus(StatKind.MaxHP),Character.moveSpeed*(1+Bonus(StatKind.MoveSpeed)),
                Character.damage*(1+Bonus(StatKind.Damage)),Character.attackInterval*Mathf.Clamp(1-Bonus(StatKind.Cooldown),0.15f,2),8);
            if(healAddedHP) Game.Health.Heal(Mathf.Max(0,Game.Stats.MaxHP-previous));
        }
        public float IncomingDamage(float damage)
        { if(Random.value<Mathf.Clamp(Bonus(StatKind.Dodge),0,0.5f)) return 0;return Mathf.Max(1,damage-Bonus(StatKind.Armor)); }
        public float RollDamage(float damage) => damage*(Random.value<Mathf.Clamp01(Bonus(StatKind.Critical)) ? 2 : 1);
        public void OnKill() {float heal=Bonus(StatKind.Leech);if(heal>0)Game.Health.Heal(heal);}
        public int ScaleExperience(int raw)
        { expRemainder+=raw*Mathf.Max(0,1+Bonus(StatKind.Experience));int whole=Mathf.FloorToInt(expRemainder);expRemainder-=whole;return whole; }
        public void AddGold(int amount)
        {
            LastGoldAward=Mathf.Max(0,Mathf.RoundToInt(amount*(1+Bonus(StatKind.Gold))));
            RunGold=(int)System.Math.Min(int.MaxValue,(long)RunGold+LastGoldAward);if(!Practice)Save.AddGold(LastGoldAward);
        }
        public void ShowChoices()
        { choices=Equipment.RollChoices(data);UI.ShowChoices(choices); }
        public bool SelectChoice(int index)
        {
            if(choices==null || index<0 || index>=choices.Length) return false;
            var c=choices[index];
            switch(c.kind)
            {
                case EquipmentChoiceKind.Weapon: Equipment.Add(c.weapon); DiscoverWeapon(c.weapon); break;
                case EquipmentChoiceKind.Passive: Equipment.Add(c.passive); DiscoverPassive(c.passive); break;
            }
            choices=null;RefreshStats(true);UI.HideModal();return true;
        }
        public void DiscoverWeapon(WeaponData weapon){if(!Practice && weapon!=null)Save.Discover("weapon."+weapon.id);}
        public void DiscoverPassive(PassiveEquipmentData passive){if(!Practice && passive!=null)Save.Discover("passive."+passive.id);}
        public void DiscoverEvolution(WeaponData weapon){if(!Practice && weapon!=null)Save.Discover("evolution."+weapon.id);}
        public void OpenChest(bool boss,bool final)
        {
            if(!Game.IsPlaying)return;
            Game.PauseForChest();
            var results=Chests.Open(boss);UI.ShowChest(results,boss,final);
        }
        public void CloseChest()
        {
            if(Game.State!=RunState.Treasure)return;
            UI.HideModal(); Game.ResumeFromChest();
        }
        public void Finish(bool won)
        {
            if(!Practice){Save.Record(Character.id,StageData.id,Stage.Elapsed);Save.Save();}
            UI.ShowResult(false);Supports.Reset();Equipment.Reset(null);
        }
        public void RegisterSpecialKill(Enemy enemy)
        {
            if(enemy.IsElite)EliteKills++;
            if(!enemy.IsBoss)return;
            BossKills++;
            if(enemy.IsFinalBoss && !Practice && StageIndex+1<data.stages.Length && Save.Progress.unlockedStages<StageIndex+2)
            {
                Save.UnlockNext(StageIndex,data.stages.Length);
                UI.Notify("구역 "+(StageIndex+2)+" 해금! 현재 구역에서 계속 생존하세요.");
            }
        }
        public void ClearCharacterSelection(){HasCharacterSelection=false;}
        public void LeaveRunEarly()
        {
            if((Game.State!=RunState.Paused && Game.State!=RunState.Playing) || Practice)return;
            Save.Record(Character.id,StageData.id,Stage.Elapsed);
            Save.Save();
        }
        public void ReturnMenu() { Supports.Reset(); Equipment.Reset(null); HasCharacterSelection=false; UI.ShowMenu(); }
        void Persist()
        {
            if(Save==null)return;
            if(!Practice && Game!=null && Game.State!=RunState.Menu && Stage!=null && HasCharacterSelection)
                Save.Record(Character.id,StageData.id,Stage.Elapsed);
            Save.Save();
        }
        void OnApplicationQuit(){Persist();}
        void OnApplicationPause(bool paused){if(paused)Persist();}
        void OnDestroy(){Persist();}
    }
}
