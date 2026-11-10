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

        static Enemy Create(ExperienceGem gem,string name,Sprite sprite,float hp,float speed,float damage,float radius,EnemyTacticKind tactic)
        {
            var go=new GameObject(name);
            var visual=go.AddComponent<SpriteRenderer>();visual.sprite=sprite;visual.sortingOrder=5;
            var enemy=go.AddComponent<Enemy>();enemy.SetGem(gem);enemy.SetAppearance(sprite);
            enemy.ConfigureStats(hp,speed,damage,radius);
            go.AddComponent<EnemyTactics>().Configure(tactic);
            string path=Root+"/Prefabs/"+name+".prefab";
            var saved=PrefabUtility.SaveAsPrefabAsset(go,path);
            UnityEngine.Object.DestroyImmediate(go);
            return saved.GetComponent<Enemy>();
        }

        static Sprite ImportTheme(string name,int pixelsPerUnit)
        {
            string path=Root+"/Resources/StageCreatures/"+name+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing stage art: "+path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=pixelsPerUnit;
            importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency=true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Enemy CreateTheme(ExperienceGem gem,string name,Sprite sprite,float hp,float speed,float damage,float radius,bool charger)
        {
            if(sprite==null)throw new InvalidOperationException("Missing sprite for "+name);
            var go=new GameObject(name);
            var visual=go.AddComponent<SpriteRenderer>();visual.sprite=sprite;visual.sortingOrder=5;
            var enemy=go.AddComponent<Enemy>();enemy.SetGem(gem);enemy.SetAppearance(sprite);
            enemy.ConfigureStats(hp,speed,damage,radius);
            if(charger)go.AddComponent<EnemyTactics>().Configure(EnemyTacticKind.Charger);
            var saved=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return saved.GetComponent<Enemy>();
        }

        [MenuItem("Tools/Pixel Survivor/Apply Stage Theme Roster")]
        public static void ApplyStageThemesToOpenScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play mode first");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.IsValid() || !scene.path.EndsWith("/MainScene.unity"))
                throw new InvalidOperationException("Open PixelSurvivor MainScene first");
            var game=UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if(game==null || game.Spawner==null || game.Spawner.EnemyPrefab==null)
                throw new InvalidOperationException("MainScene enemy spawner is not configured");
            AssetDatabase.Refresh();
            var gem=game.Spawner.EnemyPrefab.GemPrefab;
            var fox=CreateTheme(gem,"EnemyMeadowFox",ImportTheme("MeadowFox",1100),18,3.3f,8,.34f,true);
            var kestrel=CreateTheme(gem,"EnemyAmberKestrel",ImportTheme("AmberKestrel",1280),20,3.8f,9,.36f,true);
            var bat=CreateTheme(gem,"EnemyRuinBat",ImportTheme("RuinBat",1280),24,3.5f,11,.38f,true);
            var stag=CreateTheme(gem,"BossLumenStag",ImportTheme("LumenStagBoss",1200),100,1.7f,13,.8f,false);
            var cicada=CreateTheme(gem,"BossAmberCicada",ImportTheme("AmberCicadaBoss",1450),100,1.7f,13,.8f,false);
            var lich=CreateTheme(gem,"BossVioletLich",ImportTheme("VioletLichBoss",1200),100,1.7f,13,.8f,false);
            ImportTheme("LumenGround",32);
            game.Spawner.SetThemedVariants(fox,kestrel,bat,stag,cicada,lich);
            EditorUtility.SetDirty(game.Spawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        public static Enemy[] BuildVariants(ExperienceGem gem)
        {
            if(gem==null)throw new InvalidOperationException("Enemy experience gem prefab is missing");
            return new[]
            {
                Create(gem,"EnemyBoneCrawler",Import("BoneCrawler"),45,1.7f,14,.42f,EnemyTacticKind.Charger),
                Create(gem,"EnemyEmberWraith",Import("EmberWraith"),24,2.8f,12,.35f,EnemyTacticKind.EmberCaster)
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
