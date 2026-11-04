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
            Transform map=c.Game.Bounds.transform;
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
                else sr.color=d.detailColor;
            }
            c.Game.View.GetComponent<CameraFollow>().Snap();
        }
        public float Elapsed => c.Game.Timer.Elapsed*c.TimelineScale;
        public void Tick(float dt)
        {
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
            if(sample<beetle && s.BeetlePrefab!=null)return s.BeetlePrefab;
            if(sample<beetle+moth && s.MothPrefab!=null)return s.MothPrefab;
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
            Enemy enemy=c.Game.Spawner.SpawnSpecial(c.Game.Spawner.ChooseSpawnPosition(),prefab??c.Game.Spawner.EnemyPrefab);
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
