using System;
using System.Collections.Generic;
using UnityEngine;
namespace PixelSurvivor
{
    [Serializable] public sealed class SavedUpgrade { public string id; public int level; }
    [Serializable] public sealed class SurvivalRecord { public string characterId,stageId; public float seconds; }
    [Serializable] public sealed class MetaProgress
    {
        public int version=2, gold, unlockedStages=1;
        public List<SurvivalRecord> records=new List<SurvivalRecord>();
        public List<SavedUpgrade> upgrades=new List<SavedUpgrade>();
        public List<string> unlockedCharacters=new List<string>();
        public List<string> discoveredEquipment=new List<string>();
    }
    public sealed class SaveManager
    {
        public const string DefaultKey="PixelSurvivor.Campaign.v1";
        readonly string key;
        public MetaProgress Progress {get;private set;}
        public SaveManager(CampaignData data, string storageKey=DefaultKey)
        {
            key=storageKey;
            try { Progress=string.IsNullOrEmpty(key)?new MetaProgress():JsonUtility.FromJson<MetaProgress>(PlayerPrefs.GetString(key,"")); }
            catch(Exception) { Progress=null; }
            if(Progress==null) Progress=new MetaProgress();
            Progress.version=3;
            if(Progress.discoveredEquipment==null)Progress.discoveredEquipment=new List<string>();
            if(Progress.records==null)Progress.records=new List<SurvivalRecord>();
            Progress.gold=Mathf.Max(0,Progress.gold);
            Progress.unlockedStages=Mathf.Clamp(Progress.unlockedStages,1,data.stages.Length);
            if(Progress.upgrades==null) Progress.upgrades=new List<SavedUpgrade>();
            if(Progress.unlockedCharacters==null) Progress.unlockedCharacters=new List<string>();
            foreach(var c in data.characters) if(c!=null && !Progress.unlockedCharacters.Contains(c.id)) Progress.unlockedCharacters.Add(c.id);
            bool newlyUnlocked=false;
            foreach(var weapon in data.weapons)
            {
                if(weapon==null)continue;
                string normal="weapon."+weapon.id,evolved="evolution."+weapon.id;
                if(!Progress.discoveredEquipment.Contains(normal)){Progress.discoveredEquipment.Add(normal);newlyUnlocked=true;}
                if(!Progress.discoveredEquipment.Contains(evolved)){Progress.discoveredEquipment.Add(evolved);newlyUnlocked=true;}
            }
            foreach(var passive in data.passives)
            {
                if(passive==null)continue;
                string id="passive."+passive.id;
                if(!Progress.discoveredEquipment.Contains(id)){Progress.discoveredEquipment.Add(id);newlyUnlocked=true;}
            }
            if(newlyUnlocked)Save();
        }
        public bool Discovered(string id) => !string.IsNullOrEmpty(id) && Progress.discoveredEquipment.Contains(id);
        public void Discover(string id)
        {
            if(string.IsNullOrEmpty(id) || Discovered(id))return;
            Progress.discoveredEquipment.Add(id);Save();
        }
        public int Level(string id) { var s=Progress.upgrades.Find(x=>x!=null && x.id==id); return s==null ? 0 : Mathf.Max(0,s.level); }
        public void AddGold(int value) { Progress.gold=(int)Math.Min(int.MaxValue,(long)Progress.gold+Mathf.Max(0,value)); }
        public bool Purchase(string prefix, PermanentUpgrade upgrade)
        {
            string id=prefix+upgrade.id; int level=Level(id);
            if(level>=upgrade.maxLevel) return false;
            int cost=upgrade.Cost(level); if(Progress.gold<cost) return false;
            Progress.gold-=cost; var entry=Progress.upgrades.Find(x=>x!=null && x.id==id);
            if(entry==null) {entry=new SavedUpgrade {id=id};Progress.upgrades.Add(entry);} entry.level=level+1;
            Save(); return true;
        }
        public void UnlockNext(int clearedIndex,int stageCount)
        { Progress.unlockedStages=Mathf.Clamp(Mathf.Max(Progress.unlockedStages,clearedIndex+2),1,stageCount); Save(); }
        public float Best(string characterId,string stageId)
        { var r=Progress.records.Find(x=>x!=null && x.characterId==characterId && x.stageId==stageId);return r==null || float.IsNaN(r.seconds) || float.IsInfinity(r.seconds)?0:Mathf.Max(0,r.seconds); }
        public void Record(string characterId,string stageId,float seconds)
        {
            if(float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds<=Best(characterId,stageId))return;
            var r=Progress.records.Find(x=>x!=null && x.characterId==characterId && x.stageId==stageId);
            if(r==null){r=new SurvivalRecord{characterId=characterId,stageId=stageId};Progress.records.Add(r);}r.seconds=seconds;
        }
        public void Save() { if(string.IsNullOrEmpty(key))return;PlayerPrefs.SetString(key,JsonUtility.ToJson(Progress)); PlayerPrefs.Save(); }
    }
}
