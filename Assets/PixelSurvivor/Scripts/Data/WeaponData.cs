using UnityEngine;
namespace PixelSurvivor
{
    public enum WeaponPattern { Bolt, Dagger, Whip, Orbit, Scythe, Storm, Nova, Seeker, Fan, Cross, Trap, Spear }
    [CreateAssetMenu(menuName="Pixel Survivor/Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        public string id, title, evolvedTitle;
        [TextArea] public string description;
        public WeaponPattern pattern;
        public PassiveEquipmentData evolutionPartner;
        public Sprite icon, evolvedIcon;
        public Color color = Color.cyan;
        [Min(1)] public int maxLevel = 5;
        [Min(0.1f)] public float damage = 10, cooldown = 0.8f, area = 1, duration = 1, projectileSpeed = 8;
        [Min(1)] public int projectileCount = 1;
        [Min(0)] public int penetration;
        [Min(0)] public float knockback = 0.1f;
        [Header("Per level growth")]
        public float damagePerLevel = 0.25f, areaPerLevel = 0.12f, cooldownPerLevel = 0.04f;
        [Min(0)] public int extraProjectileEvery = 2;
        [Header("Evolution")]
        public float evolvedDamage = 2, evolvedArea = 1.4f, evolvedCooldown = 0.7f;
        public int evolvedExtraProjectiles = 1, evolvedPenetration = 2;
    }
}
