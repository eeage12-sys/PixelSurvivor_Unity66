using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class SupportUnitManager
    {
        sealed class Unit {public int kind;public float time,tick;public GameObject visual;public Vector2 center;public bool vertical;}
        readonly CampaignController c;readonly List<Unit> units=new List<Unit>();
        public SupportUnitManager(CampaignController owner){c=owner;}
        public int Count => units.Count;
        public void Reset(){foreach(var u in units)if(u.visual!=null)Object.Destroy(u.visual);units.Clear();}
        public void Summon(){Summon(Random.Range(0,3));}
        public void Summon(int kind)
        {
            kind=Mathf.Clamp(kind,0,2);
            // One of each type; repeated pickups refresh its duration rather than consuming slots.
            var existing=units.Find(x=>x.kind==kind);
            if(existing!=null){existing.time=c.Data.supportDuration;c.UI.Notify("지원군 지속시간이 갱신되었습니다");return;}
            var go=new GameObject("Support "+kind);go.transform.SetParent(c.Game.RuntimeRoot,false);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=kind==2?c.Data.whiteSprite:c.Data.supportSprite;sr.sortingOrder=8;
            sr.color=kind==0?Color.cyan:kind==1?Color.green:new Color(0.4f,0.7f,1,0.8f);
            Vector2 forward=c.Arsenal.Facing;
            var u=new Unit{kind=kind,time=c.Data.supportDuration,visual=go,center=(Vector2)c.Game.Player.transform.position+forward*2.5f,vertical=Mathf.Abs(forward.x)>0.5f};
            if(kind==2){go.transform.position=u.center;go.transform.localScale=u.vertical?new Vector3(16,c.Data.barrierWidth*32,1):new Vector3(c.Data.barrierWidth*32,16,1);}
            units.Add(u);c.UI.Notify(kind==0?"공격 지원군 등장":kind==1?"회복 지원군 등장":"방어 지원군 · 적의 이동을 막는 장벽 설치");
        }
        public void Tick(float dt)
        {
            for(int i=units.Count-1;i>=0;i--)
            {
                var u=units[i];u.time-=dt;if(u.time<=0){Object.Destroy(u.visual);units.RemoveAt(i);continue;}
                if(u.kind==2)continue;
                u.visual.transform.position=c.Game.Player.transform.position+new Vector3(u.kind==0?1.2f:-1.2f,0.7f,0);
                u.tick-=dt;if(u.tick>0)continue;
                if(u.kind==1){u.tick=c.Data.supportHealInterval;c.Game.Health.Heal(c.Data.supportHeal);}
                else {u.tick=c.Data.supportAttackInterval;var e=c.Arsenal.Nearest(u.visual.transform.position);
                    if(e!=null)c.Arsenal.Shoot(u.visual.transform.position,e.transform.position-u.visual.transform.position,c.Data.supportDamage,12,3,0,1,Color.cyan,WeaponPattern.Seeker,0);}
            }
        }
        public Vector2 ResolveMove(Vector2 from,Vector2 to,float radius)
        {
            foreach(var u in units)
            {
                if(u.kind!=2 || u.time<=0)continue;
                // Expanded axis-aligned wall, swept along its thin axis. Sliding chooses an end,
                // so chasing enemies route around it instead of getting permanently stuck.
                float fx=u.vertical?from.x:from.y, fy=u.vertical?from.y:from.x;
                float tx=u.vertical?to.x:to.y, ty=u.vertical?to.y:to.x;
                float cx=u.vertical?u.center.x:u.center.y,cy=u.vertical?u.center.y:u.center.x;
                float half=0.25f+radius+0.02f,length=c.Data.barrierWidth*0.5f+radius+0.02f;
                float side=fx<cx?-1:1,face=cx+side*half;
                bool inside=Mathf.Abs(fx-cx)<half && Mathf.Abs(fy-cy)<length;
                bool crossing=(fx-face)*(tx-face)<=0 && (tx-fx)*side<0 && Mathf.Abs(tx-fx)>0.000001f;
                float alpha=crossing?Mathf.Clamp01((face-fx)/(tx-fx)):0;
                float crossY=Mathf.Lerp(fy,ty,alpha);
                if(inside || (crossing && Mathf.Abs(crossY-cy)<length))
                {
                    float route=fy>=cy?1:-1;
                    float step=Vector2.Distance(from,to);
                    tx=face;ty=fy+route*step;
                    to=u.vertical?new Vector2(tx,ty):new Vector2(ty,tx);
                }
            }
            return to;
        }
    }
}
