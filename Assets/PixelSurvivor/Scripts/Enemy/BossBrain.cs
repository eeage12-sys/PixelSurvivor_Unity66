using UnityEngine;
namespace PixelSurvivor
{
    public sealed class BossBrain : MonoBehaviour
    {
        CampaignController c;Enemy enemy;float timer=3;int wave,attack;
        public int Wave => wave;
        public void Initialize(CampaignController owner,Enemy source,int number){c=owner;enemy=source;wave=number;}
        void Update()
        {
            if(c==null || !c.Game.IsPlaying || !enemy.IsAlive)return;
            timer-=Time.deltaTime;if(timer>0)return;
            var d=c.StageData;
            timer=Mathf.Max(d.minimumBossAttackInterval,d.bossAttackInterval/SurvivalRules.Growth(wave,d.bossHastePerWave));attack++;
            float damage=d.bossAttackDamage*d.difficulty*SurvivalRules.Growth(wave,d.bossDamageGrowthPerWave);
            float area=Mathf.Min(4,SurvivalRules.Growth(wave,d.bossAreaGrowthPerWave));
            Vector2 target=c.Game.Player.transform.position;
            SpawnThreat(target,1.8f*area,1.1f,damage);
            int count=Mathf.Clamp(d.bossBaseProjectiles+(wave-1)*d.bossExtraProjectilesPerWave,0,d.bossMaxProjectiles);
            for(int i=0;i<count;i++)
            {
                float a=i*Mathf.PI*2/Mathf.Max(1,count)+attack*0.3f;
                Vector2 direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                if(wave>=2 && i%2==0)SpawnThreat(target+direction*4,1.3f*area,1.5f,damage);
                var go=new GameObject("Boss projectile");go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=enemy.transform.position;
                var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.projectileSprite;sr.color=new Color(1,0.3f,0.2f);sr.sortingOrder=15;
                go.AddComponent<BossProjectile>().Initialize(c,enemy,direction,d.bossProjectileSpeed,damage,d.bossProjectileLife);
            }
        }
        void SpawnThreat(Vector2 position,float radius,float delay,float damage)
        {
            var go=new GameObject("Boss warning");go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=position;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.ringSprite;sr.color=new Color(1,0.25f,0.2f,0.5f);sr.sortingOrder=1;
            go.transform.localScale=Vector3.one*radius*2;go.AddComponent<BossThreat>().Initialize(c,enemy,radius,delay,damage);
        }
    }
}
