using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace PixelSurvivor.Editor
{
    public static class CampaignSetup
    {
        const string Root=SurvivorSetup.Root+"/Data";
        static T Asset<T>(string name,Action<T> initialize) where T:ScriptableObject
        {
            string path=Root+"/"+name+".asset";var value=AssetDatabase.LoadAssetAtPath<T>(path);
            if(value!=null)return value;
            value=ScriptableObject.CreateInstance<T>();initialize(value);AssetDatabase.CreateAsset(value,path);return value;
        }
        static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(SurvivorSetup.Root+"/Art/"+name+".png");
        static PermanentUpgrade Upgrade(string id,string title,StatKind kind,float amount,int cost=40,int max=10)
        {return new PermanentUpgrade{id=id,title=title,bonus=new StatBonus(kind,amount),baseCost=cost,maxLevel=max,costGrowth=1.4f};}
        public static CampaignData EnsureData()
        {
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            // Import new standalone icons. No spritesheet slicing or extra packages are required.
            foreach(string path in Directory.GetFiles(SurvivorSetup.Root+"/Art","*.png"))
            {
                var t=AssetImporter.GetAtPath(path.Replace('\\','/')) as TextureImporter;if(t==null)continue;
                if(t.textureType==TextureImporterType.Sprite && t.filterMode==FilterMode.Point)continue;
                t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.spritePixelsPerUnit=32;
                t.filterMode=FilterMode.Point;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();
            }
            string[] pn={"Might Sigil","Life Seed","Clockwork","Wide Lens","Long Candle","Wind Boots","Magnet Core","Twin Prism"};
            string[] pd={"All weapon damage +10% per level.","Maximum HP +20 per level; heal the added amount.","Weapon cooldown -6% per level.","Attack area +12% per level.","Attack duration +20% per level.","Movement speed +8% per level.","Pickup attraction range +25% per level.","Projectile count +1 per level."};
            StatKind[] kinds={StatKind.Damage,StatKind.MaxHP,StatKind.Cooldown,StatKind.Area,StatKind.Duration,StatKind.MoveSpeed,StatKind.Pickup,StatKind.Projectiles};
            float[] amounts={0.1f,20,0.06f,0.12f,0.2f,0.08f,0.25f,1};
            var passives=new PassiveEquipmentData[8];
            for(int i=0;i<8;i++){int k=i;passives[i]=Asset<PassiveEquipmentData>("Passive_"+i,p=>{p.id="p"+k;p.title=pn[k];p.description=pd[k];p.maxLevel=k==7?3:5;p.bonus=new StatBonus(kinds[k],amounts[k]);p.icon=Art("Passive"+k);});}
            string[] wn={"Energy Bolt","Wind Dagger","Light Whip","Orbit Orb","Crescent Scythe","Storm Mark","Pulse Nova","Seeking Spark"};
            string[] en={"Prism Barrage","Gale Knives","Lifeline Lash","Solar Halo","Eclipse Reaper","Thunder Crown","Supernova","Star Swarm"};
            string[] wd={"Fires at the nearest target. Levels add damage and bolts.","Throws knives in your facing direction; pierces targets.","Sweeps a wide arc toward enemies. Extra count adds a rear sweep.","A rotating ring damages enemies around you for several seconds.","A piercing blade travels outward then returns toward you.","Lightning strikes the target and nearby ground.","A circular shockwave damages enemies around you.","Homing sparks continually track nearby enemies."};
            float[] dmg={10,9,20,7,18,24,22,8},cd={0.8f,0.65f,1.4f,5,1.8f,2.4f,2.5f,1},area={1,0.7f,3.2f,2.5f,1.6f,1.5f,3.2f,0.8f},duration={3,2,0.2f,4,2,0.4f,0.4f,4};
            int[] partners={7,5,1,4,0,3,2,6};
            var weapons=new WeaponData[8];
            for(int i=0;i<8;i++){int k=i;weapons[i]=Asset<WeaponData>("Weapon_"+i,w=>{w.id="w"+k;w.title=wn[k];w.evolvedTitle=en[k];w.description=wd[k];w.pattern=(WeaponPattern)k;w.evolutionPartner=passives[partners[k]];w.icon=Art("Weapon"+k);w.damage=dmg[k];w.cooldown=cd[k];w.area=area[k];w.duration=duration[k];w.penetration=k==1?1:k==4?8:0;w.color=Color.HSVToRGB(k/8f,0.55f,1);w.projectileSpeed=k==1?12:8;w.knockback=k==2?0.6f:0.15f;});}
            string[] cn={"Ari - Balanced","Vela - Swift","Bram - Power","Mira - Mage","Nox - Reaper"};
            string[] desc={"Flexible mixed-weapon explorer.","Fragile, fast-moving knife specialist.","Heavy close-range sweeper with high health.","Fragile mage with large persistent attacks.","High-risk fighter who grows stronger as health falls."};
            string[] passiveDesc={"Experience +10%","Dodge chance +8%","Knockback +40%, armor +2","Area +20%, projectiles +1","Up to +80% damage at low HP"};
            float[] hp={100,70,150,75,65},speed={5,6.4f,3.8f,4.7f,5.5f},power={10,9,15,10,12},interval={0.8f,0.65f,1.05f,0.85f,0.8f};int[] start={0,1,2,3,4};
            var characters=new CharacterData[5];
            for(int i=0;i<5;i++)
            {
                int k=i;characters[i]=Asset<CharacterData>("Character_"+i,ch=>{
                    ch.id="char"+k;ch.title=cn[k];ch.description=desc[k];ch.passiveDescription=passiveDesc[k];ch.maxHP=hp[k];ch.moveSpeed=speed[k];ch.damage=power[k];ch.attackInterval=interval[k];ch.startingWeapon=weapons[start[k]];
                    ch.tint=Color.Lerp(Color.white,Color.HSVToRGB(k/5f,0.4f,1),0.5f);
                    ch.inherentBonuses=k==0?new[]{new StatBonus(StatKind.Experience,0.1f)}:k==1?new[]{new StatBonus(StatKind.Dodge,0.08f)}:k==2?new[]{new StatBonus(StatKind.Knockback,0.4f),new StatBonus(StatKind.Armor,2)}:k==3?new[]{new StatBonus(StatKind.Area,0.2f),new StatBonus(StatKind.Projectiles,1)}:new[]{new StatBonus(StatKind.LowHPDamage,0.8f)};
                    ch.upgrades=k==0?new[]{Upgrade("power","Versatile power",StatKind.Damage,0.04f),Upgrade("health","Survival",StatKind.MaxHP,8),Upgrade("learn","Learning",StatKind.Experience,0.03f),Upgrade("pickup","Gathering",StatKind.Pickup,0.06f)}:
                    k==1?new[]{Upgrade("move","Quick step",StatKind.MoveSpeed,0.04f),Upgrade("haste","Fast hands",StatKind.Cooldown,0.025f),Upgrade("dodge","Evasion",StatKind.Dodge,0.01f),Upgrade("power","Knife training",StatKind.Damage,0.035f)}:
                    k==2?new[]{Upgrade("hp","Iron body",StatKind.MaxHP,12),Upgrade("power","Heavy impact",StatKind.Damage,0.05f),Upgrade("armor","Plating",StatKind.Armor,1),Upgrade("knock","Shove",StatKind.Knockback,0.1f)}:
                    k==3?new[]{Upgrade("area","Wide magic",StatKind.Area,0.05f),Upgrade("haste","Spell flow",StatKind.Cooldown,0.025f),Upgrade("count","Split magic",StatKind.Projectiles,1,200,2),Upgrade("duration","Lingering magic",StatKind.Duration,0.05f)}:
                    new[]{Upgrade("crit","Precision",StatKind.Critical,0.025f),Upgrade("haste","Reaping speed",StatKind.Cooldown,0.025f),Upgrade("risk","Blood power",StatKind.LowHPDamage,0.08f),Upgrade("leech","Soul recovery",StatKind.Leech,0.1f,100,5)};
                });
            }
            var stages=new StageData[3];string[] sn={"Stage 1 - Lumen Field","Stage 2 - Amber Basin","Stage 3 - Violet Ruins"};
            for(int i=0;i<3;i++){int k=i;stages[i]=Asset<StageData>("Stage_"+i,s=>{s.id="stage"+k;s.title=sn[k];s.description="Infinite survival. "+(k==0?"Balanced monster mix.":k==1?"Wide basin; more fast moths.":"Tall ruins; more armored beetles.");s.halfSize=k==0?new Vector2(32,20):k==1?new Vector2(40,24):new Vector2(25,34);s.layoutSeed=41+k*101;s.difficulty=1+k*0.45f;s.groundColor=k==0?Color.white:k==1?new Color(1,0.72f,0.42f):new Color(0.8f,0.6f,1);s.detailColor=k==0?Color.white:k==1?new Color(1,0.85f,0.55f):new Color(0.8f,0.65f,1);s.mothWeight=k==1?0.5f:0.3f;s.beetleWeight=k==2?0.45f:0.2f;s.mothUnlock=k==0?30:0;s.beetleUnlock=k==0?90:30;s.propsCount=28+k*8;s.firstEliteTime=90-k*15;});}
            var result=Asset<CampaignData>("Campaign",d=>{
                d.weapons=weapons;d.passives=passives;d.characters=characters;d.stages=stages;
                d.whiteSprite=Art("White");d.ringSprite=Art("AttackRing");d.projectileSprite=Art("Projectile");d.chestSprite=Art("TreasureChest");d.propSprite=Art("SupplyCrate");d.itemSprite=Art("FieldItem");d.supportSprite=Art("SupportDrone");
                d.commonUpgrades=new[]{Upgrade("damage","Attack",StatKind.Damage,0.04f),Upgrade("hp","Maximum HP",StatKind.MaxHP,10),Upgrade("move","Move speed",StatKind.MoveSpeed,0.025f),Upgrade("armor","Armor",StatKind.Armor,1),Upgrade("exp","Experience",StatKind.Experience,0.04f),Upgrade("gold","Gold gain",StatKind.Gold,0.05f),Upgrade("pickup","Pickup range",StatKind.Pickup,0.08f)};
            });
            // Add missing art fields on existing data without resetting player-tuned stats.
            for(int i=0;i<result.characters.Length;i++)
            {
                var ch=result.characters[i];if(ch==null)continue;
                if(ch.portrait==null)ch.portrait=Art("Portrait"+i);
                if(ch.sprite==null)ch.sprite=Art("Character"+i+"_Down_0");
                if(string.IsNullOrEmpty(ch.role))ch.role=new[]{"BALANCED","SWIFT","POWER","MAGE","REAPER"}[i];
                if(!ValidFrames(ch.downFrames))ch.downFrames=Frames(i,"Down");
                if(!ValidFrames(ch.upFrames))ch.upFrames=Frames(i,"Up");
                if(!ValidFrames(ch.leftFrames))ch.leftFrames=Frames(i,"Left");
                if(!ValidFrames(ch.rightFrames))ch.rightFrames=Frames(i,"Right");
                EditorUtility.SetDirty(ch);
            }
            for(int i=0;i<result.weapons.Length;i++){if(result.weapons[i].evolvedIcon==null)result.weapons[i].evolvedIcon=Art("EvolvedWeapon"+i);EditorUtility.SetDirty(result.weapons[i]);}
            foreach(var stage in result.stages)
            {
                stage.description=(stage.description??"").Replace("20 minutes + final boss.","Infinite survival.");
                EditorUtility.SetDirty(stage);
            }
            KoreanText.LocalizeData(result);
            foreach(var w in result.weapons)EditorUtility.SetDirty(w);
            foreach(var p in result.passives)EditorUtility.SetDirty(p);
            foreach(var ch in result.characters)EditorUtility.SetDirty(ch);
            foreach(var stage in result.stages)EditorUtility.SetDirty(stage);
            EditorUtility.SetDirty(result);
            AssetDatabase.SaveAssets();return result;
        }
        static Sprite[] Frames(int character,string direction)
        {var frames=new Sprite[4];for(int i=0;i<4;i++)frames[i]=Art("Character"+character+"_"+direction+"_"+i);return frames;}
        public static void Apply(GameManager game)
        {
            CampaignData data=EnsureData();ValidateData(data);
            var campaign=game.GetComponent<CampaignController>();if(campaign==null)campaign=game.gameObject.AddComponent<CampaignController>();
            var actions=AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(SurvivorSetup.Root+"/Input/Survivor.inputactions");
            if(actions==null)throw new InvalidOperationException("Input actions missing. Wait for import, then retry.");
            var move=actions.FindAction("Gameplay/Move",true);
            if(!System.Linq.Enumerable.Any(move.bindings,b=>b.path=="<Keyboard>/upArrow"))throw new InvalidOperationException("Copy the updated Input/Survivor.inputactions file.");
            var reference=game.Player.MoveAction;
            if(reference==null){reference=UnityEngine.InputSystem.InputActionReference.Create(move);AssetDatabase.CreateAsset(reference,SurvivorSetup.Root+"/Input/MoveReference.asset");game.Player.SetInput(reference);}
            else reference.Set(move);
            EditorUtility.SetDirty(reference);EditorUtility.SetDirty(game.Player);
            KoreanSetup.ApplyScene(game);
            campaign.Configure(data);EditorUtility.SetDirty(campaign);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        }
        [MenuItem("Tools/Pixel Survivor/Apply Infinite Survival Update",false,12)]
        public static void ApplyMenu()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogWarning("Stop Play before updating.");return;}
            var game=UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if(game==null || !game.HasRequiredReferences())throw new InvalidOperationException("Open your working MainScene first, or run Setup for a new project.");
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(!game.Spawner.HasExpandedRoster){EnsureData();SurvivorSetup.ApplyContentUpdate();}
            Apply(game);SurvivorSetup.ValidateScene();EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("한국어 무한 생존 업데이트 적용 완료. Play > 시작 > 캐릭터 선택 > 스테이지 선택 순서로 진행하세요.");
        }
        static bool ValidFrames(Sprite[] frames){return frames!=null && frames.Length==4 && Array.TrueForAll(frames,x=>x!=null);}
        public static void ValidateData(CampaignData data)
        {
            if(data==null || !data.IsConfigured)throw new InvalidOperationException("Incomplete Campaign data.");
            if(data.ringSprite==null || data.projectileSprite==null || data.propSprite==null || data.chestSprite==null || data.itemSprite==null || data.supportSprite==null)throw new InvalidOperationException("Missing campaign art. Copy all new PNGs.");
            var ids=new System.Collections.Generic.HashSet<string>();
            foreach(var w in data.weapons)if(!ids.Add(w.id))throw new InvalidOperationException("Duplicate weapon id.");
            ids.Clear();foreach(var p in data.passives)if(!ids.Add(p.id))throw new InvalidOperationException("Duplicate passive id.");
            foreach(var w in data.weapons)if(w==null || w.icon==null || w.evolvedIcon==null || w.evolutionPartner==null || Array.IndexOf(data.passives,w.evolutionPartner)<0 || w.maxLevel<1)throw new InvalidOperationException("Invalid weapon or evolution partner.");
            foreach(var p in data.passives)if(p==null || p.icon==null || p.maxLevel<1)throw new InvalidOperationException("Invalid passive.");
            foreach(var ch in data.characters)if(ch==null || ch.sprite==null || ch.portrait==null || !ValidFrames(ch.downFrames) || !ValidFrames(ch.upFrames) || !ValidFrames(ch.leftFrames) || !ValidFrames(ch.rightFrames) || ch.startingWeapon==null || Array.IndexOf(data.weapons,ch.startingWeapon)<0)throw new InvalidOperationException("Missing character starter.");
            foreach(var s in data.stages)if(s==null || s.unlockBossTime<=0 || s.bossInterval<=0)throw new InvalidOperationException("Boss interval and unlock milestone must be positive.");
        }
    }
}
