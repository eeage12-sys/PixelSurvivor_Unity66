using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    public sealed class TreasureChestSystem
    {
        readonly CampaignController campaign;
        public TreasureChestSystem(CampaignController owner) {campaign=owner;}
        public static int RollCount(float sample,float one,float three,float five)
        {
            one=Mathf.Max(0,one);three=Mathf.Max(0,three);five=Mathf.Max(0,five);
            float total=one+three+five; if(total<=0) return 1;
            float roll=Mathf.Clamp(sample,0,0.999999f)*total;
            return roll<one ? 1 : roll<one+three ? 3 : 5;
        }
        public List<string> Open(bool boss)
        {
            var d=campaign.Data;
            int rolls=boss ? 3 : RollCount(Random.value,d.singleWeight,d.tripleWeight,d.fiveWeight);
            return Resolve(rolls);
        }
        public List<string> Resolve(int rolls)
        {
            var result=new List<string>();var e=campaign.Equipment;
            for(int i=0;i<rolls;i++)
            {
                // Recompute candidates every roll: a MAX gained in this jackpot may evolve on the next roll.
                var ready=new List<WeaponState>();
                foreach(var w in e.Weapons) if(e.CanEvolve(w)) ready.Add(w);
                if(ready.Count>0)
                {
                    var w=ready[Random.Range(0,ready.Count)];e.Evolve(w);campaign.DiscoverEvolution(w.data);
                    result.Add("진화: "+w.data.title+" > "+w.Name);continue;
                }
                var bag=new List<EquipmentChoice>();
                foreach(var w in e.Weapons) if(!w.evolved && w.level<w.Maximum) bag.Add(new EquipmentChoice{kind=EquipmentChoiceKind.Weapon,weapon=w.data});
                foreach(var p in e.Passives) if(p.level<p.data.maxLevel) bag.Add(new EquipmentChoice{kind=EquipmentChoiceKind.Passive,passive=p.data});
                if(bag.Count==0)
                { campaign.AddGold(campaign.Data.overflowGold);result.Add("강화 대상 없음: 골드 +"+campaign.LastGoldAward);continue; }
                var choice=bag[Random.Range(0,bag.Count)];
                if(choice.weapon!=null) {var w=e.Find(choice.weapon);int before=w.level;e.Add(choice.weapon);result.Add(w.Name+" "+before+" > "+w.level);}
                else {var p=e.Find(choice.passive);int before=p.level;e.Add(choice.passive);result.Add(p.data.title+" "+before+" > "+p.level);}
            }
            campaign.RefreshStats(true); return result;
        }
    }
}
