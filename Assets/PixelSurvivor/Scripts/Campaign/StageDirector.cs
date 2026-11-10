using UnityEngine;
namespace PixelSurvivor
{
    public sealed class StageDirector
    {
        readonly CampaignController c;
        float spawnTimer, nextElite;
        int bossWaves;
        public int BossWaves => bossWaves;
        public Enemy CurrentBoss {get;private set;}
        public StageDirector(CampaignController owner){c=owner;}
        public void Begin()
        {
            var d=c.StageData;spawnTimer=0.5f;nextElite=d.firstEliteTime;
            bossWaves=0;CurrentBoss=null;
            c.Game.Bounds.SetSize(d.halfSize);
            c.Game.Spawner.SetMaxEnemyCount(220);
            Transform map=c.Game.Bounds.transform;
            var layout=new System.Random(d.layoutSeed);
            int detailIndex=0;
            foreach(var sr in map.GetComponentsInChildren<SpriteRenderer>())
            {
                if(sr.name=="Ground"){sr.size=d.halfSize*2;sr.color=d.groundColor;}
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
                    sr.color=d.detailColor;
                    detailIndex++;
                }
                else sr.color=d.detailColor;
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
                        AddFloorBand(root,tile,"Ruins column",new Vector2(side*4,i*6.5f),
                            new Vector2(2.2f,4.5f),new Color(0.42f,0.24f,0.63f,0.4f));
            }
        }
        static void AddFloorBand(Transform parent,Sprite sprite,string name,Vector2 position,Vector2 size,Color color)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=position;
            var sr=go.AddComponent<SpriteRenderer>();
            sr.sprite=sprite;
            sr.drawMode=SpriteDrawMode.Tiled;
            sr.size=size;
            sr.color=color;
            sr.sortingOrder=-15;
        }
        public float Elapsed => c.Game.Timer.Elapsed*c.TimelineScale;
        public void Tick(float dt)
        {
            c.Terrain.Tick(dt);
            var d=c.StageData;float time=Elapsed;
            // Catch up scheduled waves, keeping the original wave identity if a prior boss is alive.
            int due=SurvivalRules.DueBosses(time,d.bossInterval);
            if(bossWaves<due) {bossWaves++;SpawnBossWave(bossWaves);}
            if(time>=nextElite)
            { nextElite=time+SurvivalRules.EliteInterval(time,d.eliteInterval,d.minimumEliteInterval,d.eliteRateGrowthPerMinute);SpawnEnemy(true); }
            spawnTimer-=dt;
            if(spawnTimer<=0)
            {
                spawnTimer=Mathf.Max(d.minimumSpawnInterval,d.initialSpawnInterval/(1+time/180));
                int count=Mathf.Min(d.maxSpawnCount,d.initialSpawnCount+Mathf.FloorToInt(time/Mathf.Max(1,d.extraSpawnEvery)));
                for(int i=0;i<count;i++)SpawnEnemy(false);
            }
        }
        Enemy ChoosePrefab()
        {
            var d=c.StageData;var s=c.Game.Spawner;float sample=Random.value;
            float beetle=Elapsed>=d.beetleUnlock ? d.beetleWeight : 0;
            float moth=Elapsed>=d.mothUnlock ? d.mothWeight : 0;
            float bone=Elapsed>=d.boneCrawlerUnlock ? d.boneCrawlerWeight : 0;
            float ember=Elapsed>=d.emberWraithUnlock ? d.emberWraithWeight : 0;
            if(sample<beetle && s.BeetlePrefab!=null)return s.BeetlePrefab;
            if(sample<beetle+moth && s.MothPrefab!=null)return s.MothPrefab;
            if(sample<beetle+moth+bone && s.BoneCrawlerPrefab!=null)return s.BoneCrawlerPrefab;
            if(sample<beetle+moth+bone+ember && s.EmberWraithPrefab!=null)return s.EmberWraithPrefab;
            return s.EnemyPrefab;
        }
        public Enemy SpawnEnemy(bool elite)
        {
            var d=c.StageData;Vector2 position=c.Game.Spawner.ChooseSpawnPosition();Enemy prefab=ChoosePrefab();
            Enemy enemy=elite?c.Game.Spawner.SpawnSpecial(position,prefab):c.Game.Spawner.SpawnAt(position,prefab);
            if(enemy==null)return null;
            float minutes=Elapsed/60;
            enemy.ConfigureEncounter((1+minutes*d.healthGrowthPerMinute)*d.difficulty*(elite?d.eliteHP:1),
                (1+minutes*d.speedGrowthPerMinute)*(elite?d.eliteSpeed:1),
                (1+minutes*d.damageGrowthPerMinute)*d.difficulty*(elite?d.eliteDamage:1),elite,false,false);
            return enemy;
        }
        // Kept for QA callers; regular events use an explicit wave number.
        public Enemy SpawnBoss(bool unlock) => SpawnBossWave(unlock?Mathf.CeilToInt(c.StageData.unlockBossTime/c.StageData.bossInterval):1);
        public Enemy SpawnBossWave(int wave)
        {
            var d=c.StageData;wave=Mathf.Max(1,wave);
            bool unlock=SurvivalRules.IsUnlockWave(wave,d.bossInterval,d.unlockBossTime);
            Enemy prefab=c.StageIndex==1?c.Game.Spawner.MothPrefab:c.StageIndex==2?c.Game.Spawner.EnemyPrefab:c.Game.Spawner.BeetlePrefab;
            Enemy enemy=c.Game.Spawner.SpawnSpecial(c.Game.Spawner.ChooseSpawnPosition(),prefab??c.Game.Spawner.EnemyPrefab,true);
            if(enemy==null)return null;
            int unlockWave=Mathf.Max(1,Mathf.CeilToInt(d.unlockBossTime/d.bossInterval));
            float hp=wave>=unlockWave?Mathf.Max(d.bossHP,d.unlockBossHP)*SurvivalRules.Growth(wave-unlockWave+1,d.bossHealthGrowthPerWave):d.bossHP*SurvivalRules.Growth(wave,d.bossHealthGrowthPerWave);
            enemy.ConfigureEncounter(hp*d.difficulty/enemy.MaxHP,0.9f,2.2f*d.difficulty*SurvivalRules.Growth(wave,d.bossDamageGrowthPerWave),false,true,unlock);
            enemy.gameObject.AddComponent<BossBrain>().Initialize(c,enemy,wave);
            CurrentBoss=enemy;c.UI.Notify("보스 등장 · "+wave+(unlock?"회차 · 처치 시 다음 구역 해금":"회차"));return enemy;
        }
        public void EnemyDied(Enemy enemy,Vector2 position)
        {
            c.RegisterSpecialKill(enemy);
            if(enemy.IsBoss || enemy.IsElite)c.Field.DropChest(position,enemy.IsBoss,enemy.IsFinalBoss);
            else if(Random.value<c.Data.monsterGoldChance)c.Field.Drop(position,FieldItemKind.Gold,c.Data.monsterGold);
        }
    }
}
