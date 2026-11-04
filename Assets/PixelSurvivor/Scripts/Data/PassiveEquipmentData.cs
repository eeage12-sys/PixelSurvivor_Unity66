using UnityEngine;
namespace PixelSurvivor
{
    public enum StatKind { Damage, MaxHP, MoveSpeed, Cooldown, Area, Duration, Projectiles, Pickup, Armor, Experience, Gold, Critical, Leech, LowHPDamage, Dodge, Knockback }
    [System.Serializable] public struct StatBonus
    {
        public StatKind kind; public float amount;
        public StatBonus(StatKind k, float value) { kind=k; amount=value; }
    }
    [CreateAssetMenu(menuName="Pixel Survivor/Passive Equipment")]
    public sealed class PassiveEquipmentData : ScriptableObject
    {
        public string id, title;
        [TextArea] public string description;
        public Sprite icon;
        [Min(1)] public int maxLevel = 5;
        public StatBonus bonus;
        // Passive equipment has no evolution field or evolution state.
    }
}
