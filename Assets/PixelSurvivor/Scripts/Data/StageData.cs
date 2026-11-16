using UnityEngine;
namespace PixelSurvivor
{
    [CreateAssetMenu(menuName="Pixel Survivor/Stage")]
    public sealed class StageData : ScriptableObject
    {
        public string id, title;
        [TextArea] public string description;
        public Vector2 halfSize=new Vector2(96,64);
        public Color groundColor=Color.white, detailColor=Color.white;
        public int layoutSeed=41;
        [UnityEngine.Serialization.FormerlySerializedAs("duration")]
        [Min(1)] public float unlockBossTime=1200;
        [Min(1)] public float bossInterval=600;
        [Header("Infinite scaling")]
        [Min(1)] public float minimumEliteInterval=20;
        [Min(0)] public float eliteRateGrowthPerMinute=0.05f;
        [Min(0)] public float bossHealthGrowthPerWave=0.75f, bossDamageGrowthPerWave=0.25f, bossHastePerWave=0.12f;
        [Min(0.2f)] public float bossAttackInterval=5, minimumBossAttackInterval=1.2f;
        [Min(0)] public int bossBaseProjectiles=4, bossExtraProjectilesPerWave=2;
        [Min(1)] public int bossMaxProjectiles=24;
        [Min(0.1f)] public float bossProjectileSpeed=4, bossProjectileLife=6;
        [Min(1)] public float bossAttackDamage=20;
        [Min(0)] public float bossAreaGrowthPerWave=0.1f;
        [Min(1)] public float firstEliteTime=90, eliteInterval=90;
        [Min(0.1f)] public float difficulty=1, healthGrowthPerMinute=0.12f, damageGrowthPerMinute=0.04f, speedGrowthPerMinute=0.01f;
        [Min(0.1f)] public float initialSpawnInterval=1.5f, minimumSpawnInterval=0.3f;
        [Min(1)] public int initialSpawnCount=1, maxSpawnCount=4, propsCount=28;
        [Min(1)] public float extraSpawnEvery=300; // Legacy serialized field retained for compatibility.
        [Header("Minute reinforcements")]
        [Min(1)] public int spawnCountPerMinute=1, maximumMinuteSpawnCount=24;
        [Min(0)] public float mothUnlock=30, beetleUnlock=90;
        [Range(0,1)] public float mothWeight=0.3f, beetleWeight=0.2f;
        [Min(0)] public float boneCrawlerUnlock=150, emberWraithUnlock=240;
        [Range(0,1)] public float boneCrawlerWeight=0.15f, emberWraithWeight=0.12f;
        [Min(1)] public float eliteHP=6, eliteDamage=1.6f, eliteSpeed=1.1f;
        [Min(1)] public float bossHP=1200;
        [UnityEngine.Serialization.FormerlySerializedAs("finalBossHP")]
        [Min(1)] public float unlockBossHP=4500;
        
    }
}
