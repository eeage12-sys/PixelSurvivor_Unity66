using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
namespace PixelSurvivor.Tests
{
    public sealed class HudUpdateTests
    {
        GameManager game;Keyboard keyboard;InputAction cancel;bool oldHealth,oldEffects;
        [UnitySetUp] public IEnumerator Load()
        {
            oldHealth=DisplayOptions.HealthBar;oldEffects=DisplayOptions.HitEffects;
            DisplayOptions.HealthBar=true;DisplayOptions.HitEffects=true;
            yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");yield return null;
            game=Object.FindAnyObjectByType<GameManager>();game.Campaign.UseTemporaryProgress();game.Campaign.SelectCharacter(0);game.BeginRun();
        }
        [TearDown] public void Cleanup()
        {
            if(cancel!=null)cancel.actionMap.devices=null;
            if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);
            keyboard=null;cancel=null;
            DisplayOptions.HealthBar=oldHealth;DisplayOptions.HitEffects=oldEffects;Time.timeScale=1;
        }
        void Escape()
        {
            if(keyboard==null){keyboard=InputSystem.AddDevice<Keyboard>();cancel=Object.FindAnyObjectByType<InputSystemUIInputModule>().cancel.action;cancel.actionMap.devices=new InputDevice[]{keyboard};}
            Assert.IsTrue(cancel.enabled);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));InputSystem.Update();game.Campaign.UI.HandlePauseInput();
        }
        Button Button(string caption)
        {
            foreach(var b in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))if(b.name==caption+" Button")return b;
            Assert.Fail("Missing active button: "+caption);return null;
        }
        [UnityTest] public IEnumerator LegacyHudHiddenAndOnlyOneClockVisible()
        {
            game.Timer.AdvanceCampaign(123);game.TogglePause();yield return null;
            int clocks=0;
            foreach(var t in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))if(t.text=="00:02:03")clocks++;
            Assert.AreEqual(1,clocks);
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))Assert.AreNotEqual("GameplayPanel",t.name);
            Assert.NotNull(Object.FindAnyObjectByType<CompactHUD>());
        }
        [Test] public void EscapePausesAndResumesWithoutChangingRunProgress()
        {
            int level=game.Level.Level;Escape();Assert.AreEqual(RunState.Paused,game.State);Assert.AreEqual(0,Time.timeScale);
            Escape();Assert.AreEqual(RunState.Playing,game.State);Assert.AreEqual(1,Time.timeScale);Assert.AreEqual(level,game.Level.Level);
        }
        [Test] public void OptionsAndRecipesRemainPausedAndEscapeGoesBack()
        {
            Escape();Button("옵션").onClick.Invoke();Assert.AreEqual(RunState.Paused,game.State);
            Escape();Button("조합법 열기").onClick.Invoke();Assert.AreEqual(RunState.Paused,game.State);
            Escape();Assert.NotNull(Button("옵션"));Button("계속하기").onClick.Invoke();Assert.AreEqual(RunState.Playing,game.State);
        }
        [Test] public void EscapeCannotSkipLevelChoice()
        {
            game.Level.AddExperience(5);Assert.AreEqual(RunState.ChoosingUpgrade,game.State);
            Escape();Assert.AreEqual(RunState.ChoosingUpgrade,game.State);Assert.AreEqual(0,Time.timeScale);
        }
        [Test] public void PauseShowsOnePipPerMaximumLevelAndLightsCurrentLevel()
        {
            var c=game.Campaign;c.Equipment.Add(c.Character.startingWeapon);Escape();
            int total=0,lit=0;
            foreach(var image in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
                if(image.name=="Level light"){total++;if(image.color.g>.8f)lit++;}
            Assert.AreEqual(c.Equipment.Weapons[0].Maximum,total);Assert.AreEqual(2,lit);
        }
        [UnityTest] public IEnumerator FootBarFollowsPlayerAndCanBeHidden()
        {
            game.TogglePause();yield return null;
            var bar=GameObject.Find("Player foot health");Assert.NotNull(bar);
            game.Player.transform.position=new Vector3(4,2,0);yield return null;
            var view=game.View;var expected=view.WorldToScreenPoint(game.Player.transform.position+Vector3.down*.62f);
            Assert.Less(Vector2.Distance(bar.transform.position,expected),2);
            DisplayOptions.HealthBar=false;yield return null;Assert.IsFalse(bar.activeSelf);
        }
        [Test] public void PropHitsEmitEffectsOnlyForPositiveDamageWhenEnabled()
        {
            var p=game.Campaign.Field.Props[0];int before=Object.FindObjectsByType<PropHitEffect>(FindObjectsSortMode.None).Length;
            p.TakeDamage(0);Assert.AreEqual(before,Object.FindObjectsByType<PropHitEffect>(FindObjectsSortMode.None).Length);
            p.TakeDamage(1);Assert.AreEqual(before+1,Object.FindObjectsByType<PropHitEffect>(FindObjectsSortMode.None).Length);
            DisplayOptions.HitEffects=false;p.TakeDamage(1);Assert.AreEqual(before+1,Object.FindObjectsByType<PropHitEffect>(FindObjectsSortMode.None).Length);
        }
    }
}
