using UnityEngine;
namespace PixelSurvivor
{
    public static class DisplayOptions
    {
        const string Prefix="PixelSurvivor.Display.";
        public static bool HitEffects {get=>PlayerPrefs.GetInt(Prefix+"HitEffects",1)!=0;set{PlayerPrefs.SetInt(Prefix+"HitEffects",value?1:0);PlayerPrefs.Save();}}
        public static bool HealthBar {get=>PlayerPrefs.GetInt(Prefix+"HealthBar",1)!=0;set{PlayerPrefs.SetInt(Prefix+"HealthBar",value?1:0);PlayerPrefs.Save();}}
    }
}
