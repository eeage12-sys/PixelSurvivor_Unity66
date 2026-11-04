using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class WeaponZone : MonoBehaviour
    {
        CampaignController c;float radius,damage,duration,age,tick,knock;bool follow,ring;
        public void Initialize(CampaignController owner,float range,float power,float life,bool track,bool ringOnly,float force)
        {c=owner;radius=range;damage=power;duration=life;follow=track;ring=ringOnly;knock=force;transform.localScale=Vector3.one*range*2;}
        void Update()
        {
            if(c==null || !c.Game.IsPlaying)return;
            age+=Time.deltaTime;if(age>=duration){Destroy(gameObject);return;}
            if(follow)transform.position=c.Game.Player.transform.position;
            transform.Rotate(0,0,90*Time.deltaTime);
            tick-=Time.deltaTime;if(damage<=0 || tick>0)return;tick=0.4f;
            if(!ring){c.Arsenal.HurtCircle(transform.position,radius,damage,knock,Vector2.zero,-1);return;}
            foreach(var e in new List<Enemy>(c.Game.Spawner.ActiveEnemies))
            {
                if(e==null || !e.IsAlive)continue;Vector2 delta=e.transform.position-transform.position;
                if(Mathf.Abs(delta.magnitude-radius)<=0.6f+e.Radius){e.TakeDamage(c.RollDamage(damage));e.Knockback(delta,knock);}
            }
            foreach(var p in c.Field.Props.ToArray())if(p!=null && Mathf.Abs(Vector2.Distance(p.transform.position,transform.position)-radius)<1)p.TakeDamage(damage);
        }
    }
}
