using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public enum FieldItemKind { Gold, Magnet, Bomb, Heal, Support, EliteChest, BossChest }
    public sealed class FieldSystem
    {
        readonly CampaignController c;
        public readonly List<DestructibleProp> Props=new List<DestructibleProp>();
        public readonly List<FieldItem> Items=new List<FieldItem>();
        float respawn;
        System.Random layout;
        public FieldSystem(CampaignController owner){c=owner;}
        public void Reset()
        {
            Props.Clear();Items.Clear();layout=new System.Random(c.StageData.layoutSeed);respawn=c.Data.propRespawnSeconds;
            for(int i=0;i<c.StageData.propsCount;i++)CreateProp();
        }
        public void Tick(float dt)
        {
            respawn-=dt;if(respawn>0)return;respawn=Mathf.Max(1,c.Data.propRespawnSeconds);
            if(Props.Count<c.StageData.propsCount)CreateProp();
        }
        void CreateProp()
        {
            Vector2 half=c.Game.Bounds.HalfSize-Vector2.one*2;
            Vector2 pos=new Vector2((float)(layout.NextDouble()*2-1)*half.x,(float)(layout.NextDouble()*2-1)*half.y);
            if(pos.magnitude<3)pos+=Vector2.right*4;
            var go=new GameObject("Breakable supply");go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=pos;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.propSprite;sr.sortingOrder=2;sr.color=c.StageData.detailColor;
            var prop=go.AddComponent<DestructibleProp>();prop.Initialize(c);Props.Add(prop);
        }
        public DestructibleProp NearestProp(Vector2 origin,float range)
        {
            float best=range*range;DestructibleProp result=null;
            foreach(var p in Props)if(p!=null){float d=((Vector2)p.transform.position-origin).sqrMagnitude;if(d<best){result=p;best=d;}}
            return result;
        }
        public FieldItem Drop(Vector2 position,FieldItemKind kind,int gold=0,bool final=false)
        {
            var go=new GameObject(kind.ToString());go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=position;
            var sr=go.AddComponent<SpriteRenderer>();sr.sortingOrder=3;
            bool chest=kind==FieldItemKind.EliteChest || kind==FieldItemKind.BossChest;
            sr.sprite=chest?c.Data.chestSprite:c.Data.itemSprite;
            Color[] colors={new Color(1,0.85f,0.25f),Color.cyan,new Color(1,0.3f,0.3f),Color.green,new Color(0.8f,0.6f,1),new Color(0.5f,1,0.7f),new Color(1,0.65f,0.2f)};
            sr.color=colors[(int)kind];go.transform.localScale=Vector3.one*(chest?1.3f:0.7f);
            var item=go.AddComponent<FieldItem>();item.Initialize(c,kind,gold,final);Items.Add(item);return item;
        }
        public void DropChest(Vector2 position,bool boss,bool final){Drop(position,boss?FieldItemKind.BossChest:FieldItemKind.EliteChest,0,final);}
        public void BreakProp(DestructibleProp prop,Vector2 position)
        {
            Props.Remove(prop);float total=0;for(int i=0;i<5 && i<c.Data.dropWeights.Length;i++)total+=Mathf.Max(0,c.Data.dropWeights[i]);
            float roll=Random.value*total;int index=0;
            for(int i=0;i<5 && i<c.Data.dropWeights.Length;i++){roll-=Mathf.Max(0,c.Data.dropWeights[i]);if(roll<=0){index=i;break;}}
            Drop(position,(FieldItemKind)index,c.Data.fieldGold);
        }
        public void Magnet()
        {
            foreach(var gem in Object.FindObjectsByType<ExperienceGem>(FindObjectsSortMode.None))gem.Attract();
            foreach(var item in Items.ToArray())if(item!=null)item.Attract();
            c.UI.Notify("자석 · 필드의 아이템을 끌어옵니다");
        }
        public void Bomb()
        {
            foreach(var enemy in new List<Enemy>(c.Game.Spawner.ActiveEnemies))
                if(enemy!=null && enemy.IsAlive)enemy.TakeDamage(enemy.IsBoss || enemy.IsElite ? c.Data.bombSpecialDamage : enemy.CurrentHP+1);
            c.Arsenal.Zone(c.Game.Player.transform.position,12,0,0.5f,new Color(1,0.4f,0.2f),false,false,0);
            c.UI.Notify("폭탄 · 일반 몬스터를 처치했습니다");
        }
    }
}
