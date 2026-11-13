using System;
namespace PixelSurvivor
{
    // Pure rules shared by the director, HUD and tests. No elapsed-time win condition.
    public static class SurvivalRules
    {
        public static int MinuteSpawnCount(float seconds,int initial,int perMinute,int maximum)
        {
            int minute=(int)Math.Floor(Math.Max(0,seconds)/60);
            return (int)Math.Min(Math.Max(1,maximum),(long)Math.Max(1,initial)+(long)minute*Math.Max(1,perMinute));
        }
        public static string Clock(float seconds)
        {
            long total=(long)Math.Max(0,seconds);
            return (total/3600).ToString("00")+":"+((total/60)%60).ToString("00")+":"+(total%60).ToString("00");
        }
        public static int DueBosses(float seconds,float interval) => (int)Math.Floor(Math.Max(0,seconds)/Math.Max(1,interval));
        public static bool IsUnlockWave(int wave,float interval,float unlockTime) => wave==Math.Max(1,(int)Math.Ceiling(unlockTime/Math.Max(1,interval)));
        public static float Growth(int wave,float perWave) => 1+Math.Max(0,wave-1)*Math.Max(0,perWave);
        public static float EliteInterval(float seconds,float initial,float minimum,float growth) => Math.Max(minimum,initial/(1+Math.Max(0,seconds)/60*Math.Max(0,growth)));
    }
}
