using NUnit.Framework;
using UnityEngine;
namespace PixelSurvivor.Tests
{
    public sealed class EquipmentRulesTests
    {
        WeaponData[] weapons;PassiveEquipmentData[] passives;EquipmentManager equipment;CampaignData data;
        [SetUp] public void Setup()
        {
            equipment=new EquipmentManager();weapons=new WeaponData[8];passives=new PassiveEquipmentData[8];
            for(int i=0;i<8;i++){passives[i]=ScriptableObject.CreateInstance<PassiveEquipmentData>();passives[i].id="p"+i;passives[i].maxLevel=5;
                weapons[i]=ScriptableObject.CreateInstance<WeaponData>();weapons[i].id="w"+i;weapons[i].maxLevel=5;weapons[i].evolutionPartner=passives[i];}
            data=ScriptableObject.CreateInstance<CampaignData>();data.weapons=weapons;data.passives=passives;
        }
        [TearDown] public void Cleanup(){foreach(var w in weapons)Object.DestroyImmediate(w);foreach(var p in passives)Object.DestroyImmediate(p);Object.DestroyImmediate(data);}
        [Test] public void SlotsNeverExceedSevenOfEach()
        {for(int i=0;i<7;i++){Assert.IsTrue(equipment.Add(weapons[i]));Assert.IsTrue(equipment.Add(passives[i]));}Assert.IsFalse(equipment.Add(weapons[7]));Assert.IsFalse(equipment.Add(passives[7]));Assert.AreEqual(7,equipment.Weapons.Count);Assert.AreEqual(7,equipment.Passives.Count);}
        [Test] public void BothEvolutionConditionsAreRequiredAndPartnerIsKept()
        {
            equipment.Add(weapons[0]);equipment.Add(passives[0]);var w=equipment.Find(weapons[0]);
            Assert.IsFalse(equipment.CanEvolve(w));for(int i=0;i<3;i++)equipment.Add(weapons[0]);Assert.AreEqual(4,w.level);Assert.IsFalse(equipment.Evolve(w));
            equipment.Add(weapons[0]);Assert.IsTrue(equipment.Evolve(w));Assert.IsTrue(w.evolved);Assert.AreEqual(1,equipment.Weapons.Count);Assert.AreEqual(1,equipment.Find(passives[0]).level);Assert.IsFalse(equipment.Evolve(w));
        }
        [Test] public void MaxWeaponWithoutPartnerCannotEvolve()
        {for(int i=0;i<5;i++)equipment.Add(weapons[0]);Assert.IsFalse(equipment.CanEvolve(equipment.Find(weapons[0])));equipment.Add(passives[1]);Assert.IsFalse(equipment.CanEvolve(equipment.Find(weapons[0])));}
        [Test] public void FullSlotsStillOfferOwnedUpgradesAndNeverAnEighth()
        {
            for(int i=0;i<7;i++){equipment.Add(weapons[i]);equipment.Add(passives[i]);}
            for(int roll=0;roll<30;roll++){var choices=equipment.RollChoices(data);Assert.AreEqual(3,choices.Length);var seen=new System.Collections.Generic.HashSet<Object>();
                foreach(var c in choices){Object item=c.weapon!=null?(Object)c.weapon:c.passive;Assert.IsTrue(seen.Add(item));Assert.AreNotSame(weapons[7],item);Assert.AreNotSame(passives[7],item);}}
        }
        [Test] public void AllMaxOffersNoNonEquipmentFallbackAndResetRemovesEvolution()
        {
            for(int i=0;i<7;i++){for(int rank=0;rank<5;rank++){equipment.Add(weapons[i]);equipment.Add(passives[i]);}equipment.Evolve(equipment.Find(weapons[i]));}
            Assert.AreEqual(0,equipment.RollChoices(data).Length);
            equipment.Reset(weapons[1]);Assert.AreEqual(1,equipment.Weapons.Count);Assert.AreEqual(0,equipment.Passives.Count);Assert.AreEqual(1,equipment.Weapons[0].level);Assert.IsFalse(equipment.Weapons[0].evolved);
        }
        [TestCase(0,1)][TestCase(0.849f,1)][TestCase(0.85f,3)][TestCase(0.969f,3)][TestCase(0.97f,5)][TestCase(1,5)]
        public void JackpotBoundaries(float roll,int expected){Assert.AreEqual(expected,TreasureChestSystem.RollCount(roll,85,12,3));}
        [Test] public void ZeroJackpotWeightsFallBackToSingle(){Assert.AreEqual(1,TreasureChestSystem.RollCount(0.5f,0,0,0));}
    }
}
