using UnityEngine;
namespace PixelSurvivor
{
    public sealed class StageDirector
    {
        const float MeadowFoxUnlockSeconds=300f;
        const float AmberKestrelUnlockSeconds=240f;
        const float ArmoredMonsterUnlockSeconds=180f;
        readonly CampaignController c;
        float spawnTimer, nextElite;
        int bossWaves;
        int reportedMinute=-1;
        bool doomAppeared;
        bool doomCaughtPlayer;
        Sprite originalGround;
        public int BossWaves => bossWaves;
        public Enemy CurrentBoss {get;private set;}
        public DoomPursuer Doom {get;private set;}
        public bool DoomAppeared => doomAppeared;
        public bool DoomCaughtPlayer => doomCaughtPlayer;
        // More enemies enter over the run without filling the absolute 220 cap in the opening minutes.
        public int DensityCap => Mathf.Min(c.Game.Spawner.MaxEnemyCount,
            70+c.StageIndex*10+Mathf.FloorToInt(Elapsed/60f)*7);
        public StageDirector(CampaignController owner){c=owner;}
        public void Begin()
        {
            var d=c.StageData;spawnTimer=0.5f;nextElite=d.firstEliteTime;
            reportedMinute=-1;bossWaves=0;CurrentBoss=null;Doom=null;doomAppeared=false;doomCaughtPlayer=false;
            c.Game.Bounds.SetSize(d.halfSize);
            c.Game.Spawner.SetMaxEnemyCount(220);
            Transform map=c.Game.Bounds.transform;
            var layout=new System.Random(d.layoutSeed);
            int detailIndex=0;
            foreach(var sr in map.GetComponentsInChildren<SpriteRenderer>())
            {
                if(sr.name=="Ground")
                {
                    if(originalGround==null)originalGround=sr.sprite;
                    var meadow=Resources.Load<Sprite>("StageCreatures/LumenGround");
                    sr.sprite=c.StageIndex==0 && meadow!=null ? meadow : originalGround;
                    sr.size=d.halfSize*2;
                    // A cool moss tint keeps the meadow green while separating bright enemies from the floor.
                    sr.color=c.StageIndex==0 ? d.groundColor*new Color(.52f,.63f,.48f,1) : d.groundColor;
                }
                else if(sr.name.Contains("boundary"))
                {
                    bool horizontal=sr.name.StartsWith("North")||sr.name.StartsWith("South");
                    float sign=sr.name.StartsWith("North")||sr.name.StartsWith("East") ? 1 : -1;
                    sr.transform.localPosition=horizontal ? new Vector3(0,sign*d.halfSize.y,0) : new Vector3(sign*d.halfSize.x,0,0);
                    sr.transform.localScale=horizontal ? new Vector3((d.halfSize.x*2+0.5f)*32,16,1) : new Vector3(16,d.halfSize.y*64,1);
                }
                else if(sr.name.StartsWith("Floor detail "))
                {
                    float x,y;
                    if(c.StageIndex==1)
                    {
                        float angle=detailIndex*2.39996323f;
                        float ring=0.18f+0.65f*(detailIndex%19)/18f;
                        x=Mathf.Cos(angle)*d.halfSize.x*ring;
                        y=Mathf.Sin(angle)*d.halfSize.y*ring;
                    }
                    else if(c.StageIndex==2)
                    {
                        x=(detailIndex%2==0?-1:1)*d.halfSize.x*0.28f+((float)layout.NextDouble()-0.5f)*8;
                        y=((float)layout.NextDouble()*2-1)*d.halfSize.y*0.82f;
                    }
                    else
                    {
                        x=((float)layout.NextDouble()*2-1)*Mathf.Min(d.halfSize.x*0.85f,30);
                        y=((float)layout.NextDouble()*2-1)*Mathf.Min(d.halfSize.y*0.85f,18);
                    }
                    sr.transform.localPosition=new Vector3(x,y,sr.transform.localPosition.z);
                    sr.color=c.StageIndex==0 ? d.detailColor*new Color(.58f,.68f,.5f,1) : d.detailColor;
                    detailIndex++;
                }
                else sr.color=c.StageIndex==0 ? d.detailColor*new Color(.58f,.68f,.5f,1) : d.detailColor;
            }
            ApplyFloorLayout(map,d);
            c.Terrain.Build(map);
            c.Game.View.GetComponent<CameraFollow>().Snap();
        }
        void ApplyFloorLayout(Transform map,StageData d)
        {
            var previous=map.Find("Stage floor layout");
            if(previous!=null){previous.gameObject.SetActive(false);Object.Destroy(previous.gameObject);}
            if(c.StageIndex==0)return;
            var ground=map.Find("Ground")?.GetComponent<SpriteRenderer>();
            if(ground==null || ground.sprite==null)return;
            var tile=c.Data.whiteSprite!=null?c.Data.whiteSprite:ground.sprite;
            var root=new GameObject("Stage floor layout").transform;
            root.SetParent(map,false);
            if(c.StageIndex==1)
            {
                for(int i=-3;i<=3;i++)
                    AddFloorBand(root,tile,"Basin band",new Vector2(Mathf.Sin(i*1.7f)*2,i*5.5f),
                        new Vector2(d.halfSize.x*1.45f,2.5f),new Color(0.45f,0.25f,0.07f,0.4f));
            }
            else if(c.StageIndex==2)
            {
                for(int side=-1;side<=1;side+=2)
                    for(int i=-2;i<=2;i++)
                        AddFloorBand(root,tile,"Ruins paving",new Vector2(side*4,i*6.5f),
                            new Vector2(1.1f,3.2f),new Color(0.12f,0.19f,0.28f,0.35f));
            }
        }
        static void AddFloorBand(Transform parent,Sprite sprite,string name,Vector2 position,Vector2 size,Color color)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=position;
            var sr=go.AddComponent<SpriteRenderer>();
            sr.sprite=sprite;
            sr.drawMode=SpriteDrawMode.Simple;
            if(sprite.bounds.size.x>0 && sprite.bounds.size.y>0)
                go.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
            sr.color=color;
            sr.sortingOrder=-15;
        }
        public float Elapsed => c.Game.Timer.Elapsed;
        public void Tick(float dt)
        {
            c.Terrain.Tick(dt);
            var d=c.StageData;float time=Elapsed;
            if(!doomAppeared && time>=d.unlockBossTime)
            {
                doomAppeared=true;
                bool unlocked=c.UnlockForSurvivingMilestone();
                c.Game.Spawner.DespawnAll();
                var go=new GameObject("Doom Pursuer");
                go.transform.SetParent(c.Game.RuntimeRoot,false);
                go.transform.position=c.Game.Spawner.ChooseSpawnPosition();
                Doom=go.AddComponent<DoomPursuer>();Doom.Initialize(c);
                CurrentBoss=null;
                c.UI.ShowDoomWarning(unlocked);
                return;
            }
            if(doomAppeared)return;
            int minute=Mathf.FloorToInt(time/60);
            if(minute!=reportedMinute)
            {
                reportedMinute=minute;
                Debug.Log("[Pixel Survivor] Stage "+(c.StageIndex+1)+" minute "+minute+
                    " spawn batch="+SurvivalRules.MinuteSpawnCount(time,d.initialSpawnCount,d.spawnCountPerMinute,d.maximumMinuteSpawnCount)+" density cap="+DensityCap);
            }
            // Catch up scheduled waves, keeping the original wave identity if a prior boss is alive.
            int due=SurvivalRules.DueBosses(time,d.bossInterval);
            if(bossWaves<due) {bossWaves++;SpawnBossWave(bossWaves);}
            if(time>=nextElite)
            { nextElite=time+SurvivalRules.EliteInterval(time,d.eliteInterval,d.minimumEliteInterval,d.eliteRateGrowthPerMinute);SpawnEnemy(true); }
            spawnTimer-=dt;
            if(spawnTimer<=0)
            {
                spawnTimer=Mathf.Max(d.minimumSpawnInterval,d.initialSpawnInterval/(1+time/180));
                int count=SurvivalRules.MinuteSpawnCount(time,d.initialSpawnCount,d.spawnCountPerMinute,d.maximumMinuteSpawnCount);
                for(int i=0;i<count && c.Game.Spawner.ActiveEnemies.Count<DensityCap;i++)SpawnEnemy(false);
            }
        }
        Enemy ChoosePrefab()
        {
            var s=c.Game.Spawner;float sample=Random.value;
            if(c.StageIndex==0)
            {
                if(Elapsed>=MeadowFoxUnlockSeconds && sample<0.2f && s.MeadowFoxPrefab!=null)return s.MeadowFoxPrefab;
                if(sample<(Elapsed>=MeadowFoxUnlockSeconds?0.55f:0.45f) && s.WindLeafSpiritPrefab!=null)return s.WindLeafSpiritPrefab;
                return s.EnemyPrefab;
            }
            if(c.StageIndex==1)
            {
                if(Elapsed>=AmberKestrelUnlockSeconds && sample<0.18f && s.AmberKestrelPrefab!=null)return s.AmberKestrelPrefab;
                if(Elapsed>=ArmoredMonsterUnlockSeconds && sample<0.38f && s.BeetlePrefab!=null)return s.BeetlePrefab;
                if(sample<(Elapsed>=ArmoredMonsterUnlockSeconds?0.7f:0.5f) && s.AmberFireflyPrefab!=null)return s.AmberFireflyPrefab;
                return s.MothPrefab??s.AmberFireflyPrefab;
            }
            if(Elapsed>=ArmoredMonsterUnlockSeconds && sample<0.24f && s.BoneCrawlerPrefab!=null)return s.BoneCrawlerPrefab;
            if(Elapsed>=90f && sample<0.42f && s.EmberWraithPrefab!=null)return s.EmberWraithPrefab;
            if(sample<(Elapsed>=90f?0.7f:0.5f) && s.RuinZombiePrefab!=null)return s.RuinZombiePrefab;
            return s.RuinBatPrefab??s.RuinZombiePrefab;
        }
        public Enemy SpawnEnemy(bool elite)
        {
            var d=c.StageData;Vector2 position=c.Game.Spawner.ChooseSpawnPosition();
            int reinforcement=StageReinforcements.Select(c.StageIndex,Elapsed,Random.value);
            Enemy prefab=reinforcement>=0?c.Game.Spawner.EnemyPrefab:elite?ChooseElitePrefab():ChoosePrefab();
            Enemy enemy=elite?c.Game.Spawner.SpawnSpecial(position,prefab):c.Game.Spawner.SpawnAt(position,prefab);
            if(enemy==null)return null;
            if(reinforcement>=0){StageReinforcements.Apply(enemy,reinforcement);enemy.SetEliteAppearance(null);}
            float minutes=Elapsed/60;
            enemy.ConfigureEncounter((1+minutes*d.healthGrowthPerMinute)*d.difficulty*(elite?d.eliteHP:1),
                (1+minutes*d.speedGrowthPerMinute)*(elite?d.eliteSpeed:1),
                (1+minutes*d.damageGrowthPerMinute)*d.difficulty*(elite?d.eliteDamage:1),elite,false,false);
            return enemy;
        }
        Enemy ChooseElitePrefab()
        {
            var s=c.Game.Spawner;
            return c.StageIndex==0 ? s.EnemyPrefab :
                c.StageIndex==1 ? (Elapsed>=ArmoredMonsterUnlockSeconds ? s.BeetlePrefab??s.AmberFireflyPrefab : s.AmberFireflyPrefab??s.MothPrefab) :
                (Elapsed>=ArmoredMonsterUnlockSeconds ? s.BoneCrawlerPrefab??s.RuinZombiePrefab : s.RuinZombiePrefab??s.RuinBatPrefab);
        }
        // Kept for QA callers; regular events use an explicit wave number.
        public Enemy SpawnBoss(bool unlock) => SpawnBossWave(unlock?Mathf.CeilToInt(c.StageData.unlockBossTime/c.StageData.bossInterval):1);
        public Enemy SpawnBossWave(int wave)
        {
            var d=c.StageData;wave=Mathf.Max(1,wave);
            bool unlock=SurvivalRules.IsUnlockWave(wave,d.bossInterval,d.unlockBossTime);
            Enemy prefab=c.StageIndex==0 ? c.Game.Spawner.LumenStagBossPrefab :
                c.StageIndex==1 ? c.Game.Spawner.AmberCicadaBossPrefab : c.Game.Spawner.VioletLichBossPrefab;
            Enemy enemy=c.Game.Spawner.SpawnSpecial(c.Game.Spawner.ChooseSpawnPosition(),prefab??c.Game.Spawner.EnemyPrefab,true);
            if(enemy==null)return null;
            int unlockWave=Mathf.Max(1,Mathf.CeilToInt(d.unlockBossTime/d.bossInterval));
            float hp=wave>=unlockWave?Mathf.Max(d.bossHP,d.unlockBossHP)*SurvivalRules.Growth(wave-unlockWave+1,d.bossHealthGrowthPerWave):d.bossHP*SurvivalRules.Growth(wave,d.bossHealthGrowthPerWave);
            enemy.ConfigureEncounter(hp*d.difficulty/enemy.MaxHP,0.9f,2.2f*d.difficulty*SurvivalRules.Growth(wave,d.bossDamageGrowthPerWave),false,true,unlock);
            enemy.gameObject.AddComponent<BossBrain>().Initialize(c,enemy,wave);
            CurrentBoss=enemy;c.UI.Notify("보스 등장 · "+wave+"회차");return enemy;
        }
        public void CaughtByDoom()
        {
            if(!doomAppeared || !c.Game.IsPlaying)return;
            doomCaughtPlayer=true;
            c.Game.EndRun(false);
        }
        public void EnemyDied(Enemy enemy,Vector2 position)
        {
            c.RegisterSpecialKill(enemy);
            if(enemy.IsBoss || enemy.IsElite)c.Field.DropChest(position,enemy.IsBoss,enemy.IsFinalBoss);
            else if(Random.value<c.Data.monsterGoldChance)c.Field.Drop(position,FieldItemKind.Gold,c.Data.monsterGold);
        }
    }
}
