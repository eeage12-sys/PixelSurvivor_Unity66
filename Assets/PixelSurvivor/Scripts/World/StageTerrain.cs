using System.Collections.Generic;
using UnityEngine;

namespace PixelSurvivor
{
    // Deterministic landmarks and hazards for the later stages.
    public sealed class StageTerrain
    {
        readonly CampaignController c;
        readonly List<Rect> obstacles=new List<Rect>();
        readonly List<Rect> slowGround=new List<Rect>();
        readonly List<Vector2> cursedGround=new List<Vector2>();
        readonly List<SpriteRenderer> curseMarks=new List<SpriteRenderer>();
        float curseClock;

        public int ObstacleCount => obstacles.Count;
        public int HazardCount => slowGround.Count+cursedGround.Count;
        public StageTerrain(CampaignController owner){c=owner;}

        public void Build(Transform map)
        {
            obstacles.Clear();slowGround.Clear();cursedGround.Clear();curseMarks.Clear();curseClock=1.2f;
            var previous=map.Find("Stage terrain");
            if(previous!=null){previous.gameObject.SetActive(false);Object.Destroy(previous.gameObject);}
            if(c.StageIndex==0)return;
            var root=new GameObject("Stage terrain").transform;
            root.SetParent(map,false);
            if(c.StageIndex==1)
            {
                for(int i=-4;i<=4;i++)
                    AddObstacle(root,new Vector2(i%2==0?-13:13,i*11),new Vector2(3.2f,3.2f),new Color(.32f,.22f,.12f));
                AddObstacle(root,new Vector2(-35,18),new Vector2(4,3),new Color(.32f,.22f,.12f));
                AddObstacle(root,new Vector2(35,-18),new Vector2(4,3),new Color(.32f,.22f,.12f));
                AddSlow(root,new Vector2(-5,12),new Vector2(13,4));
                AddSlow(root,new Vector2(14,-15),new Vector2(13,4));
                AddSlow(root,new Vector2(-22,-27),new Vector2(11,4));
            }
            else if(c.StageIndex==2)
            {
                for(int side=-1;side<=1;side+=2)
                    for(int i=-5;i<=5;i++)
                        AddObstacle(root,new Vector2(side*(8+Mathf.Abs(i)%3*2),i*13),
                            new Vector2(2.6f,4.2f),new Color(.27f,.18f,.38f));
                AddCurse(root,new Vector2(-6,-20));
                AddCurse(root,new Vector2(6,20));
                AddCurse(root,new Vector2(-20,45));
                AddCurse(root,new Vector2(20,-45));
            }
        }

        void AddObstacle(Transform root,Vector2 local,Vector2 size,Color color)
        {
            Vector2 center=(Vector2)root.position+local;
            obstacles.Add(new Rect(center-size*.5f,size));
            var go=new GameObject("Solid terrain");go.transform.SetParent(root,false);go.transform.localPosition=local;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.whiteSprite;
            sr.drawMode=SpriteDrawMode.Tiled;sr.size=size;sr.color=color;sr.sortingOrder=-4;
            var top=new GameObject("Stone rim");top.transform.SetParent(go.transform,false);
            top.transform.localPosition=new Vector2(0,size.y*.4f);
            var edge=top.AddComponent<SpriteRenderer>();edge.sprite=c.Data.whiteSprite;
            edge.drawMode=SpriteDrawMode.Tiled;edge.size=new Vector2(size.x,.24f);
            edge.color=Color.Lerp(color,Color.white,.32f);edge.sortingOrder=-3;
        }

        void AddSlow(Transform root,Vector2 local,Vector2 size)
        {
            slowGround.Add(new Rect((Vector2)root.position+local-size*.5f,size));
            var go=new GameObject("Basin mire");go.transform.SetParent(root,false);go.transform.localPosition=local;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.whiteSprite;
            sr.drawMode=SpriteDrawMode.Tiled;sr.size=size;
            sr.color=new Color(.52f,.28f,.08f,.58f);sr.sortingOrder=-8;
        }

        void AddCurse(Transform root,Vector2 local)
        {
            cursedGround.Add((Vector2)root.position+local);
            var go=new GameObject("Cursed ground");go.transform.SetParent(root,false);go.transform.localPosition=local;
            go.transform.localScale=Vector3.one*5.5f;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=c.Data.ringSprite;
            sr.color=new Color(.8f,.35f,1f,.65f);sr.sortingOrder=-7;
            curseMarks.Add(sr);
        }

        public bool IsBlocked(Vector2 position,float radius)
        {
            foreach(var rect in obstacles)
                if(position.x>rect.xMin-radius && position.x<rect.xMax+radius &&
                   position.y>rect.yMin-radius && position.y<rect.yMax+radius)return true;
            return false;
        }

        public Vector2 ResolveMove(Vector2 from,Vector2 to,float radius)
        {
            Vector2 result=from;
            if(!IsBlocked(new Vector2(to.x,from.y),radius))result.x=to.x;
            if(!IsBlocked(new Vector2(result.x,to.y),radius))result.y=to.y;
            return result;
        }

        public float PlayerSpeedFactor(Vector2 position)
        {
            foreach(var area in slowGround)if(area.Contains(position))return .68f;
            return 1f;
        }

        public void Tick(float dt)
        {
            if(cursedGround.Count==0 || !c.Game.IsPlaying)return;
            float pulse=.45f+.2f*Mathf.Sin(Time.time*4);
            foreach(var mark in curseMarks)if(mark!=null)
                mark.color=new Color(.8f,.35f,1f,pulse);
            curseClock-=dt;
            if(curseClock>0)return;
            curseClock=1.2f;
            Vector2 player=c.Game.Player.transform.position;
            foreach(var center in cursedGround)
                if((center-player).sqrMagnitude<2.75f*2.75f)
                {c.Game.Health.TakeDamage(8*c.StageData.difficulty);break;}
        }
    }
}
