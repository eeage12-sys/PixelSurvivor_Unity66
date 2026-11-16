using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PixelSurvivor.Tests
{
    public sealed class PickupEncounterTests
    {
        GameManager game;
        [UnitySetUp] public IEnumerator Setup()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");
            yield return null;
            game=Object.FindAnyObjectByType<GameManager>();Assert.NotNull(game);
            game.Campaign.UseTemporaryProgress();game.Campaign.SelectCharacter(0);game.BeginRun();
            game.Campaign.Equipment.Reset(null);
        }
        [TearDown] public void Cleanup(){Time.timeScale=1;}
        [Test] public void EveryFieldDropUsesItsOwnReadableIcon()
        {
            for(int i=0;i<7;i++)
            {
                var kind=(FieldItemKind)i;
                var item=game.Campaign.Field.Drop(new Vector2(4+i,0),kind,5);
                var renderer=item.GetComponent<SpriteRenderer>();
                Assert.AreSame(FieldPickupArt.Icon(kind),renderer.sprite);
                Assert.AreEqual(Color.white,renderer.color);Assert.AreEqual(kind,item.Kind);
                Assert.NotNull(item.transform.Find("Pickup marker"));
            }
        }
        [Test] public void CollectingNewGoldAndHealArtStillAppliesTheOriginalEffectsOnce()
        {
            int gold=game.Campaign.RunGold;
            var coin=game.Campaign.Field.Drop(new Vector2(5,0),FieldItemKind.Gold,5);
            coin.Collect();int awarded=game.Campaign.RunGold;
            Assert.Greater(awarded,gold);coin.Collect();Assert.AreEqual(awarded,game.Campaign.RunGold);
            game.Health.TakeDamage(20);float hp=game.Health.CurrentHP;
            game.Campaign.Field.Drop(new Vector2(5,0),FieldItemKind.Heal).Collect();
            Assert.Greater(game.Health.CurrentHP,hp);Assert.LessOrEqual(game.Health.CurrentHP,game.Stats.MaxHP);
        }
        [UnityTest] public IEnumerator ReinforcementsKeepTheirOwnArtStatsAndMovement()
        {
            for(int i=0;i<6;i++)
            {
                var enemy=game.Spawner.SpawnAt(new Vector2(7+i,0),game.Spawner.EnemyPrefab);
                StageReinforcements.Apply(enemy,i);enemy.SetEliteAppearance(null);
                enemy.ConfigureEncounter(1,1,1,false,false,false);
                Assert.AreSame(StageReinforcements.Icon(i),enemy.GetComponent<SpriteRenderer>().sprite);
                Assert.AreEqual(enemy.MaxHP,enemy.CurrentHP);
                float start=enemy.transform.position.x;
                yield return new WaitForSeconds(.1f);
                Assert.Less(enemy.transform.position.x,start);enemy.TakeDamage(enemy.CurrentHP+1);
            }
        }
        [Test] public void MeadowIsDarkerAndStillGreen()
        {
            var ground=game.Bounds.transform.Find("Ground").GetComponent<SpriteRenderer>();
            Assert.Less(ground.color.r,.7f);Assert.Greater(ground.color.g,ground.color.r);
            Assert.Greater(ground.color.g,ground.color.b);
        }
    }
}
