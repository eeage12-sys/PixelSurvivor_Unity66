using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace PixelSurvivor.Tests
{
    public sealed class EncounterReadabilityTests
    {
        [TestCase(0,1)][TestCase(59.99f,1)][TestCase(60,2)][TestCase(119.99f,2)]
        [TestCase(120,3)][TestCase(1140,20)][TestCase(3600,24)]
        public void SpawnBatchesIncreaseOnMinuteBoundaries(float time,int expected)
        {Assert.AreEqual(expected,SurvivalRules.MinuteSpawnCount(time,1,1,24));}
        [Test] public void EveryMinuteBeforeThePursuerAddsEnemiesForAllStages()
        {
            for(int stage=0;stage<3;stage++)
            {
                var data=UnityEditor.AssetDatabase.LoadAssetAtPath<StageData>("Assets/PixelSurvivor/Data/Stage_"+stage+".asset");
                Assert.NotNull(data);
                int initial=data.initialSpawnCount;
                for(int minute=1;minute<20;minute++)
                    Assert.Greater(SurvivalRules.MinuteSpawnCount(minute*60,initial,data.spawnCountPerMinute,data.maximumMinuteSpawnCount),
                        SurvivalRules.MinuteSpawnCount((minute-1)*60,initial,data.spawnCountPerMinute,data.maximumMinuteSpawnCount));
            }
        }
        [Test] public void ReinforcementsRespectStageAndTimeAndRetainTheOriginalRoster()
        {
            for(int stage=0;stage<3;stage++)
            {
                Assert.AreEqual(-1,StageReinforcements.Select(stage,119.99f,0));
                Assert.AreEqual(stage*2,StageReinforcements.Select(stage,120,0));
                Assert.AreEqual(stage*2,StageReinforcements.Select(stage,359.99f,0));
                Assert.AreEqual(stage*2+1,StageReinforcements.Select(stage,360,0));
                Assert.AreEqual(stage*2,StageReinforcements.Select(stage,360,.2f));
                Assert.AreEqual(-1,StageReinforcements.Select(stage,600,.8f));
            }
        }
        [Test] public void PickupIconsAreDistinctCachedAndNamed()
        {
            var icons=new HashSet<Sprite>();
            for(int i=0;i<7;i++)
            {
                var kind=(FieldItemKind)i;var icon=FieldPickupArt.Icon(kind);
                Assert.NotNull(icon);Assert.IsTrue(icons.Add(icon));
                Assert.AreSame(icon,FieldPickupArt.Icon(kind));
                Assert.AreEqual(FilterMode.Point,icon.texture.filterMode);
                Assert.IsNotEmpty(FieldPickupArt.Label(kind));Assert.IsNotEmpty(FieldPickupArt.Description(kind));
            }
        }
    }
}
