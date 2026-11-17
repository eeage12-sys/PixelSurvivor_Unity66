using UnityEngine;
namespace PixelSurvivor
{
    public sealed class StageDirector
    {
        const float MeadowFoxUnlockSeconds=300f;
        const float AmberBeetleUnlockSeconds=240f;
        const float AmberKestrelUnlockSeconds=360f;
        const float AmberScorpionUnlockSeconds=480f;
        const float AmberScarabUnlockSeconds=660f;
        const float RuinWraithUnlockSeconds=240f;
        const float RuinBoneUnlockSeconds=300f;
        const float RuinKnightUnlockSeconds=480f;
        const float RuinSpecterUnlockSeconds=600f;
        const float LateMonsterUnlockSeconds=360f;
        const float HighTierMonsterUnlockSeconds=600f;
        const float WindSpiritUnlockSeconds=60f;
        readonly CampaignController c;
        float spawnTimer;
        int bossWaves;
        int scriptedEventMask,firstRushGroups,secondRushGroups,thirdRushGroups,lateRushGroups,finalRushGroups;
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
            var d=c.StageData;spawnTimer=0.5f;
            bossWaves=0;CurrentBoss=null;Doom=null;doomAppeared=false;doomCaughtPlayer=false;
            scriptedEventMask=0;firstRushGroups=0;secondRushGroups=0;thirdRushGroups=0;lateRushGroups=0;finalRushGroups=0;
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
                    sr.color=d.groundColor;
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
            if(c.StageIndex==0)TickMeadowEvents(time);
            else if(c.StageIndex==1)TickAmberEvents(time);
            else TickRuinEvents(time);
            spawnTimer-=dt;
            if(spawnTimer<=0)
            {
                spawnTimer=Mathf.Max(d.minimumSpawnInterval,d.initialSpawnInterval/(1+time/180));
                int count=Mathf.Min(d.maxSpawnCount,d.initialSpawnCount+Mathf.FloorToInt(time/Mathf.Max(1,d.extraSpawnEvery)));
                for(int i=0;i<count && c.Game.Spawner.ActiveEnemies.Count<DensityCap;i++)SpawnEnemy(false);
            }
        }
        void TickMeadowEvents(float time)
        {
            var s=c.Game.Spawner;
            ScriptedElite(time,120f,1,s.EnemyPrefab,"빛 정령 정예");
            ScriptedRush(time,180f,ref firstRushGroups,s.EnemyPrefab,10,1.65f,"빛 정령");
            ScriptedElite(time,240f,2,s.WindLeafSpiritPrefab,"바람 정령 정예");
            ScriptedRush(time,420f,ref secondRushGroups,s.WindLeafSpiritPrefab,9,1.7f,"바람 정령");
            ScriptedRush(time,480f,ref thirdRushGroups,s.MeadowFoxPrefab,7,1.45f,"초원 여우");
            ScriptedElite(time,480f,4,s.MeadowFoxPrefab,"초원 여우 정예");
            ScriptedElite(time,540f,8,s.BrambleBoarPrefab,"가시덤불 멧돼지 정예");
            ScriptedBoss(time,900f,16,s.GuardianGolemPrefab??s.MossGolemPrefab,"수호 골렘");
            ScriptedBoss(time,1080f,32,s.LumenStagBossPrefab,"루멘 사슴");
        }
        void TickAmberEvents(float time)
        {
            var s=c.Game.Spawner;
            HorizontalSweep(time,120f,ref firstRushGroups,s.AmberFireflyPrefab,8,1.65f,"반딧불이 횡단",4);
            ScriptedElite(time,180f,1,s.AmberFireflyPrefab,"호박 반딧불이 정예");
            HorizontalSweep(time,300f,ref secondRushGroups,s.BeetlePrefab,9,1.4f,"딱정벌레 장벽",4);
            ScriptedElite(time,420f,2,s.AmberKestrelPrefab,"호박 황조롱이 정예");
            HorizontalSweep(time,540f,ref thirdRushGroups,s.AmberScorpionPrefab,7,1.55f,"호박 전갈 협공",4);
            ScriptedElite(time,540f,4,s.AmberScorpionPrefab,"호박 전갈 정예");
            ScriptedBoss(time,600f,8,s.AmberSentinelPrefab??s.CinderScarabPrefab,"호박 갑충 수호자");
            HorizontalSweep(time,780f,ref lateRushGroups,s.CinderScarabPrefab,6,1.4f,"잿불 풍뎅이 포화",3);
            ScriptedElite(time,840f,16,s.CinderScarabPrefab,"잿불 풍뎅이 정예");
            HorizontalSweep(time,960f,ref finalRushGroups,s.AmberFireflyPrefab,12,1.8f,"호박빛 대습격",5,s.MothPrefab);
            ScriptedBoss(time,1080f,32,s.AmberCicadaBossPrefab,"호박 매미");
        }
        void TickRuinEvents(float time)
        {
            var s=c.Game.Spawner;
            RingSiege(time,120f,ref firstRushGroups,s.RuinZombiePrefab,10,1.45f,"좀비 포위",3);
            ScriptedElite(time,180f,1,s.RuinZombiePrefab,"폐허 좀비 정예");
            ScriptedElite(time,360f,2,s.BoneCrawlerPrefab,"뼈 크롤러 정예");
            RingSiege(time,420f,ref secondRushGroups,s.RuinBatPrefab,10,1.7f,"박쥐 포위",3);
            ScriptedElite(time,540f,4,s.CryptKnightPrefab,"묘지 기사 정예");
            RingSiege(time,660f,ref thirdRushGroups,s.VoidSpecterPrefab,6,1.45f,"망령 포위",4);
            ScriptedBoss(time,720f,8,s.RuinWardenPrefab??s.CryptKnightPrefab,"폐허 수문장");
            RingSiege(time,840f,ref lateRushGroups,s.BoneCrawlerPrefab,9,1.4f,"폐허 공성",4,s.CryptKnightPrefab);
            ScriptedElite(time,960f,16,s.VoidSpecterPrefab,"공허 망령 정예");
            ScriptedBoss(time,1080f,32,s.VioletLichBossPrefab,"보랏빛 리치");
            RingSiege(time,1140f,ref finalRushGroups,s.VoidSpecterPrefab,10,1.55f,"최후의 포위",3,s.CryptKnightPrefab);
        }
        void ScriptedBoss(float time,float due,int bit,Enemy prefab,string label)
        {
            if(time<due || (scriptedEventMask&bit)!=0)return;
            scriptedEventMask|=bit;bossWaves++;
            SpawnBossWave(bossWaves,prefab,label);
        }
        void ScriptedElite(float time,float due,int bit,Enemy prefab,string label)
        {
            if(time<due || (scriptedEventMask&bit)!=0)return;
            scriptedEventMask|=bit;
            var enemy=SpawnEnemy(true,prefab??c.Game.Spawner.EnemyPrefab);
            if(enemy!=null){c.UI.Notify(label+" 등장 · 보물상자 보장");Debug.Log("Stage elite: "+label+" at "+time.ToString("F1")+"s");}
        }
        void ScriptedRush(float time,float start,ref int groups,Enemy prefab,int size,float speed,string label,int totalGroups=4)
        {
            if(prefab==null || groups>=totalGroups || time<start+groups*2.5f)return;
            int group=++groups;
            Vector2 origin=c.Game.Spawner.ChooseSpawnPosition();
            Vector2 towards=((Vector2)c.Game.Player.transform.position-origin).normalized;
            Vector2 sideways=new Vector2(-towards.y,towards.x);
            int spawned=0;
            for(int i=0;i<size;i++)
            {
                Vector2 position=origin+sideways*((i-(size-1)*0.5f)*0.55f)+towards*Random.Range(-0.4f,0.4f);
                if(!SpawnRushEnemy(prefab,position,towards,speed))break;
                spawned++;
            }
            if(group==1)c.UI.Notify(label+" 무리 습격!");
            Debug.Log("Stage rush: "+label+" group "+group+"/"+totalGroups+", spawned="+spawned+" at "+time.ToString("F1")+"s");
        }
        void HorizontalSweep(float time,float start,ref int groups,Enemy prefab,int size,float speed,string label,int totalGroups,Enemy alternate=null)
        {
            if(prefab==null || groups>=totalGroups || time<start+groups*2.5f)return;
            int group=++groups;
            var view=c.Game.View;
            float halfWidth=view.orthographicSize*view.aspect+2.5f;
            bool fromLeft=group%2!=0;
            Vector2 direction=fromLeft?Vector2.right:Vector2.left;
            Vector2 center=view.transform.position;
            int spawned=0;
            for(int i=0;i<size;i++)
            {
                var chosen=alternate!=null && i%2!=0?alternate:prefab;
                Vector2 position=center+new Vector2(fromLeft?-halfWidth:halfWidth,(i-(size-1)*.5f)*.72f);
                if(!SpawnRushEnemy(chosen,position,direction,speed))break;
                spawned++;
            }
            if(group==1)c.UI.Notify(label+" · 좌우에서 밀려옵니다!");
            Debug.Log("Horizontal sweep: "+label+" group "+group+"/"+totalGroups+", spawned="+spawned+" at "+time.ToString("F1")+"s");
        }
        void RingSiege(float time,float start,ref int groups,Enemy prefab,int size,float speed,string label,int totalGroups,Enemy alternate=null)
        {
            if(prefab==null || groups>=totalGroups || time<start+groups*2.5f)return;
            int group=++groups;
            var view=c.Game.View;
            float radius=Mathf.Max(view.orthographicSize*view.aspect,view.orthographicSize)+3f;
            Vector2 center=c.Game.Player.transform.position;
            int spawned=0;
            for(int i=0;i<size;i++)
            {
                float angle=(i+(group-1)*.35f)*Mathf.PI*2/size;
                Vector2 outward=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                var chosen=alternate!=null && i%2!=0?alternate:prefab;
                if(!SpawnRushEnemy(chosen,center+outward*radius,-outward,speed))break;
                spawned++;
            }
            if(group==1)c.UI.Notify(label+" · 사방에서 포위합니다!");
            Debug.Log("Ring siege: "+label+" group "+group+"/"+totalGroups+", spawned="+spawned+" at "+time.ToString("F1")+"s");
        }
        bool SpawnRushEnemy(Enemy prefab,Vector2 position,Vector2 direction,float speed)
        {
            var enemy=c.Game.Spawner.SpawnSpecial(position,prefab);
            if(enemy==null)return false;
            float minutes=Elapsed/60f;var d=c.StageData;
            enemy.ConfigureEncounter((1+minutes*d.healthGrowthPerMinute)*d.difficulty,
                (1+minutes*d.speedGrowthPerMinute)*speed,
                (1+minutes*d.damageGrowthPerMinute)*d.difficulty,false,false,false);
            enemy.SetRush(direction,7f);
            return true;
        }
        Enemy ChoosePrefab()
        {
            var s=c.Game.Spawner;float sample=Random.value;
            if(c.StageIndex==0)
            {
                if(Elapsed<WindSpiritUnlockSeconds)return s.EnemyPrefab;
                if(Elapsed<MeadowFoxUnlockSeconds)
                    return sample<0.30f && s.WindLeafSpiritPrefab!=null?s.WindLeafSpiritPrefab:s.EnemyPrefab;
                if(Elapsed>=LateMonsterUnlockSeconds)
                {
                    if(Elapsed>=HighTierMonsterUnlockSeconds && sample<0.14f && s.MossGolemPrefab!=null)return s.MossGolemPrefab;
                    if(sample<0.32f && s.BrambleBoarPrefab!=null)return s.BrambleBoarPrefab;
                    if(sample<0.50f && s.MeadowFoxPrefab!=null)return s.MeadowFoxPrefab;
                    if(sample<0.75f && s.WindLeafSpiritPrefab!=null)return s.WindLeafSpiritPrefab;
                    return s.EnemyPrefab;
                }
                if(Elapsed>=MeadowFoxUnlockSeconds && sample<0.2f && s.MeadowFoxPrefab!=null)return s.MeadowFoxPrefab;
                if(sample<0.55f && s.WindLeafSpiritPrefab!=null)return s.WindLeafSpiritPrefab;
                return s.EnemyPrefab;
            }
            if(c.StageIndex==1)
            {
                if(Elapsed<60f)return s.AmberFireflyPrefab??s.MothPrefab;
                if(Elapsed>=AmberScorpionUnlockSeconds)
                {
                    if(Elapsed>=AmberScarabUnlockSeconds && sample<0.14f && s.CinderScarabPrefab!=null)return s.CinderScarabPrefab;
                    if(sample<0.32f && s.AmberScorpionPrefab!=null)return s.AmberScorpionPrefab;
                    if(sample<0.48f && s.AmberKestrelPrefab!=null)return s.AmberKestrelPrefab;
                    if(sample<0.64f && s.BeetlePrefab!=null)return s.BeetlePrefab;
                    if(sample<0.82f && s.AmberFireflyPrefab!=null)return s.AmberFireflyPrefab;
                    return s.MothPrefab??s.AmberFireflyPrefab;
                }
                if(Elapsed>=AmberKestrelUnlockSeconds && sample<0.18f && s.AmberKestrelPrefab!=null)return s.AmberKestrelPrefab;
                if(Elapsed>=AmberBeetleUnlockSeconds && sample<0.38f && s.BeetlePrefab!=null)return s.BeetlePrefab;
                if(sample<(Elapsed>=AmberBeetleUnlockSeconds?0.7f:0.5f) && s.AmberFireflyPrefab!=null)return s.AmberFireflyPrefab;
                return s.MothPrefab??s.AmberFireflyPrefab;
            }
            if(Elapsed<60f)return s.RuinZombiePrefab??s.RuinBatPrefab;
            if(Elapsed>=RuinKnightUnlockSeconds)
            {
                if(Elapsed>=RuinSpecterUnlockSeconds && sample<0.14f && s.VoidSpecterPrefab!=null)return s.VoidSpecterPrefab;
                if(sample<0.32f && s.CryptKnightPrefab!=null)return s.CryptKnightPrefab;
                if(sample<0.50f && s.BoneCrawlerPrefab!=null)return s.BoneCrawlerPrefab;
                if(sample<0.64f && s.EmberWraithPrefab!=null)return s.EmberWraithPrefab;
                if(sample<0.82f && s.RuinZombiePrefab!=null)return s.RuinZombiePrefab;
                return s.RuinBatPrefab??s.RuinZombiePrefab;
            }
            if(Elapsed>=RuinBoneUnlockSeconds && sample<0.24f && s.BoneCrawlerPrefab!=null)return s.BoneCrawlerPrefab;
            if(Elapsed>=RuinWraithUnlockSeconds && sample<0.42f && s.EmberWraithPrefab!=null)return s.EmberWraithPrefab;
            if(sample<(Elapsed>=RuinWraithUnlockSeconds?0.7f:0.5f) && s.RuinZombiePrefab!=null)return s.RuinZombiePrefab;
            return s.RuinBatPrefab??s.RuinZombiePrefab;
        }
        public Enemy SpawnEnemy(bool elite)
        {
            return SpawnEnemy(elite,elite?ChooseElitePrefab():ChoosePrefab());
        }
        Enemy SpawnEnemy(bool elite,Enemy prefab)
        {
            var d=c.StageData;Vector2 position=c.Game.Spawner.ChooseSpawnPosition();
            Enemy enemy=elite?c.Game.Spawner.SpawnSpecial(position,prefab):c.Game.Spawner.SpawnAt(position,prefab);
            if(enemy==null)return null;
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
                c.StageIndex==1 ? (Elapsed>=AmberBeetleUnlockSeconds ? s.BeetlePrefab??s.AmberFireflyPrefab : s.AmberFireflyPrefab??s.MothPrefab) :
                (Elapsed>=RuinBoneUnlockSeconds ? s.BoneCrawlerPrefab??s.RuinZombiePrefab : s.RuinZombiePrefab??s.RuinBatPrefab);
        }
        // Kept for QA callers; regular events use an explicit wave number.
        public Enemy SpawnBoss(bool unlock) => SpawnBossWave(unlock?Mathf.CeilToInt(c.StageData.unlockBossTime/c.StageData.bossInterval):1);
        public Enemy SpawnBossWave(int wave) => SpawnBossWave(wave,null,null);
        Enemy SpawnBossWave(int wave,Enemy overridePrefab,string label)
        {
            var d=c.StageData;wave=Mathf.Max(1,wave);
            bool unlock=c.StageIndex==0 ? wave>=2 : SurvivalRules.IsUnlockWave(wave,d.bossInterval,d.unlockBossTime);
            Enemy prefab=overridePrefab??(c.StageIndex==0 ? c.Game.Spawner.LumenStagBossPrefab :
                c.StageIndex==1 ? c.Game.Spawner.AmberCicadaBossPrefab : c.Game.Spawner.VioletLichBossPrefab);
            Enemy enemy=c.Game.Spawner.SpawnSpecial(c.Game.Spawner.ChooseSpawnPosition(),prefab??c.Game.Spawner.EnemyPrefab,true);
            if(enemy==null)return null;
            int unlockWave=Mathf.Max(1,Mathf.CeilToInt(d.unlockBossTime/d.bossInterval));
            float hp=wave>=unlockWave?Mathf.Max(d.bossHP,d.unlockBossHP)*SurvivalRules.Growth(wave-unlockWave+1,d.bossHealthGrowthPerWave):d.bossHP*SurvivalRules.Growth(wave,d.bossHealthGrowthPerWave);
            enemy.ConfigureEncounter(hp*d.difficulty/enemy.MaxHP,0.9f,2.2f*d.difficulty*SurvivalRules.Growth(wave,d.bossDamageGrowthPerWave),false,true,unlock);
            enemy.gameObject.AddComponent<BossBrain>().Initialize(c,enemy,wave);
            CurrentBoss=enemy;c.UI.Notify(label==null?"보스 등장 · "+wave+"회차":label+" 등장!");
            Debug.Log("Boss event: "+(label??prefab.name)+" at "+Elapsed.ToString("F1")+"s");return enemy;
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
