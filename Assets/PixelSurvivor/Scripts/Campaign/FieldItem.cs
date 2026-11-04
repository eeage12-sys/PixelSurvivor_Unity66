using UnityEngine;
namespace PixelSurvivor
{
    public sealed class FieldItem : MonoBehaviour
    {
        CampaignController c;FieldItemKind kind;int gold;bool final,collected,magnet;
        public void Initialize(CampaignController owner,FieldItemKind type,int amount,bool finalChest){c=owner;kind=type;gold=amount;final=finalChest;}
        public void Attract(){magnet=true;}
        void Update()
        {
            if(c==null || !c.Game.IsPlaying || collected)return;
            Vector2 target=c.Game.Player.transform.position;float d=Vector2.Distance(transform.position,target);
            if(d<0.7f){Collect();return;}
            if(magnet || d<1.5f*c.PickupFactor)transform.position=Vector2.MoveTowards(transform.position,target,10*Time.deltaTime);
        }
        public void Collect()
        {
            if(collected || c==null || !c.Game.IsPlaying)return;collected=true;
            c.Field.Items.Remove(this);gameObject.SetActive(false);
            switch(kind)
            {
                case FieldItemKind.Gold:c.AddGold(gold);break;
                case FieldItemKind.Magnet:c.Field.Magnet();break;
                case FieldItemKind.Bomb:c.Field.Bomb();break;
                case FieldItemKind.Heal:c.Game.Health.Heal(c.Data.healAmount);c.UI.Notify("체력 회복 +"+c.Data.healAmount);break;
                case FieldItemKind.Support:c.Supports.Summon();break;
                case FieldItemKind.EliteChest:c.OpenChest(false,false);break;
                case FieldItemKind.BossChest:c.OpenChest(true,final);break;
            }
            Destroy(gameObject);
        }
        void OnDisable(){if(c!=null)c.Field.Items.Remove(this);}
    }
}
