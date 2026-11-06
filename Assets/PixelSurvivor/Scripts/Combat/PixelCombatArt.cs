using UnityEngine;

namespace PixelSurvivor
{
    // All visuals are imported as point-filtered sprites from the generated pixel set.
    public static class PixelCombatArt
    {
        static readonly Sprite[] projectiles=new Sprite[12],flashes=new Sprite[12];

        public static Sprite Projectile(WeaponPattern pattern)
        {
            int i=(int)pattern;
            if(i<0 || i>=projectiles.Length)return null;
            if(projectiles[i]==null)projectiles[i]=Resources.Load<Sprite>("PixelCombat/Projectile"+i);
            return projectiles[i];
        }

        public static Sprite Flash(WeaponPattern pattern)
        {
            int i=(int)pattern;
            if(i<0 || i>=flashes.Length)return null;
            if(flashes[i]==null)flashes[i]=Resources.Load<Sprite>("PixelCombat/Flash"+i);
            return flashes[i];
        }
    }
}
