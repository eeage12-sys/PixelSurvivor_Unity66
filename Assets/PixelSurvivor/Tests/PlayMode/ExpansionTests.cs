using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace PixelSurvivor.Tests
{
    public sealed class ExpansionTests
    {
        GameManager game;
        [UnitySetUp] public IEnumerator Setup()
        {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");
            yield return null;
            game = Object.FindAnyObjectByType<GameManager>(); Assert.NotNull(game);
            Assert.NotNull(game.Campaign);game.Campaign.UseTemporaryProgress();game.Campaign.SelectCharacter(0);
            Assert.IsTrue(game.Spawner.HasExpandedRoster, "Run Apply Monster and Animation Update first.");
            game.Spawner.enabled = false; game.BeginRun();
            game.Player.GetComponent<AutoAttack>().enabled = false;game.Campaign.Equipment.Reset(null);
        }
        [TearDown] public void Cleanup() { Time.timeScale = 1; }
        [Test] public void UnlockBoundariesAndRestartUseTheCorrectRoster()
        {
            var s = game.Spawner;
            Assert.AreSame(s.EnemyPrefab, s.SelectPrefab(29.99f, 0.1f));
            Assert.AreSame(s.MothPrefab, s.SelectPrefab(30, 0.1f));
            Assert.AreSame(s.EnemyPrefab, s.SelectPrefab(89.99f, 0.8f));
            Assert.AreSame(s.BeetlePrefab, s.SelectPrefab(90, 0.1f));
            Assert.AreSame(s.MothPrefab, s.SelectPrefab(90, 0.3f));
            Assert.AreSame(s.EnemyPrefab, s.SelectPrefab(90, 0.8f));
            game.Timer.Tick(120); game.EndRun(false); game.BeginRun();
            Assert.AreEqual(0, game.Timer.Elapsed);
            Assert.AreSame(s.EnemyPrefab, s.SelectPrefab(game.Timer.Elapsed, 0.1f));
        }
        [UnityTest] public IEnumerator NewEnemiesTrackAndUseTheirOwnStats()
        {
            var moth = game.Spawner.SpawnAt(new Vector2(5, 0), game.Spawner.MothPrefab);
            var beetle = game.Spawner.SpawnAt(new Vector2(-5, 0), game.Spawner.BeetlePrefab);
            Assert.AreSame(RedesignedSprites.Monster(1), moth.GetComponent<SpriteRenderer>().sprite);
            Assert.AreSame(RedesignedSprites.Monster(2), beetle.GetComponent<SpriteRenderer>().sprite);
            Assert.AreEqual(10, moth.CurrentHP); Assert.AreEqual(60, beetle.CurrentHP);
            Assert.AreEqual(8, moth.ContactDamage); Assert.AreEqual(18, beetle.ContactDamage);
            yield return new WaitForSeconds(0.2f);
            Assert.Less(moth.transform.position.x, 5); Assert.Greater(beetle.transform.position.x, -5);
            Assert.Greater(5 - moth.transform.position.x, beetle.transform.position.x + 5);
        }
        [UnityTest] public IEnumerator BeetleContactAndBothDeathsUseExistingCombatLoop()
        {
            var beetle = game.Spawner.SpawnAt(Vector2.zero, game.Spawner.BeetlePrefab);
            yield return null; yield return null;
            Assert.AreEqual(82, game.Health.CurrentHP);
            beetle.TakeDamage(10); Assert.AreEqual(50, beetle.CurrentHP); Assert.AreEqual(0, game.KillCount);
            beetle.TakeDamage(50); beetle.TakeDamage(50); Assert.AreEqual(1, game.KillCount);
            var moth = game.Spawner.SpawnAt(new Vector2(5, 0), game.Spawner.MothPrefab);
            moth.TakeDamage(10); Assert.AreEqual(2, game.KillCount);
            Assert.AreEqual(0, game.Spawner.ActiveEnemies.Count);
            // Inspect before next Update can collect the beetle's gem.
            Assert.AreEqual(2, Object.FindObjectsByType<ExperienceGem>(FindObjectsSortMode.None).Length);
        }
        [Test] public void WalkChangesFrameDirectionAndStopsAtTheBoundary()
        {
            var animator = game.Player.GetComponent<PlayerSpriteAnimator>(); Assert.IsTrue(animator.IsConfigured);
            var renderer = game.Player.GetComponent<SpriteRenderer>();
            game.Player.Move(Vector2.right, 0.1f);
            Assert.AreSame(RedesignedSprites.Direction(0,3)[1], renderer.sprite);
            game.Player.Move(Vector2.zero, 0.1f);
            Assert.AreSame(RedesignedSprites.Direction(0,3)[0], renderer.sprite);
            game.Player.Move(Vector2.up, 0.1f); Assert.AreSame(RedesignedSprites.Direction(0,1)[1], renderer.sprite);
            game.Player.Move(Vector2.left, 0.1f); Assert.AreSame(RedesignedSprites.Direction(0,2)[1], renderer.sprite);
            game.Player.Move(Vector2.down, 0.1f); Assert.AreSame(RedesignedSprites.Direction(0,0)[1], renderer.sprite);
            game.Player.Move(Vector2.right, 1000);
            game.Player.Move(Vector2.right, 0.1f);
            Assert.AreSame(RedesignedSprites.Direction(0,3)[0], renderer.sprite, "No walking in place against boundary.");
        }
        [Test] public void DiagonalWalkKeepsFacingAndMovesOnBothAxes()
        {
            var animator = game.Player.GetComponent<PlayerSpriteAnimator>();
            Vector2 start = game.Player.transform.position;
            int facing = -1;
            for (int i = 0; i < 20; i++)
            {
                game.Player.Move(new Vector2(-1f, 1f + (i % 2 == 0 ? .0001f : -.0001f)), .016f);
                if (facing < 0) facing = animator.Facing;
                Assert.AreEqual(facing, animator.Facing, "Facing changed during a steady up-left diagonal.");
            }
            Vector2 travelled = (Vector2)game.Player.transform.position - start;
            Assert.Less(travelled.x, -.5f);
            Assert.Greater(travelled.y, .5f);
        }
        [Test] public void UpgradeFreezesAnimationAndRestartResetsFacing()
        {
            var renderer = game.Player.GetComponent<SpriteRenderer>();
            game.Player.Move(Vector2.left, 0.1f); Sprite before = renderer.sprite;
            game.Level.AddExperience(5); game.Player.Move(Vector2.up, 1);
            Assert.AreSame(before, renderer.sprite);
            game.SelectUpgrade(0); game.EndRun(false); game.BeginRun();
            Assert.AreSame(RedesignedSprites.Direction(0,0)[0], renderer.sprite);
            Assert.AreEqual(0, game.Player.GetComponent<PlayerSpriteAnimator>().Facing);
        }
    }
}
