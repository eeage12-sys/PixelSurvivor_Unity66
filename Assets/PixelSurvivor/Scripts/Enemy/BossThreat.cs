using UnityEngine;
namespace PixelSurvivor
{
    public sealed class BossThreat : MonoBehaviour
    {
        CampaignController c;Enemy owner;float radius,delay,damage,age;bool hit;
        public void Initialize(CampaignController campaign,Enemy source,float range,float wait,float power){c=campaign;owner=source;radius=range;delay=wait;damage=power;}
        void Update()
        {
            if(c==null || !c.Game.IsPlaying)return;
            if(owner==null || !owner.IsAlive){Destroy(gameObject);return;}
            age+=Time.deltaTime;if(age<delay)return;
            if(!hit){hit=true;GetComponent<SpriteRenderer>().color=Color.red;
                if(Vector2.Distance(transform.position,c.Game.Player.transform.position)<=radius+c.Game.Player.Radius)c.Game.Health.TakeDamage(damage);}
            if(age>delay+0.3f)Destroy(gameObject);
        }
    }
}
