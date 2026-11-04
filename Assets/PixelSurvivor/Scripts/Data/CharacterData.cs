using UnityEngine;
namespace PixelSurvivor
{
    [System.Serializable] public class PermanentUpgrade
    {
        public string id, title, role;
        public Sprite sprite, portrait;
        public Sprite[] downFrames, upFrames, leftFrames, rightFrames;
        public StatBonus bonus;
        [Min(1)] public int maxLevel=10, baseCost=40;
        [Min(1)] public float costGrowth=1.4f;
        public int Cost(int level) => Mathf.CeilToInt(baseCost * Mathf.Pow(costGrowth, level));
    }
    [CreateAssetMenu(menuName="Pixel Survivor/Character")]
    public sealed class CharacterData : ScriptableObject
    {
        public string id, title, role;
        public Sprite sprite, portrait;
        public Sprite[] downFrames, upFrames, leftFrames, rightFrames;
        [TextArea] public string description, passiveDescription;
        public WeaponData startingWeapon;
        public Color tint=Color.white;
        [Min(1)] public float maxHP=100;
        [Min(0.1f)] public float moveSpeed=5, damage=10, attackInterval=0.8f;
        public StatBonus[] inherentBonuses = new StatBonus[0];
        public PermanentUpgrade[] upgrades = new PermanentUpgrade[0];
    }
}
