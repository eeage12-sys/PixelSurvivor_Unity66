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
            if(charger)
            {
                var tactics=go.AddComponent<EnemyTactics>();tactics.Configure(EnemyTacticKind.Charger);
                if(name=="EnemyMeadowFox")tactics.ConfigureCharge(.22f,2f,5.5f);
                if(name=="EnemyAmberKestrel")tactics.ConfigureCharge(.24f,2.1f,5.5f);
            }
            var saved=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+name+".prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return saved.GetComponent<Enemy>();
        }

        [MenuItem("Tools/Pixel Survivor/Apply Combat Balance")]
        public static void ApplyCombatBalance()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play mode first");
            ApplyChargeBalance("EnemyMeadowFox",.22f,2f,5.5f);
            ApplyChargeBalance("EnemyAmberKestrel",.24f,2.1f,5.5f);
            var playerPath=Root+"/Prefabs/Player.prefab";
            var player=PrefabUtility.LoadPrefabContents(playerPath);
            try
            {
                var health=player.GetComponent<PlayerHealth>();
                if(health==null)throw new InvalidOperationException("Player health is missing");
                var serialized=new SerializedObject(health);
                serialized.FindProperty("invulnerabilitySeconds").floatValue=1f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(player,playerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            AssetDatabase.SaveAssets();
        }

        static void ApplyChargeBalance(string name,float duration,float speedMultiplier,float minimumSpeed)
        {
            string path=Root+"/Prefabs/"+name+".prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var tactics=root.GetComponent<EnemyTactics>();
                if(tactics==null)throw new InvalidOperationException(name+" tactics are missing");
                tactics.ConfigureCharge(duration,speedMultiplier,minimumSpeed);
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("Tools/Pixel Survivor/Apply Early Stage Monster Roster")]
        public static void ApplyEarlyStageRosterToOpenScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play mode first");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(!scene.IsValid() || !scene.path.EndsWith("/MainScene.unity"))
                throw new InvalidOperationException("Open PixelSurvivor MainScene first");
            var game=UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if(game==null || game.Spawner==null || game.Spawner.EnemyPrefab==null)
                throw new InvalidOperationException("MainScene enemy spawner is not configured");
            AssetDatabase.Refresh();
            var light=ImportTheme("MeadowLightSpirit",1050);
            var guardian=ImportTheme("MeadowGuardian",1050);
            var wind=ImportTheme("WindLeafSpirit",1100);
            var firefly=ImportTheme("AmberFirefly",1280);
            var zombie=ImportTheme("RuinZombie",1100);
            string basePath=Root+"/Prefabs/Enemy.prefab";
            var baseRoot=PrefabUtility.LoadPrefabContents(basePath);
            try
            {
                var enemy=baseRoot.GetComponent<Enemy>();
                if(enemy==null)throw new InvalidOperationException("Base enemy prefab is missing");
                enemy.SetAppearance(light);enemy.SetEliteAppearance(guardian);
                baseRoot.GetComponent<SpriteRenderer>().sprite=light;
                PrefabUtility.SaveAsPrefabAsset(baseRoot,basePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(baseRoot); }
            var gem=game.Spawner.EnemyPrefab.GemPrefab;
            var windPrefab=CreateTheme(gem,"EnemyWindLeafSpirit",wind,15,2.55f,6,.31f,false);
            var fireflyPrefab=CreateTheme(gem,"EnemyAmberFirefly",firefly,17,3.15f,7,.32f,false);
            var zombiePrefab=CreateTheme(gem,"EnemyRuinZombie",zombie,30,1.9f,10,.4f,false);
            game.Spawner.SetEarlyVariants(windPrefab,fireflyPrefab,zombiePrefab);
            var campaign=game.GetComponent<CampaignController>();
            if(campaign==null || campaign.Data==null)
                throw new InvalidOperationException("MainScene campaign data is not configured");
            var stages=campaign.Data.stages;
            stages[0].description="햇살이 내려앉은 들판입니다. 잎 정령과 바람 정령을 피해 살아남으세요.";
            stages[1].description="호박빛 분지입니다. 나방과 반딧불이 나타나고, 3분 뒤 딱정벌레가 합류합니다.";
            stages[2].description="보랏빛 폐허입니다. 박쥐와 좀비가 배회하고, 3분 뒤 해골이 나타납니다.";
            foreach(var stageData in stages)EditorUtility.SetDirty(stageData);
            EditorUtility.SetDirty(game.Spawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
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
