using NUnit.Framework;
using UnityEngine;
namespace PixelSurvivor.Tests
{
    public sealed class SaveRulesTests
    {
        CampaignData data;CharacterData character;StageData stage;string key;
        [SetUp] public void Setup()
        {
            key="PixelSurvivor.Test."+System.Guid.NewGuid().ToString("N");
            data=ScriptableObject.CreateInstance<CampaignData>();character=ScriptableObject.CreateInstance<CharacterData>();character.id="test-character";
            stage=ScriptableObject.CreateInstance<StageData>();data.characters=new[]{character};data.stages=new[]{stage,stage,stage};
        }
        [TearDown] public void Cleanup(){PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();Object.DestroyImmediate(data);Object.DestroyImmediate(character);Object.DestroyImmediate(stage);}
        [Test] public void GoldUpgradesAndUnlocksSurviveReload()
        {
            var s=new SaveManager(data,key);s.AddGold(500);
            var upgrade=new PermanentUpgrade{id="power",baseCost=40,maxLevel=2,costGrowth=1.5f};
            Assert.IsTrue(s.Purchase("common.",upgrade));Assert.IsTrue(s.Purchase(character.id+".",upgrade));s.UnlockNext(0,3);s.Save();
            var loaded=new SaveManager(data,key);Assert.AreEqual(420,loaded.Progress.gold);Assert.AreEqual(1,loaded.Level("common.power"));Assert.AreEqual(1,loaded.Level(character.id+".power"));Assert.AreEqual(2,loaded.Progress.unlockedStages);
            CollectionAssert.Contains(loaded.Progress.unlockedCharacters,character.id);
        }
        [Test] public void InsufficientFundsAndMaxRankCannotChargeGold()
        {
            var s=new SaveManager(data,key);var upgrade=new PermanentUpgrade{id="hp",baseCost=40,maxLevel=1};
            Assert.IsFalse(s.Purchase("common.",upgrade));s.AddGold(100);Assert.IsTrue(s.Purchase("common.",upgrade));Assert.IsFalse(s.Purchase("common.",upgrade));Assert.AreEqual(60,s.Progress.gold);
        }
        [Test] public void BestRecordsArePerCharacterAndStageAndOnlyIncrease()
        {
            var save=new SaveManager(data,key);save.Record("ari","one",600);save.Record("ari","one",300);
            save.Record("nox","one",900);save.Record("ari","two",1200);save.Save();
            var loaded=new SaveManager(data,key);Assert.AreEqual(600,loaded.Best("ari","one"));Assert.AreEqual(900,loaded.Best("nox","one"));
            Assert.AreEqual(1200,loaded.Best("ari","two"));Assert.AreEqual(0,loaded.Best("nox","two"));
        }
        [Test] public void VersionOneSaveMigratesWithoutLosingGoldOrUpgradeIsolation()
        {
            PlayerPrefs.SetString(key,"{\"version\":1,\"gold\":321,\"unlockedStages\":2,\"upgrades\":[{\"id\":\"char1.haste\",\"level\":3}]}");
            var save=new SaveManager(data,key);Assert.AreEqual(321,save.Progress.gold);Assert.AreEqual(2,save.Progress.unlockedStages);
            Assert.AreEqual(3,save.Level("char1.haste"));Assert.AreEqual(0,save.Level("char2.haste"));Assert.AreEqual(0,save.Best("char1","stage0"));
            Assert.AreEqual(2,save.Progress.version);
        }
        [Test] public void InvalidSaveFallsBackToPlayableDefaults()
        {PlayerPrefs.SetString(key,"invalid-json");var s=new SaveManager(data,key);Assert.AreEqual(1,s.Progress.unlockedStages);Assert.AreEqual(0,s.Progress.gold);}
    }
}
