using UnityEngine;
namespace PixelSurvivor
{
    public sealed class DestructibleProp : MonoBehaviour
    {
        CampaignController c;float hp;bool broken;
        public void Initialize(CampaignController owner){c=owner;hp=c.Data.propHP;}
        public void TakeDamage(float amount)
        {
            if(c==null || !c.Game.IsPlaying || broken)return;hp-=Mathf.Max(0,amount);if(hp>0)return;
            broken=true;c.Field.BreakProp(this,transform.position);gameObject.SetActive(false);Destroy(gameObject);
        }
        void OnDisable(){if(c!=null)c.Field.Props.Remove(this);}
    }
}
