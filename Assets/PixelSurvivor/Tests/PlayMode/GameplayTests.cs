using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
namespace PixelSurvivor.Tests
{
    public sealed class GameplayTests
    {
        GameManager game;
        Keyboard keyboard;
        [UnitySetUp] public IEnumerator LoadScene()
        {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");
            yield return null;
            game = Object.FindFirstObjectByType<GameManager>();
            Assert.NotNull(game, "Run Tools > Pixel Survivor > Setup - Rebuild MVP Scene before these tests.");
            Assert.IsTrue(game.HasRequiredReferences());
            game.Spawner.enabled = false;
        }
        [TearDown] public void Cleanup()
        {
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            keyboard = null; Time.timeScale = 1;
        }
        [Test] public void StartButtonStartsRunAndRefsAreComplete()
        {
            Assert.AreEqual(RunState.Menu, game.State); Assert.AreEqual(0, game.Timer.Elapsed);
            Button start = null;
            foreach (Button b in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (b.name == "StartButton") start = b;
            Assert.NotNull(start); start.onClick.Invoke();
            Assert.AreEqual(RunState.Playing, game.State); Assert.AreEqual(1, Time.timeScale);
            Assert.AreEqual(100, game.Health.CurrentHP); Assert.AreEqual(1, game.Level.Level);
        }
        [Test] public void RealInputActionNormalizesWASDAndMapClampsPlayer()
        {
            game.BeginRun(); keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.D)); InputSystem.Update();
            Vector2 input = game.Player.MoveAction.action.ReadValue<Vector2>();
            Assert.Greater(input.x, 0); Assert.Greater(input.y, 0); Assert.LessOrEqual(input.magnitude, 1.0001f);
            Vector2 before = game.Player.transform.position;
            game.Player.Move(input, 0.2f);
            Assert.AreEqual(1, Vector2.Distance(before, game.Player.transform.position), 0.01f);
            game.Player.Move(Vector2.one, 1000);
            Vector2 p = game.Player.transform.position;
            Assert.LessOrEqual(p.x, game.Bounds.HalfSize.x - game.Player.Radius + 0.001f);
            Assert.LessOrEqual(p.y, game.Bounds.HalfSize.y - game.Player.Radius + 0.001f);
        }
        [UnityTest] public IEnumerator SpawnerCreatesEnemiesOutsideViewport()
        {
            game.BeginRun(); game.Spawner.enabled = true;
            yield return new WaitForSeconds(0.6f);
            Assert.Greater(game.Spawner.ActiveEnemies.Count, 0);
            Enemy enemy = game.Spawner.ActiveEnemies[0];
            Vector3 v = game.View.WorldToViewportPoint(enemy.transform.position);
            Assert.IsTrue(v.x < 0 || v.x > 1 || v.y < 0 || v.y > 1);
        }
        [UnityTest] public IEnumerator EnemyTracksPlayerAndContactDealsDamageWithInvulnerability()
        {
            game.BeginRun(); game.Player.GetComponent<AutoAttack>().enabled = false;
            Enemy enemy = game.Spawner.SpawnAt(new Vector2(3, 0));
            yield return new WaitForSeconds(0.2f);
            Assert.Less(enemy.transform.position.x, 3);
            enemy.transform.position = game.Player.transform.position;
            yield return null; yield return null;
            Assert.AreEqual(90, game.Health.CurrentHP);
            game.Health.TakeDamage(10); Assert.AreEqual(90, game.Health.CurrentHP, "No multi-enemy instant kill during immunity.");
        }
        [UnityTest] public IEnumerator AutoAttackKillsEnemyDropsOneGemAndCollectsEXP()
        {
            game.BeginRun();
            game.Spawner.SpawnAt(new Vector2(4.5f, 0)); // Real moving target; gem initially drops outside pickup range.
            float timeout = Time.realtimeSinceStartup + 4;
            while (game.KillCount == 0 && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.AreEqual(1, game.KillCount);
            ExperienceGem[] gems = Object.FindObjectsByType<ExperienceGem>(FindObjectsSortMode.None);
            Assert.AreEqual(1, gems.Length);
            game.Player.transform.position = gems[0].transform.position;
            yield return null; yield return null;
            Assert.AreEqual(1, game.Level.Experience);
            Assert.AreEqual(0, Object.FindObjectsByType<ExperienceGem>(FindObjectsSortMode.None).Length);
        }
        [UnityTest] public IEnumerator LevelUpFreezesTimeOffersThreeAndCardActuallyApplies()
        {
            game.BeginRun(); game.Level.AddExperience(5);
            Assert.AreEqual(2, game.Level.Level); Assert.AreEqual(RunState.ChoosingUpgrade, game.State);
            Assert.AreEqual(0, Time.timeScale);
            float elapsed = game.Timer.Elapsed;
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.AreEqual(elapsed, game.Timer.Elapsed);
            var choices = game.Choices;
            Assert.AreEqual(3, new System.Collections.Generic.HashSet<UpgradeKind>(choices).Count);
            float damage = game.Stats.Damage, interval = game.Stats.AttackInterval, speed = game.Stats.MoveSpeed;
            float maxHP = game.Stats.MaxHP, projectileSpeed = game.Stats.ProjectileSpeed;
            UpgradeButton first = null;
            foreach (UpgradeButton card in Object.FindObjectsByType<UpgradeButton>(FindObjectsSortMode.None))
                if (card.name == "Choice 1") first = card;
            Assert.NotNull(first); first.Button.onClick.Invoke();
            switch (choices[0])
            {
                case UpgradeKind.Damage: Assert.AreEqual(damage * 1.2f, game.Stats.Damage, 0.001f); break;
                case UpgradeKind.AttackSpeed: Assert.AreEqual(interval * 0.85f, game.Stats.AttackInterval, 0.001f); break;
                case UpgradeKind.MoveSpeed: Assert.AreEqual(speed * 1.1f, game.Stats.MoveSpeed, 0.001f); break;
                case UpgradeKind.MaxHP: Assert.AreEqual(maxHP + 20, game.Stats.MaxHP); Assert.AreEqual(maxHP + 20, game.Health.CurrentHP); break;
                case UpgradeKind.ProjectileSpeed: Assert.AreEqual(projectileSpeed * 1.2f, game.Stats.ProjectileSpeed, 0.001f); break;
            }
            Assert.AreEqual(RunState.Playing, game.State); Assert.AreEqual(1, Time.timeScale);
        }
        [Test] public void ExcessExperienceQueuesEachUpgradeAndPreservesRemainder()
        {
            game.BeginRun(); game.Level.AddExperience(15);
            Assert.AreEqual(3, game.Level.Level); Assert.AreEqual(2, game.Level.Experience);
            Assert.AreEqual(2, game.Level.PendingChoices);
            game.SelectUpgrade(0); Assert.AreEqual(RunState.ChoosingUpgrade, game.State);
            game.SelectUpgrade(0); Assert.AreEqual(RunState.Playing, game.State);
            Assert.AreEqual(0, game.Level.PendingChoices);
        }
        [UnityTest] public IEnumerator DeathAndRestartClearEveryRunValueAndObject()
        {
            game.BeginRun(); game.Level.AddExperience(5); game.SelectUpgrade(0);
            game.Spawner.SpawnAt(new Vector2(5, 0)); game.RegisterKill();
            game.Timer.Tick(30); game.Health.TakeDamage(10000);
            Assert.AreEqual(RunState.GameOver, game.State); Assert.AreEqual(0, game.Health.CurrentHP);
            Button restart = null;
            foreach (Button b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None)) if (b.name == "RestartButton") restart = b;
            Assert.NotNull(restart); restart.onClick.Invoke(); yield return null;
            Assert.AreEqual(RunState.Playing, game.State); Assert.AreEqual(100, game.Health.CurrentHP);
            Assert.AreEqual(1, game.Level.Level); Assert.AreEqual(0, game.Level.Experience); Assert.AreEqual(0, game.KillCount);
            Assert.AreEqual(0, game.Spawner.ActiveEnemies.Count); Assert.AreEqual(0, game.RuntimeRoot.childCount);
            Assert.AreEqual(10, game.Stats.Damage); Assert.Less(game.Timer.Elapsed, 1f);
        }
        [UnityTest] public IEnumerator TimerReachesExactDurationAndShowsVictory()
        {
            game.BeginRun(); Assert.AreEqual(300, game.Timer.Duration);
            Assert.IsTrue(game.Timer.Tick(300)); yield return null;
            Assert.AreEqual(300, game.Timer.Elapsed); Assert.AreEqual(RunState.Victory, game.State);
            Assert.AreEqual(0, Time.timeScale); Assert.AreEqual("05:00", GameRules.Clock(game.Timer.Elapsed));
        }
        [UnityTest] public IEnumerator HUDReflectsHealthAndExperience()
        {
            game.BeginRun(); game.Health.TakeDamage(25); game.Level.AddExperience(2);
            yield return null;
            bool hpFound = false, expFound = false;
            foreach (Image image in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                if (image.name != "Fill") continue;
                if (image.transform.parent.name == "HP Bar") { Assert.AreEqual(0.75f, image.fillAmount, 0.001f); hpFound = true; }
                if (image.transform.parent.name == "EXP Bar") { Assert.AreEqual(0.4f, image.fillAmount, 0.001f); expFound = true; }
            }
            Assert.IsTrue(hpFound && expFound);
        }
    }
}
