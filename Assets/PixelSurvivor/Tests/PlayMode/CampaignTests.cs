using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PixelSurvivor.Tests
{
    public sealed class CampaignTests
    {
        GameManager game;CampaignController c;
        [UnitySetUp] public IEnumerator Load()
        {
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");yield return null;
            game=Object.FindAnyObjectByType<GameManager>();Assert.NotNull(game);c=game.Campaign;Assert.NotNull(c,"Run Apply Infinite Survival Update.");c.UseTemporaryProgress();
        }
        void Begin(){if(!c.HasCharacterSelection)c.SelectCharacter(0);game.BeginRun();}
        [TearDown] public void Cleanup(){Time.timeScale=1;}
        [Test] public void FiveCharactersHaveDifferentStartersAndStageTwoIsLocked()
        {
            Assert.AreEqual(5,c.Data.characters.Length);Assert.AreEqual(1,c.Save.Progress.unlockedStages);Assert.IsFalse(c.SelectStage(1));
            var starters=new System.Collections.Generic.HashSet<WeaponData>();foreach(var ch in c.Data.characters)Assert.IsTrue(starters.Add(ch.startingWeapon));
            c.SelectCharacter(1);Begin();Assert.AreEqual(70,game.Stats.MaxHP);Assert.AreEqual(6.4f,game.Stats.MoveSpeed,0.01f);Assert.AreSame(c.Character.startingWeapon,c.Equipment.Weapons[0].data);
        }
        [Test] public void MovementAndBoundaryStillWork()
        {Begin();Vector2 before=game.Player.transform.position;game.Player.Move(Vector2.one,0.2f);Assert.AreEqual(1,Vector2.Distance(before,game.Player.transform.position),0.01f);game.Player.Move(Vector2.one,1000);Assert.LessOrEqual(game.Player.transform.position.x,game.Bounds.HalfSize.x-game.Player.Radius+0.001f);}
        [Test] public void LevelUpPausesAndChoiceResumes()
        {Begin();game.Level.AddExperience(5);Assert.AreEqual(RunState.ChoosingUpgrade,game.State);Assert.AreEqual(0,Time.timeScale);game.SelectUpgrade(0);Assert.AreEqual(RunState.Playing,game.State);Assert.AreEqual(1,Time.timeScale);}
        [Test] public void ChestEvolvesOnlyMaxWeaponAndNeverConsumesPassive()
        {Begin();var w=c.Equipment.Weapons[0];while(w.level<w.Maximum)c.Equipment.Add(w.data);c.Equipment.Add(w.data.evolutionPartner);c.OpenChest(true,false);Assert.AreEqual(RunState.Treasure,game.State);Assert.IsTrue(w.evolved);Assert.NotNull(c.Equipment.Find(w.data.evolutionPartner));Assert.AreEqual(1,c.Equipment.Weapons.Count);c.CloseChest();Assert.AreEqual(RunState.Playing,game.State);}
        [Test] public void JackpotCanReachMaxThenEvolveAndConvertRemainderToGold()
        {
            Begin();var w=c.Equipment.Weapons[0];for(int i=1;i<w.Maximum-1;i++)c.Equipment.Add(w.data);
            var partner=w.data.evolutionPartner;for(int i=0;i<partner.maxLevel;i++)c.Equipment.Add(partner);
            int gold=c.RunGold;var rewards=c.Chests.Resolve(5);Assert.AreEqual(5,rewards.Count);Assert.IsTrue(w.evolved);Assert.Greater(c.RunGold,gold);Assert.AreEqual(1,c.Equipment.Passives.Count);
        }
        [Test] public void Surviving20MinutesUnlocksStageAndDoomCatchesPlayer()
        {
            Begin();game.Timer.AdvanceCampaign(600);c.Stage.Tick(0);
            Assert.AreEqual(1,c.Stage.BossWaves);Assert.NotNull(c.Stage.CurrentBoss);
            game.Timer.AdvanceCampaign(600);c.Stage.Tick(0);
            Assert.IsTrue(c.Stage.DoomAppeared);Assert.NotNull(c.Stage.Doom);
            Assert.AreEqual(2,c.Save.Progress.unlockedStages);
            Assert.AreEqual(0,game.Spawner.ActiveEnemies.Count);
            Assert.AreNotSame(RedesignedSprites.Monster(3),c.Stage.Doom.GetComponent<SpriteRenderer>().sprite);
            Assert.IsNull(c.Stage.Doom.GetComponent<Enemy>());
            Assert.AreEqual(RunState.Playing,game.State);
            float hp=game.Health.CurrentHP;game.Health.TakeDamage(50);c.Field.Bomb();
            Assert.AreEqual(hp,game.Health.CurrentHP);
            Assert.IsTrue(c.Stage.Doom.gameObject.activeSelf);
            c.Stage.Doom.Advance(10);
            Assert.IsTrue(c.Stage.DoomCaughtPlayer);Assert.AreEqual(RunState.GameOver,game.State);
            game.ReturnToMenu();Assert.IsTrue(c.SelectStage(1));Begin();
            Assert.AreEqual(0,game.Timer.Elapsed);Assert.AreEqual(1,c.Equipment.Weapons.Count);
        }
        [UnityTest] public IEnumerator MidBossDoesNotClearAndDropsBossChest()
        {
            Begin();game.Timer.Tick(600);yield return null;var boss=c.Stage.CurrentBoss;Assert.IsFalse(boss.IsFinalBoss);
            boss.TakeDamage(boss.CurrentHP+1);Assert.AreEqual(RunState.Playing,game.State);Assert.Greater(c.Field.Items.Count,0);Assert.AreEqual(1,c.Save.Progress.unlockedStages);
        }
        [Test] public void EliteAlwaysDropsChestAndBombDoesNotInstakillHealthyBoss()
        {
            Begin();var elite=c.Stage.SpawnEnemy(true);Assert.AreSame(game.Spawner.MeadowFoxPrefab.GetComponent<SpriteRenderer>().sprite,elite.GetComponent<SpriteRenderer>().sprite);
            int before=c.Field.Items.Count;elite.TakeDamage(elite.CurrentHP+1);Assert.AreEqual(before+1,c.Field.Items.Count);
            var boss=c.Stage.SpawnBoss(true);float hp=boss.CurrentHP;c.Field.Bomb();Assert.IsTrue(boss.IsAlive);Assert.AreEqual(hp-c.Data.bombSpecialDamage,boss.CurrentHP,0.01f);
        }
        [Test] public void EachStageUsesOnlyItsCreaturesAndAUniqueBoss()
        {
            c.ToggleStagePreview();
            var normalNames=new[]
            {
                new[]{"Enemy", "EnemyMeadowFox"},
                new[]{"EnemyMoth", "EnemyBeetle", "EnemyAmberKestrel"},
                new[]{"EnemyBoneCrawler", "EnemyEmberWraith", "EnemyRuinBat"}
            };
            var bossSprites=new Sprite[3];
            for(int stage=0;stage<3;stage++)
            {
                Assert.IsTrue(c.SelectStage(stage));c.SelectCharacter(0);Begin();
                for(int i=0;i<20;i++)
                {
                    var enemy=c.Stage.SpawnEnemy(false);
                    Assert.NotNull(enemy);
                    string name=enemy.name.Replace("(Clone)","");
                    CollectionAssert.Contains(normalNames[stage],name);
                }
                var boss=c.Stage.SpawnBossWave(1);
                Assert.NotNull(boss);bossSprites[stage]=boss.GetComponent<SpriteRenderer>().sprite;
                Assert.NotNull(bossSprites[stage]);
                game.EndRun(false);game.ReturnToMenu();
            }
            Assert.AreNotSame(bossSprites[0],bossSprites[1]);
            Assert.AreNotSame(bossSprites[1],bossSprites[2]);
            Assert.AreNotSame(bossSprites[0],bossSprites[2]);
        }
        [Test] public void PermanentPurchaseAppliesNextRunAndRunEquipmentResets()
        {
            c.Save.AddGold(1000);var u=c.Data.commonUpgrades[0];Assert.IsTrue(c.Save.Purchase("common.",u));Begin();Assert.Greater(game.Stats.Damage,10);
            c.Equipment.Add(c.Data.weapons[1]);c.Equipment.Add(c.Data.passives[0]);game.EndRun(false);int gold=c.Save.Progress.gold;Begin();
            Assert.AreEqual(1,c.Equipment.Weapons.Count);Assert.AreEqual(0,c.Equipment.Passives.Count);Assert.AreEqual(gold,c.Save.Progress.gold);Assert.Greater(game.Stats.Damage,10);
        }
        [Test] public void BarrierBlocksEnemySweepButPlayerCanWalkThrough()
        {
            Begin();game.Player.Move(Vector2.right,0.1f);c.Supports.Summon(2);
            Vector2 from=game.Player.transform.position;Vector2 end=from+Vector2.right*5;
            Vector2 blocked=c.Supports.ResolveMove(from,end,0.4f);Assert.Less(blocked.x,end.x);
            game.Player.Move(Vector2.right,1);Assert.AreEqual(end.x,game.Player.transform.position.x,0.01f);Assert.AreEqual(1,c.Supports.Count);
        }
        [UnityTest] public IEnumerator AutoWeaponsDamageEnemyAndPropsDropItems()
        {
            Begin();var enemy=game.Spawner.SpawnAt(new Vector2(4,0));float end=Time.realtimeSinceStartup+5;
            while(enemy!=null && enemy.IsAlive && Time.realtimeSinceStartup<end)yield return null;
            Assert.IsTrue(enemy==null || !enemy.IsAlive);Assert.Greater(game.KillCount,0);
            int before=c.Field.Items.Count;var prop=c.Field.Props[0];prop.TakeDamage(10000);Assert.AreEqual(before+1,c.Field.Items.Count);
        }
        [Test] public void AllFiveCharactersApplyTheirArtStatsAndStarter()
        {
            for(int i=0;i<5;i++)
            {
                c.SelectCharacter(i);Begin();var ch=c.Character;
                Assert.AreEqual(ch.maxHP+c.Bonus(StatKind.MaxHP),game.Stats.MaxHP,0.001f);
                Assert.AreEqual(ch.moveSpeed*(1+c.Bonus(StatKind.MoveSpeed)),game.Stats.MoveSpeed,0.001f);
                Assert.AreEqual(ch.damage*(1+c.Bonus(StatKind.Damage)),game.Stats.Damage,0.001f);
                Assert.AreSame(RedesignedSprites.Direction(i,0)[0],game.Player.GetComponent<SpriteRenderer>().sprite);
                Assert.AreSame(ch.startingWeapon,c.Equipment.Weapons[0].data);
                game.EndRun(false);game.ReturnToMenu();
            }
        }
        [Test] public void BossKillAloneDoesNotUnlockStage()
        {
            Begin();var milestone=c.Stage.SpawnBossWave(2);c.Stage.SpawnBossWave(3);
            milestone.TakeDamage(milestone.CurrentHP+1);Assert.AreEqual(1,c.Save.Progress.unlockedStages);
            Assert.AreEqual(RunState.Playing,game.State);Assert.AreEqual(1,c.BossKills);
            game.Timer.AdvanceCampaign(1200);c.Stage.Tick(0);Assert.AreEqual(2,c.Save.Progress.unlockedStages);
        }
        [Test] public void BossHealthContinuesGrowingAfterUnlockWave()
        {
            Begin();var second=c.Stage.SpawnBossWave(2);var third=c.Stage.SpawnBossWave(3);var sixth=c.Stage.SpawnBossWave(6);
            Assert.Greater(third.MaxHP,second.MaxHP);Assert.Greater(sixth.MaxHP,third.MaxHP);
            Assert.Greater(third.ContactDamage,second.ContactDamage);
        }
        [Test] public void BossChestIsAlwaysThreeAndEliteKillIsCounted()
        {
            Begin();Assert.AreEqual(3,c.Chests.Open(true).Count);
            var e=c.Stage.SpawnEnemy(true);e.TakeDamage(e.CurrentHP+1);Assert.AreEqual(1,c.EliteKills);Assert.AreEqual(1,game.KillCount);
        }
        [Test] public void FastTestCannotUnlockStagesOrSaveGold()
        {c.TogglePractice();Begin();Assert.AreEqual(20,c.TimelineScale);int gold=c.Save.Progress.gold;c.AddGold(100);game.Timer.AdvanceCampaign(60);c.Stage.Tick(0);Assert.IsTrue(c.Stage.DoomAppeared);game.EndRun(false);Assert.AreEqual(gold,c.Save.Progress.gold);Assert.AreEqual(1,c.Save.Progress.unlockedStages);}
        [Test] public void DamageFlashesAndShakesSpritesWithoutMovingCombatCoordinates()
        {
            Begin();var enemy=game.Spawner.SpawnAt(new Vector2(4,0));Vector3 enemyPosition=enemy.transform.position;
            enemy.TakeDamage(1);
            var enemyFeedback=enemy.GetComponent<HitFeedbackVisual>();
            Assert.NotNull(enemyFeedback);Assert.IsTrue(enemyFeedback.IsShaking);
            Assert.NotNull(enemyFeedback.Display);Assert.AreEqual(enemyPosition,enemy.transform.position);
            float hp=game.Health.CurrentHP;game.Health.TakeDamage(10);
            Assert.Less(game.Health.CurrentHP,hp);
            Assert.IsTrue(game.Player.GetComponent<HitFeedbackVisual>().IsShaking);
            Assert.NotNull(game.RuntimeRoot.Find("Player blood flecks"));
        }
        [UnityTest] public IEnumerator HitBurstReturnsToPoolAndIsReused()
        {
            Begin();
            CombatVfx.Hit(game,new Vector3(2,2),.5f,false,false);
            var first=game.RuntimeRoot.Find("Hit burst");Assert.NotNull(first);
            yield return new WaitForSeconds(.4f);
            Assert.IsFalse(first.gameObject.activeSelf);
            CombatVfx.Hit(game,new Vector3(3,2),.5f,false,false);
            Assert.AreSame(first,game.RuntimeRoot.Find("Hit burst"));
        }
    }
}
