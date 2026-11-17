using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PixelSurvivor.Tests
{
    public sealed class StageArtTests
    {
        [UnityTest]
        public IEnumerator EachStageUsesItsOwnChestsAndOnlyLaterStagesHaveScenery()
        {
            yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");
            yield return null;
            var game=Object.FindAnyObjectByType<GameManager>();
            var campaign=game.Campaign;
            campaign.UseTemporaryProgress();
            campaign.Save.UnlockNext(1,campaign.Data.stages.Length);
            string[] names={"Meadow","Amber","Ruin"};
            for(int stage=0;stage<names.Length;stage++)
            {
                campaign.SelectCharacter(0);
                Assert.IsTrue(campaign.SelectStage(stage));
                game.BeginRun();
                Assert.IsTrue(game.IsPlaying);
                Assert.AreEqual(names[stage]+"Supply",campaign.Field.Props[0].GetComponent<SpriteRenderer>().sprite.name);
                int opening=0;
                foreach(var prop in campaign.Field.Props)
                    if(Vector2.Distance(prop.transform.position,game.Player.transform.position)<8)opening++;
                Assert.GreaterOrEqual(opening,3,"Supply objects should be visible near the stage start.");
                var first=campaign.Field.Props[0];
                Assert.IsFalse(campaign.Terrain.IsNearScenery(first.transform.position,1.6f));
                first.TakeDamage(campaign.Data.propHP+1);
                Assert.Greater(campaign.Field.Items.Count,0,"Breaking a supply object should still drop an item.");
                var chest=campaign.Field.Drop(game.Player.transform.position,FieldItemKind.EliteChest);
                Assert.AreEqual(names[stage]+"Chest",chest.GetComponent<SpriteRenderer>().sprite.name);
                Transform terrain=null;
                foreach(Transform child in game.Bounds.transform)
                    if(child.name=="Stage terrain" && child.gameObject.activeSelf)terrain=child;
                if(stage==0)
                {
                    Assert.IsNull(terrain);
                    Assert.AreEqual(0,campaign.Terrain.ObstacleCount);
                }
                else
                {
                    Assert.NotNull(terrain);
                    Assert.Greater(campaign.Terrain.ObstacleCount,0);
                    Assert.IsTrue(ContainsSprite(terrain,stage==1?"AmberTree":"RuinTree"));
                    Assert.IsTrue(ContainsSprite(terrain,stage==1?"AmberBoulder":"RuinStone"));
                    if(stage==2)
                    {
                        foreach(var renderer in terrain.GetComponentsInChildren<SpriteRenderer>())
                            Assert.IsTrue(renderer.sprite.name=="RuinTree" || renderer.sprite.name=="RuinStone",
                                "Non-pixel ruin object: "+renderer.name);
                        Assert.AreEqual(0,campaign.Terrain.HazardCount);
                        foreach(Transform child in game.Bounds.transform)
                            Assert.IsFalse(child.name=="Stage floor layout" && child.gameObject.activeSelf);
                    }
                }
                Debug.Log("Stage art verified: "+names[stage]+" chest, supply prop and terrain.");
                game.ReturnToMenu();
                yield return null;
            }
            Time.timeScale=1;
        }

        static bool ContainsSprite(Transform root,string name)
        {
            foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>())
                if(renderer.sprite!=null && renderer.sprite.name==name)return true;
            return false;
        }
    }
}
