using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public enum FieldItemKind { Gold, Magnet, Bomb, Heal, Support, EliteChest, BossChest }
    public sealed class FieldSystem
    {
        static readonly string[] ItemArt={"GoldCoin","Magnet","Bomb","HealPotion","SupportBeacon"};
        static readonly string[] StageArt={"Meadow","Amber","Ruin"};
        static readonly Vector2[][] OpeningOffsets={
            new[]{new Vector2(-5,-1),new Vector2(5,1),new Vector2(-2,3),new Vector2(3,-3),new Vector2(-7,2),new Vector2(7,-2)},
            new[]{new Vector2(-5,0),new Vector2(5,0),new Vector2(-3,3),new Vector2(3,-3),new Vector2(-6,-2),new Vector2(6,2)},
            new[]{new Vector2(-4,-1),new Vector2(4,1),new Vector2(-3,3),new Vector2(3,-3),new Vector2(-5,-3),new Vector2(5,3),new Vector2(0,-4)}
        };
        readonly Sprite[] itemSprites=new Sprite[ItemArt.Length];
        readonly Sprite[] chestSprites=new Sprite[StageArt.Length],supplySprites=new Sprite[StageArt.Length];
        readonly CampaignController c;
        public readonly List<DestructibleProp> Props=new List<DestructibleProp>();
        public readonly List<FieldItem> Items=new List<FieldItem>();
        struct PendingProp { public DestructibleProp prop; public float readyAt; }
        readonly Queue<PendingProp> pendingProps=new Queue<PendingProp>();
        float respawn;
        System.Random layout;
        public FieldSystem(CampaignController owner){c=owner;}
        public void Reset()
        {
            Props.Clear();Items.Clear();pendingProps.Clear();layout=new System.Random(c.StageData.layoutSeed);respawn=c.Data.propRespawnSeconds;
            int opening=0;
            foreach(var offset in OpeningOffsets[Mathf.Clamp(c.StageIndex,0,OpeningOffsets.Length-1)])
            {
                if(opening>=4 || opening>=c.StageData.propsCount)break;
                Vector2 position=(Vector2)c.Game.Player.transform.position+offset;
                if(!CanPlaceProp(position))continue;
                CreateProp(position);opening++;
            }
            for(int i=opening;i<c.StageData.propsCount;i++)CreateProp(i<opening+12);
        }
        public void Tick(float dt)
        {
            // Reuse at most four broken props per frame to avoid a burst of work.
            for(int i=0;i<4 && pendingProps.Count>0 && Props.Count<c.StageData.propsCount;i++)
            {
                var next=pendingProps.Peek();
                if(Time.time<next.readyAt || !TryHiddenPosition(out Vector2 position))break;
                pendingProps.Dequeue();
                if(next.prop==null)continue;
                next.prop.transform.position=position;
                next.prop.ResetForRespawn();
                next.prop.gameObject.SetActive(true);
                Props.Add(next.prop);
            }
            // Replace only objects lost for reasons other than normal breakage.
            respawn-=dt;if(respawn>0)return;respawn=Mathf.Max(1,c.Data.propRespawnSeconds);
            if(Props.Count+pendingProps.Count<c.StageData.propsCount && TryHiddenPosition(out Vector2 fallback))CreateProp(fallback);
        }
        void CreateProp(bool nearPlayer=false)
        {
            if(nearPlayer && TryHiddenPosition(out Vector2 nearby)){CreateProp(nearby);return;}
            Vector2 half=c.Game.Bounds.HalfSize-Vector2.one*2;
            Vector2 center=c.Game.Bounds.transform.position;
            for(int attempt=0;attempt<20;attempt++)
            {
                Vector2 pos=center+new Vector2((float)(layout.NextDouble()*2-1)*half.x,(float)(layout.NextDouble()*2-1)*half.y);
                if((pos-center).magnitude<3)pos+=Vector2.right*4;
                if(!CanPlaceProp(pos))continue;
                CreateProp(pos);return;
            }
            CreateProp(center+Vector2.up*7);
        }
        void CreateProp(Vector2 pos)
        {
            var go=new GameObject(StageArt[Mathf.Clamp(c.StageIndex,0,StageArt.Length-1)]+" breakable supply");
            go.transform.SetParent(c.Game.RuntimeRoot,false);go.transform.position=pos;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=StageSprite(supplySprites,"Supply")??c.Data.propSprite;
            sr.sortingOrder=2;sr.color=Color.white;
            var prop=go.AddComponent<DestructibleProp>();prop.Initialize(c);Props.Add(prop);
        }
        Sprite StageSprite(Sprite[] cache,string suffix)
        {
            int stage=Mathf.Clamp(c.StageIndex,0,StageArt.Length-1);
            if(cache[stage]==null)cache[stage]=Resources.Load<Sprite>("StageArt/"+StageArt[stage]+suffix);
            return cache[stage];
        }
        bool CanPlaceProp(Vector2 position)
        {
            if(((Vector2)c.Game.Player.transform.position-position).sqrMagnitude<9)return false;
            if(c.Terrain.IsBlocked(position,.65f) || c.Terrain.IsNearScenery(position,1.6f))return false;
            foreach(var prop in Props)
                if(prop!=null && ((Vector2)prop.transform.position-position).sqrMagnitude<2.25f)return false;
            return true;
        }
        bool TryHiddenPosition(out Vector2 position)
        {
            Camera view=c.Game.View;
            Vector2 camera=view.transform.position;
            float height=view.orthographicSize, width=height*view.aspect;
            for(int attempt=0;attempt<48;attempt++)
            {
                float distance=Random.Range(2f,10f);
                Vector2 sample;
                switch(Random.Range(0,4))
                {
                    case 0: sample=camera+new Vector2(-width-distance,Random.Range(-height-8,height+8));break;
                    case 1: sample=camera+new Vector2(width+distance,Random.Range(-height-8,height+8));break;
                    case 2: sample=camera+new Vector2(Random.Range(-width-8,width+8),-height-distance);break;
                    default: sample=camera+new Vector2(Random.Range(-width-8,width+8),height+distance);break;
                }
                sample=c.Game.Bounds.Clamp(sample,2);
                Vector3 viewport=view.WorldToViewportPoint(sample);
                if(viewport.x>-.1f && viewport.x<1.1f && viewport.y>-.1f && viewport.y<1.1f)continue;
                if(((Vector2)c.Game.Player.transform.position-sample).sqrMagnitude<25)continue;
                if(!CanPlaceProp(sample))continue;
                position=sample;return true;
            }
            position=default;return false;
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
            if(!chest && (int)kind<ItemArt.Length && itemSprites[(int)kind]==null)
                itemSprites[(int)kind]=Resources.Load<Sprite>("FieldIcons/"+ItemArt[(int)kind]);
            sr.sprite=chest?StageSprite(chestSprites,"Chest")??c.Data.chestSprite:itemSprites[(int)kind]!=null?itemSprites[(int)kind]:c.Data.itemSprite;
            sr.color=Color.white;
            go.transform.localScale=Vector3.one*(chest?(kind==FieldItemKind.BossChest?1.4f:1.2f):0.62f);
            var item=go.AddComponent<FieldItem>();item.Initialize(c,kind,gold,final);Items.Add(item);return item;
        }
        public void DropChest(Vector2 position,bool boss,bool final){Drop(position,boss?FieldItemKind.BossChest:FieldItemKind.EliteChest,0,final);}
        public void BreakProp(DestructibleProp prop,Vector2 position)
        {
            Props.Remove(prop);
            pendingProps.Enqueue(new PendingProp{prop=prop,readyAt=Time.time+Mathf.Max(0,c.Data.propRespawnSeconds)});
            float total=0;for(int i=0;i<5 && i<c.Data.dropWeights.Length;i++)total+=Mathf.Max(0,c.Data.dropWeights[i]);
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
