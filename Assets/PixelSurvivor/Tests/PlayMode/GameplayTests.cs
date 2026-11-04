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
        GameManager game;Keyboard keyboard;bool savedHealthBar;
        [UnitySetUp] public IEnumerator LoadScene()
        {
            savedHealthBar=DisplayOptions.HealthBar;DisplayOptions.HealthBar=true;
            Time.timeScale=1;yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");yield return null;
            game=Object.FindAnyObjectByType<GameManager>();Assert.NotNull(game);Assert.IsTrue(game.HasRequiredReferences());
            Assert.NotNull(game.Campaign);game.Campaign.UseTemporaryProgress();
        }
        [TearDown] public void Cleanup(){DisplayOptions.HealthBar=savedHealthBar;if(game!=null && game.Player.MoveAction!=null)game.Player.MoveAction.action.actionMap.devices=null;if(keyboard!=null && keyboard.added)InputSystem.RemoveDevice(keyboard);keyboard=null;Time.timeScale=1;}
        void Begin(){game.Campaign.SelectCharacter(0);game.BeginRun();}
        Button ActiveButton(string name)
        {foreach(var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))if(b.name==name+" Button")return b;return null;}
        [Test] public void MenuRequiresCharacterThenStageBeforeRun()
        {
            game.BeginRun();Assert.AreEqual(RunState.Menu,game.State);
            var start=ActiveButton("시작");Assert.NotNull(start);start.onClick.Invoke();
            var next=ActiveButton("다음: 스테이지 선택");Assert.NotNull(next);Assert.IsFalse(next.interactable);
            ActiveButton("Character 1").onClick.Invoke();Assert.IsTrue(next.interactable);next.onClick.Invoke();
            var run=ActiveButton("전투 시작");Assert.NotNull(run);Assert.IsTrue(run.interactable);run.onClick.Invoke();
            Assert.AreEqual(RunState.Playing,game.State);Assert.AreEqual(70,game.Health.CurrentHP);
        }
        [TestCase(Key.W,0,1)][TestCase(Key.S,0,-1)][TestCase(Key.A,-1,0)][TestCase(Key.D,1,0)]
        [TestCase(Key.UpArrow,0,1)][TestCase(Key.DownArrow,0,-1)][TestCase(Key.LeftArrow,-1,0)][TestCase(Key.RightArrow,1,0)]
        public void BothKeyboardLayoutsDriveTheSameMoveAction(Key key,float x,float y)
        {
            Begin();keyboard=InputSystem.AddDevice<Keyboard>();game.Player.MoveAction.action.actionMap.devices=new InputDevice[]{keyboard};
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));InputSystem.Update();
            Assert.AreEqual(new Vector2(x,y),game.Player.MoveAction.action.ReadValue<Vector2>());
        }
        [TestCase(Key.W,Key.D)][TestCase(Key.UpArrow,Key.RightArrow)][TestCase(Key.W,Key.UpArrow)][TestCase(Key.A,Key.LeftArrow)]
        public void DiagonalAndDuplicateLayoutsNeverMultiplySpeed(Key a,Key b)
        {
            Begin();keyboard=InputSystem.AddDevice<Keyboard>();game.Player.MoveAction.action.actionMap.devices=new InputDevice[]{keyboard};
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(a,b));InputSystem.Update();
            var input=game.Player.MoveAction.action.ReadValue<Vector2>();Assert.AreEqual(1,input.magnitude,0.0001f);
            Vector2 before=game.Player.transform.position;game.Player.Move(input,0.2f);
            Assert.AreEqual(1,Vector2.Distance(before,game.Player.transform.position),0.001f);
        }
        [Test] public void TimerNeverReturnsVictoryAtFiveTwentyOrSixtyMinutes()
        {
            Begin();Assert.IsFalse(game.Timer.Tick(300));Assert.AreEqual(RunState.Playing,game.State);
            Assert.IsFalse(game.Timer.Tick(900));Assert.AreEqual(RunState.Playing,game.State);
            Assert.IsFalse(game.Timer.Tick(2400));Assert.AreEqual(3600,game.Timer.Elapsed);
            game.EndRun(true);Assert.AreEqual(RunState.Playing,game.State);
        }
        [Test] public void DeathSavesBestAndRestartClearsRunState()
        {
            Begin();game.Timer.AdvanceCampaign(900);game.Campaign.AddGold(30);game.Campaign.Equipment.Add(game.Campaign.Data.weapons[1]);
            game.Health.TakeDamage(10000);Assert.AreEqual(RunState.GameOver,game.State);
            Assert.AreEqual(900,game.Campaign.Save.Best(game.Campaign.Character.id,game.Campaign.StageData.id));
            game.BeginRun();Assert.AreEqual(1,game.Campaign.Equipment.Weapons.Count);Assert.AreEqual(0,game.Campaign.Equipment.Passives.Count);
            Assert.AreEqual(0,game.Timer.Elapsed);Assert.AreEqual(100,game.Health.CurrentHP);Assert.AreEqual(1,game.Level.Level);Assert.AreEqual(0,game.KillCount);
            Assert.Greater(game.Campaign.Save.Progress.gold,0);
        }
        [UnityTest] public IEnumerator HUDUsesIncreasingSurvivalClockAndHealthBars()
        {
            Begin();game.Health.TakeDamage(25);game.Level.AddExperience(2);game.Timer.AdvanceCampaign(3670);game.TogglePause();yield return null;
            bool hp=false,exp=false,clock=false;
            foreach(var im in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                if(im.name=="Health fill"){Assert.AreEqual(0.75f,im.fillAmount,0.001f);hp=true;}
                if(im.name=="Experience fill"){Assert.AreEqual(0.4f,im.fillAmount,0.001f);exp=true;}
            }
            foreach(var text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))if(text.text=="01:01:10")clock=true;
            Assert.IsTrue(hp && exp && clock);
        }
        [Test] public void AllMaxDoesNotOfferStatGoldHealOrSupportCards()
        {
            Begin();var c=game.Campaign;
            for(int i=0;i<7;i++){for(int j=0;j<10;j++){c.Equipment.Add(c.Data.weapons[i]);c.Equipment.Add(c.Data.passives[i]);}}
            game.Level.AddExperience(5);Assert.AreEqual(2,game.Level.Level);Assert.AreEqual(RunState.Playing,game.State);Assert.AreEqual(0,game.Level.PendingChoices);
        }
    }
}
