using UnityEngine;
namespace PixelSurvivor
{
    [CreateAssetMenu(menuName="Pixel Survivor/Campaign")]
    public sealed class CampaignData : ScriptableObject
    {
        public WeaponData[] weapons;
        public PassiveEquipmentData[] passives;
        public CharacterData[] characters;
        public StageData[] stages;
        public PermanentUpgrade[] commonUpgrades;
        public Sprite ringSprite, whiteSprite, projectileSprite, chestSprite, propSprite, itemSprite, supportSprite;
        [Header("Elite chest weights: 1 / 3 / 5 rolls")]
        [Min(0)] public float singleWeight=85, tripleWeight=12, fiveWeight=3;
        [Min(1)] public int overflowGold=25;
        [Header("Field drop weights: Gold / Magnet / Bomb / Heal / Support")]
        public float[] dropWeights={40,15,10,25,10};
        [Min(1)] public int fieldGold=15, monsterGold=3;
        [Range(0,1)] public float monsterGoldChance=0.12f;
        [Min(0)] public float healAmount=30, bombSpecialDamage=300;
        [Min(1)] public float propHP=20, propRespawnSeconds=35;
        [Header("Support")]
        [Min(1)] public float supportDuration=15;
        [Min(0.1f)] public float supportAttackInterval=0.25f, supportHealInterval=2;
        public float supportDamage=15, supportHeal=8, barrierWidth=4;
        static bool FramesValid(Sprite[] frames) => frames!=null && frames.Length==4 && System.Array.TrueForAll(frames,x=>x!=null);
        public bool IsConfigured
        {
            get
            {
                if(weapons==null || weapons.Length<8 || passives==null || passives.Length<8 || characters==null || characters.Length!=5 || stages==null || stages.Length<3 || commonUpgrades==null || dropWeights==null || dropWeights.Length<5)return false;
                if(whiteSprite==null || ringSprite==null || projectileSprite==null || propSprite==null || chestSprite==null || itemSprite==null || supportSprite==null)return false;
                foreach(var w in weapons)if(w==null || w.maxLevel<1 || w.evolutionPartner==null)return false;
                foreach(var p in passives)if(p==null || p.maxLevel<1)return false;
                foreach(var ch in characters)if(ch==null || ch.sprite==null || ch.portrait==null || !FramesValid(ch.downFrames) || !FramesValid(ch.upFrames) || !FramesValid(ch.leftFrames) || !FramesValid(ch.rightFrames) || ch.startingWeapon==null || ch.upgrades==null || ch.inherentBonuses==null)return false;
                foreach(var stage in stages)if(stage==null || stage.unlockBossTime<=0 || stage.bossInterval<=0)return false;
                return true;
            }
        }
    }
}
