using NUnit.Framework;
namespace PixelSurvivor.Tests
{
    public sealed class SurvivalRulesTests
    {
        [TestCase(0,0)][TestCase(300,0)][TestCase(599,0)][TestCase(600,1)][TestCase(1200,2)][TestCase(1800,3)][TestCase(3600,6)][TestCase(36000,60)]
        public void BossScheduleContinuesBeyondTwentyMinutes(float seconds,int count)=>Assert.AreEqual(count,SurvivalRules.DueBosses(seconds,600));
        [TestCase(1,false)][TestCase(2,true)][TestCase(3,false)][TestCase(60,false)]
        public void OnlyTheTwentyMinuteWaveUnlocks(int wave,bool expected)=>Assert.AreEqual(expected,SurvivalRules.IsUnlockWave(wave,600,1200));
        [TestCase(0,"00:00:00")][TestCase(877,"00:14:37")][TestCase(3600,"01:00:00")][TestCase(360000,"100:00:00")]
        public void SurvivalClockIncludesHours(float seconds,string text)=>Assert.AreEqual(text,SurvivalRules.Clock(seconds));
        [Test] public void BossGrowthAndEliteFrequencyContinueButRateHasSafetyFloor()
        {
            Assert.Greater(SurvivalRules.Growth(6,0.75f),SurvivalRules.Growth(2,0.75f));
            Assert.Less(SurvivalRules.EliteInterval(3600,90,20,0.05f),90);
            Assert.AreEqual(20,SurvivalRules.EliteInterval(360000,90,20,0.05f));
        }
    }
}
