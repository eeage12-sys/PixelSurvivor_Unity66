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
        readonly List<Vector2> sceneryPositions=new List<Vector2>();

        public int ObstacleCount => obstacles.Count;
        public int HazardCount => slowGround.Count;
        public StageTerrain(CampaignController owner){c=owner;}

        public void Build(Transform map)
        {
            obstacles.Clear();slowGround.Clear();sceneryPositions.Clear();
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
                PlaceScenery(root,"AmberTree",new Vector2(-8,2.5f),2.5f,false);
                PlaceScenery(root,"AmberTree",new Vector2(8,-2.5f),2.5f,false);
                ScatterScenery(root,"AmberTree",70,12,105,64,2.5f,false);
            }
            else if(c.StageIndex==2)
            {
                for(int side=-1;side<=1;side+=2)
                    for(int i=-5;i<=5;i++)
                        PlaceScenery(root,"RuinStone",new Vector2(side*(8+Mathf.Abs(i)%3*2),i*13),3f,true);
                PlaceScenery(root,"RuinTree",new Vector2(-5,2.5f),2.5f,false);
                PlaceScenery(root,"RuinTree",new Vector2(5,-2.5f),2.5f,false);
                PlaceScenery(root,"RuinStone",new Vector2(2.5f,2.5f),2.3f,true);
                ScatterScenery(root,"RuinTree",55,13,65,87,2.5f,false);
                ScatterScenery(root,"RuinStone",24,25,67,89,2.3f,true);
            }
        }

        void ScatterScenery(Transform root,string spriteName,int count,float minX,float maxX,float maxY,float width,bool blocking)
        {
            var random=new System.Random(c.StageData.layoutSeed+spriteName.Length*101+(blocking?7:0));
            for(int i=0;i<count;i++)
            {
                float side=i%2==0?-1:1;
                Vector2 local=new Vector2(side*Mathf.Lerp(minX,maxX,(float)random.NextDouble()),
                    Mathf.Lerp(-maxY,maxY,(float)random.NextDouble()));
                PlaceScenery(root,spriteName,local,width,blocking);
            }
        }
        void PlaceScenery(Transform root,string spriteName,Vector2 local,float width,bool blocking)
        {
            var sprite=Resources.Load<Sprite>("StageArt/"+spriteName);
            if(sprite==null)return;
            Vector2 center=(Vector2)root.position+local;
            if(IsBlocked(center,width*.5f))return;
            if(blocking)obstacles.Add(new Rect(center-Vector2.one*width*.5f,Vector2.one*width));
            sceneryPositions.Add(center);
            var go=new GameObject(spriteName+(blocking?" obstacle":" scenery"));
            go.transform.SetParent(root,false);go.transform.localPosition=local;
            go.transform.localScale=Vector3.one*(width/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y));
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=-2;
        }

        void AddObstacle(Transform root,Vector2 local,Vector2 size,Color color)
        {
            Vector2 center=(Vector2)root.position+local;
            obstacles.Add(new Rect(center-size*.5f,size));
            sceneryPositions.Add(center);
            var go=new GameObject("Solid terrain");go.transform.SetParent(root,false);go.transform.localPosition=local;
            var sprite=Resources.Load<Sprite>("StageArt/AmberBoulder");
            if(sprite!=null)
            {
                var image=go.AddComponent<SpriteRenderer>();image.sprite=sprite;image.sortingOrder=-2;
                go.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
                return;
            }
            AddWallLayer(go.transform,"Stone face",Vector2.zero,size,color,-4,c.Data.whiteSprite);
            var top=new GameObject("Stone rim");top.transform.SetParent(go.transform,false);
            top.transform.localPosition=new Vector2(0,size.y*.4f);
            AddWallLayer(top.transform,"Stone highlight",Vector2.zero,new Vector2(size.x,.24f),
                Color.Lerp(color,Color.white,.32f),-3,c.Data.whiteSprite);
        }

        static void AddWallLayer(Transform parent,string name,Vector2 offset,Vector2 size,Color color,int order,Sprite sprite)
        {
            var part=new GameObject(name);part.transform.SetParent(parent,false);part.transform.localPosition=offset;
            var image=part.AddComponent<SpriteRenderer>();image.sprite=sprite;
            image.drawMode=SpriteDrawMode.Simple;
            if(sprite!=null && sprite.bounds.size.x>0 && sprite.bounds.size.y>0)
                part.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
            image.color=color;image.sortingOrder=order;
        }

        void AddSlow(Transform root,Vector2 local,Vector2 size)
        {
            slowGround.Add(new Rect((Vector2)root.position+local-size*.5f,size));
            var go=new GameObject("Basin mire");go.transform.SetParent(root,false);go.transform.localPosition=local;
            AddWallLayer(go.transform,"Mire color",Vector2.zero,size,
                new Color(.52f,.28f,.08f,.58f),-8,c.Data.whiteSprite);
        }

        public bool IsBlocked(Vector2 position,float radius)
        {
            foreach(var rect in obstacles)
                if(position.x>rect.xMin-radius && position.x<rect.xMax+radius &&
                   position.y>rect.yMin-radius && position.y<rect.yMax+radius)return true;
            return false;
        }

        public bool IsNearScenery(Vector2 position,float radius)
        {
            float distance=radius*radius;
            foreach(var center in sceneryPositions)
                if((center-position).sqrMagnitude<distance)return true;
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

        public void Tick(float dt) { }
    }
}
