using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PixelSurvivor
{
    public static class MonsterRosterSetup
    {
        const string Root="Assets/PixelSurvivor";

        static Sprite Import(string name)
        {
            string path=Root+"/Resources/Redesign/"+name+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing monster art: "+path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=800;
            importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency=true;
            importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null)throw new InvalidOperationException("Cannot import monster sprite: "+path);
            return sprite;
        }

        static Enemy Create(ExperienceGem gem,string name,Sprite sprite,float hp,float speed,float damage,float radius)
        {
            var go=new GameObject(name);
            var visual=go.AddComponent<SpriteRenderer>();visual.sprite=sprite;visual.sortingOrder=5;
            var enemy=go.AddComponent<Enemy>();enemy.SetGem(gem);enemy.SetAppearance(sprite);
            enemy.ConfigureStats(hp,speed,damage,radius);
            string path=Root+"/Prefabs/"+name+".prefab";
            var saved=PrefabUtility.SaveAsPrefabAsset(go,path);
            UnityEngine.Object.DestroyImmediate(go);
            return saved.GetComponent<Enemy>();
        }

        public static Enemy[] BuildVariants(ExperienceGem gem)
        {
            if(gem==null)throw new InvalidOperationException("Enemy experience gem prefab is missing");
            return new[]
            {
                Create(gem,"EnemyBoneCrawler",Import("BoneCrawler"),45,1.7f,14,.42f),
                Create(gem,"EnemyEmberWraith",Import("EmberWraith"),24,2.8f,12,.35f)
            };
        }

        [MenuItem("Tools/Pixel Survivor/Apply Expanded Monster Roster")]
        public static void ApplyToOpenScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play mode first");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.IsValid() || !scene.path.EndsWith("/MainScene.unity"))
                throw new InvalidOperationException("Open PixelSurvivor MainScene first");
            var game=UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if(game==null || game.Spawner==null || game.Spawner.EnemyPrefab==null)
                throw new InvalidOperationException("MainScene enemy spawner is not configured");
            var variants=BuildVariants(game.Spawner.EnemyPrefab.GemPrefab);
            game.Spawner.SetAdditionalVariants(variants[0],variants[1]);
            EditorUtility.SetDirty(game.Spawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
