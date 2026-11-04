using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    [System.Serializable] public sealed class WeaponState
    {
        public WeaponData data; public int level=1; public bool evolved;
        public int Maximum => Mathf.Max(1,data.maxLevel);
        public string Name => evolved ? data.evolvedTitle : data.title;
    }
    [System.Serializable] public sealed class PassiveState { public PassiveEquipmentData data; public int level=1; }
    public enum EquipmentChoiceKind { Weapon, Passive }
    public sealed class EquipmentChoice
    {
        public EquipmentChoiceKind kind; public WeaponData weapon; public PassiveEquipmentData passive;
    }
    public sealed class EquipmentManager
    {
        public const int WeaponSlots=7, PassiveSlots=7;
        readonly List<WeaponState> weapons=new List<WeaponState>();
        readonly List<PassiveState> passives=new List<PassiveState>();
        public IReadOnlyList<WeaponState> Weapons => weapons;
        public IReadOnlyList<PassiveState> Passives => passives;
        public WeaponState Find(WeaponData data) => weapons.Find(x=>x.data==data);
        public PassiveState Find(PassiveEquipmentData data) => passives.Find(x=>x.data==data);
        public void Reset(WeaponData starter) { weapons.Clear(); passives.Clear(); if(starter!=null) Add(starter); }
        public bool Add(WeaponData data)
        {
            if(data==null) return false;
            var owned=Find(data);
            if(owned!=null) { if(owned.evolved || owned.level>=owned.Maximum) return false; owned.level++; return true; }
            if(weapons.Count>=WeaponSlots) return false;
            weapons.Add(new WeaponState { data=data }); return true;
        }
        public bool Add(PassiveEquipmentData data)
        {
            if(data==null) return false;
            var owned=Find(data);
            if(owned!=null) { if(owned.level>=Mathf.Max(1,data.maxLevel)) return false; owned.level++; return true; }
            if(passives.Count>=PassiveSlots) return false;
            passives.Add(new PassiveState { data=data }); return true;
        }
        public bool CanEvolve(WeaponState weapon) => weapon!=null && !weapon.evolved && weapon.level>=weapon.Maximum &&
            weapon.data.evolutionPartner!=null && Find(weapon.data.evolutionPartner)!=null;
        public bool Evolve(WeaponState weapon)
        { if(!CanEvolve(weapon)) return false; weapon.evolved=true; return true; }
        public float Bonus(StatKind kind)
        { float sum=0; foreach(var p in passives) if(p.data.bonus.kind==kind) sum+=p.data.bonus.amount*p.level; return sum; }
        public string EvolutionStatus(WeaponData data)
        {
            var w=Find(data); bool max=w!=null && w.level>=w.Maximum;
            bool partner=data.evolutionPartner!=null && Find(data.evolutionPartner)!=null;
            if(w!=null && w.evolved) return "진화 완료";
            if(max && partner) return "진화 준비 완료 · 상자를 획득하세요";
            return (max ? "[충족] 무기 최대 레벨" : "[미충족] 무기 최대 레벨 필요")+"\n"+
                (partner ? "[보유] " : "[미보유] ")+(data.evolutionPartner!=null ? data.evolutionPartner.title : "조합 장비 미설정")+(partner ? " 보유" : " 필요");
        }
        public string Synergy(PassiveEquipmentData passive)
        {
            string text="";
            foreach(var w in weapons) if(w.data.evolutionPartner==passive)
                text+=(text.Length>0 ? "\n" : "")+w.data.title+": "+(w.evolved ? "진화 완료" : "진화 조합 · 무기 최대 레벨 필요");
            return text.Length==0 ? "조합할 공격형 장비를 아직 보유하지 않았습니다." : text;
        }
        public EquipmentChoice[] RollChoices(CampaignData data)
        {
            var bag=new List<EquipmentChoice>();
            foreach(var w in data.weapons)
            {
                if(w==null) continue; var state=Find(w);
                if(state==null ? weapons.Count<WeaponSlots : !state.evolved && state.level<state.Maximum)
                    bag.Add(new EquipmentChoice {kind=EquipmentChoiceKind.Weapon,weapon=w});
            }
            foreach(var p in data.passives)
            {
                if(p==null) continue; var state=Find(p);
                if(state==null ? passives.Count<PassiveSlots : state.level<Mathf.Max(1,p.maxLevel))
                    bag.Add(new EquipmentChoice {kind=EquipmentChoiceKind.Passive,passive=p});
            }
            for(int i=bag.Count-1;i>0;i--) { int j=Random.Range(0,i+1); var temp=bag[i];bag[i]=bag[j];bag[j]=temp; }
            if(bag.Count>3) bag.RemoveRange(3,bag.Count-3);
            return bag.ToArray();
        }
    }
}
