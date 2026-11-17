using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PixelSurvivor.Tests
{
    public sealed class EnemySizeTests
    {
        [UnityTest]
        public IEnumerator RegularEnemiesAreSmallerAndElitesOnlySlightlyLarger()
        {
            yield return SceneManager.LoadSceneAsync("Assets/PixelSurvivor/Scenes/MainScene.unity");
            yield return null;
            var game=Object.FindAnyObjectByType<GameManager>();
            game.Campaign.UseTemporaryProgress();
            game.Campaign.SelectCharacter(0);
            game.BeginRun();

            Enemy[] prefabs={game.Spawner.EnemyPrefab,game.Spawner.AmberScorpionPrefab,game.Spawner.CryptKnightPrefab};
            foreach(var prefab in prefabs)
            {
                Assert.NotNull(prefab);
                var regular=game.Spawner.SpawnSpecial(new Vector2(15,10),prefab);
                var elite=game.Spawner.SpawnSpecial(new Vector2(18,10),prefab);
                regular.ConfigureEncounter(1,1,1,false,false,false);
                elite.ConfigureEncounter(1,1,1,true,false,false);
                Assert.AreEqual(0.72f,regular.transform.localScale.x,0.001f);
                float regularExtent=Extent(regular.GetComponent<SpriteRenderer>());
                float eliteExtent=Extent(elite.GetComponent<SpriteRenderer>());
                Assert.That(eliteExtent/regularExtent,Is.InRange(1.19f,1.25f),prefab.name);
                Assert.That(elite.Radius/regular.Radius,Is.InRange(1.19f,1.25f),prefab.name);
                Debug.Log("Enemy size verified: "+prefab.name+" regular="+regularExtent.ToString("F2")+
                    " elite="+eliteExtent.ToString("F2"));
            }
            var boss=game.Spawner.SpawnSpecial(new Vector2(20,10),game.Spawner.GuardianGolemPrefab);
            boss.ConfigureEncounter(1,1,1,false,true,false);
            Assert.AreEqual(2.8f,boss.transform.localScale.x,0.001f);
            Time.timeScale=1;
        }

        static float Extent(SpriteRenderer renderer)
        {return Mathf.Max(renderer.bounds.size.x,renderer.bounds.size.y);}
    }
}
