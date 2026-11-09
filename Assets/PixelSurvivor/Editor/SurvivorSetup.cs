using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace PixelSurvivor.Editor
{
    public static class SurvivorSetup
    {
        public const string Root = "Assets/PixelSurvivor";
        public const string ScenePath = Root + "/Scenes/MainScene.unity";
        static Font font;
        static Sprite white;
        static readonly Color Ink = new Color(0.025f, 0.05f, 0.09f, 0.96f);
        static readonly Color PanelColor = new Color(0.06f, 0.12f, 0.18f, 1);
        static readonly Color Mint = new Color(0.37f, 0.95f, 0.76f);
        static readonly Color Muted = new Color(0.57f, 0.69f, 0.75f);

        [MenuItem("Tools/Pixel Survivor/Setup - Rebuild Game Scene", false, 10)]
        public static void SetupMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Stop Play mode before Setup."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Rebuild Pixel Survivor",
                "This recreates only PixelSurvivor/MainScene and its generated prefabs. Changes to those generated assets will be replaced. C# and PNG source files are preserved.", "Rebuild", "Cancel")) return;
            Build();
        }

        // Batch mode entry point: -executeMethod PixelSurvivor.Editor.SurvivorSetup.Build
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Prefabs"); Directory.CreateDirectory(Root + "/Scenes");
            AssetDatabase.Refresh();
            ImportSprites();
            font = KoreanSetup.EnsureFont();
            white = Sprite("White");
            if (font == null || white == null) throw new InvalidOperationException("Missing built-in font or Art/White.png");
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root + "/Input/Survivor.inputactions");
            if (actions == null) throw new InvalidOperationException("Input actions not imported. Wait for Package Manager / import to complete.");
            InputAction move = actions.FindAction("Gameplay/Move", true);
            string referencePath = Root + "/Input/MoveReference.asset";
            InputActionReference reference = AssetDatabase.LoadAssetAtPath<InputActionReference>(referencePath);
            if (reference == null) { reference = InputActionReference.Create(move); AssetDatabase.CreateAsset(reference, referencePath); }
            else { reference.Set(move); EditorUtility.SetDirty(reference); }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject map = new GameObject("Map");
            MapBounds bounds = map.AddComponent<MapBounds>();
            MakeMap(map.transform, bounds);
            GameObject runtime = new GameObject("RuntimeObjects");
            GameObject cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.025f, 0.05f, 0.07f);
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 100; cameraObject.transform.position = new Vector3(0, 0, -10);
            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();

            ExperienceGem gemPrefab = MakeGemPrefab();
            Projectile projectilePrefab = MakeProjectilePrefab();
            Enemy enemyPrefab = MakeEnemyPrefab(gemPrefab);
            Enemy mothPrefab = MakeVariantPrefab(gemPrefab, "EnemyMoth", 10, 3.4f, 8, 0.32f);
            Enemy beetlePrefab = MakeVariantPrefab(gemPrefab, "EnemyBeetle", 60, 1.25f, 18, 0.46f);
            Enemy[] addedMonsters = MonsterRosterSetup.BuildVariants(gemPrefab);
            GameObject playerPrefab = MakePlayerPrefab(reference, projectilePrefab);
            GameObject playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerObject.name = "Player";
            PlayerMovement player = playerObject.GetComponent<PlayerMovement>();
            follow.Configure(player.transform, bounds); follow.Snap();
            EnemySpawner spawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>(); spawner.SetEnemy(enemyPrefab); spawner.SetVariants(mothPrefab, beetlePrefab);
            spawner.SetAdditionalVariants(addedMonsters[0], addedMonsters[1]);
            GameObject managerObject = new GameObject("GameManager");
            GameTimer timer = managerObject.AddComponent<GameTimer>();
            GameManager game = managerObject.AddComponent<GameManager>();

            GameObject events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            var uiInput = events.AddComponent<InputSystemUIInputModule>();
            uiInput.UnassignActions(); uiInput.actionsAsset = actions;
            uiInput.point = InputReference(actions, "UI/Point", "UIPoint");
            uiInput.leftClick = InputReference(actions, "UI/Click", "UIClick");
            uiInput.scrollWheel = InputReference(actions, "UI/ScrollWheel", "UIScroll");
            uiInput.move = InputReference(actions, "UI/Navigate", "UINavigate");
            uiInput.submit = InputReference(actions, "UI/Submit", "UISubmit");
            uiInput.cancel = InputReference(actions, "UI/Cancel", "UICancel");
            BuildUI(out GameUI gameUI, out LevelUpUI levelUI, out ResultUI resultUI);
            game.Configure(player, spawner, timer, bounds, camera, runtime.transform, gameUI, levelUI, resultUI);
            EditorUtility.SetDirty(game); EditorUtility.SetDirty(spawner); EditorUtility.SetDirty(follow);
            CampaignSetup.Apply(game);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            foreach (var existing in EditorBuildSettings.scenes) if (existing.path != ScenePath) scenes.Add(existing);
            EditorBuildSettings.scenes = scenes.ToArray();
            PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = game.gameObject;
            ValidateScene();
#if !ENABLE_INPUT_SYSTEM
            Debug.LogWarning("Set Edit > Project Settings > Player > Active Input Handling to Input System Package (New), then restart Unity. Do not choose Both.");
#endif
            Debug.Log("Pixel Survivor setup finished. Open Game tab, press Play, then START. Tests: Window > General > Test Runner.");
        }

        static InputActionReference InputReference(InputActionAsset asset, string actionPath, string fileName)
        {
            string path = Root + "/Input/" + fileName + "Reference.asset";
            InputAction action = asset.FindAction(actionPath, true);
            InputActionReference reference = AssetDatabase.LoadAssetAtPath<InputActionReference>(path);
            if (reference == null) { reference = InputActionReference.Create(action); AssetDatabase.CreateAsset(reference, path); }
            else { reference.Set(action); EditorUtility.SetDirty(reference); }
            return reference;
        }
        static void ImportSprites()
        {
            foreach (string path in Directory.GetFiles(Root + "/Art", "*.png"))
            {
                var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true; importer.wrapMode = TextureWrapMode.Repeat;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }
        static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/" + name + ".png");
        static GameObject Visual(string name, Sprite sprite, Transform parent = null, int order = 0)
        {
            var go = new GameObject(name); if (parent != null) go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order;
            return go;
        }
        static GameObject SavePrefab(GameObject go, string name)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, Root + "/Prefabs/" + name + ".prefab");
            UnityEngine.Object.DestroyImmediate(go); return saved;
        }
        static ExperienceGem MakeGemPrefab()
        {
            var go = Visual("ExperienceGem", Sprite("ExperienceGem"), null, 2); go.AddComponent<ExperienceGem>();
            return SavePrefab(go, "ExperienceGem").GetComponent<ExperienceGem>();
        }
        static Projectile MakeProjectilePrefab()
        {
            var go = Visual("Projectile", Sprite("Projectile"), null, 12); go.AddComponent<Projectile>();
            return SavePrefab(go, "Projectile").GetComponent<Projectile>();
        }
        static Enemy MakeEnemyPrefab(ExperienceGem gem)
        {
            var go = Visual("Enemy", Sprite("Enemy"), null, 5); go.AddComponent<Enemy>().SetGem(gem);
            return SavePrefab(go, "Enemy").GetComponent<Enemy>();
        }
        static Enemy MakeVariantPrefab(ExperienceGem gem, string name, float hp, float speed, float damage, float radius)
        {
            var go = Visual(name, Sprite(name), null, 5);
            Enemy enemy = go.AddComponent<Enemy>(); enemy.SetGem(gem); enemy.ConfigureStats(hp, speed, damage, radius);
            return SavePrefab(go, name).GetComponent<Enemy>();
        }
        static Sprite[] Frames(string direction)
        {
            var frames = new Sprite[4];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = Sprite("Player_" + direction + "_" + i);
                if (frames[i] == null) throw new InvalidOperationException("Missing player frame: " + direction + " " + i);
            }
            return frames;
        }
        static void ConfigureAnimation(GameObject player)
        {
            PlayerSpriteAnimator animation = player.GetComponent<PlayerSpriteAnimator>();
            if (animation == null) animation = player.AddComponent<PlayerSpriteAnimator>();
            animation.Configure(Frames("Down"), Frames("Up"), Frames("Left"), Frames("Right"));
            EditorUtility.SetDirty(animation);
            EditorUtility.SetDirty(player.GetComponent<SpriteRenderer>());
        }
        [MenuItem("Tools/Pixel Survivor/Apply Monster and Animation Update", false, 11)]
        public static void ApplyContentUpdate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Stop Play mode before updating."); return; }
            GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (game == null || !game.HasRequiredReferences())
                throw new InvalidOperationException("Open your working MainScene first. For a fresh project use Setup - Rebuild Game Scene.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Root + "/Prefabs");
            AssetDatabase.Refresh(); ImportSprites();
            // Validate all 16 source frames before changing the scene.
            Frames("Down"); Frames("Up"); Frames("Left"); Frames("Right");
            if (Sprite("EnemyMoth") == null || Sprite("EnemyBeetle") == null)
                throw new InvalidOperationException("Missing new enemy PNGs. Copy the entire update Assets folder first.");
            ExperienceGem gem = game.Spawner.EnemyPrefab.GemPrefab;
            Enemy moth = MakeVariantPrefab(gem, "EnemyMoth", 10, 3.4f, 8, 0.32f);
            Enemy beetle = MakeVariantPrefab(gem, "EnemyBeetle", 60, 1.25f, 18, 0.46f);
            Enemy[] addedMonsters = MonsterRosterSetup.BuildVariants(gem);
            string playerPath = Root + "/Prefabs/Player.prefab";
            if (File.Exists(playerPath))
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(playerPath);
                try { ConfigureAnimation(contents); PrefabUtility.SaveAsPrefabAsset(contents, playerPath); }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
            ConfigureAnimation(game.Player.gameObject);
            game.Spawner.SetVariants(moth, beetle); EditorUtility.SetDirty(game.Spawner);
            game.Spawner.SetAdditionalVariants(addedMonsters[0], addedMonsters[1]);
            if (PrefabUtility.IsPartOfPrefabInstance(game.Player.gameObject))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(game.Player.GetComponent<SpriteRenderer>());
                PrefabUtility.RecordPrefabInstancePropertyModifications(game.Player.GetComponent<PlayerSpriteAnimator>());
            }
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
            EditorSceneManager.SaveScene(game.gameObject.scene);
            AssetDatabase.SaveAssets(); ValidateScene();
            Debug.Log("Content update applied: 3 enemies, 4-direction player walking. Moth unlock 30s; beetle unlock 90s.");
        }
        static GameObject MakePlayerPrefab(InputActionReference action, Projectile projectile)
        {
            var go = Visual("Player", Sprite("Player"), null, 10);
            go.AddComponent<PlayerStats>(); go.AddComponent<PlayerHealth>(); go.AddComponent<PlayerLevel>();
            go.AddComponent<PlayerMovement>().SetInput(action); go.AddComponent<AutoAttack>().SetProjectile(projectile);
            ConfigureAnimation(go);
            return SavePrefab(go, "Player");
        }
        static void MakeMap(Transform parent, MapBounds bounds)
        {
            GameObject floor = Visual("Ground", Sprite("Ground"), parent, -20);
            SpriteRenderer renderer = floor.GetComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Tiled; renderer.size = bounds.HalfSize * 2;
            Vector2 half = bounds.HalfSize;
            Block("North boundary", parent, new Vector2(0, half.y), new Vector2(half.x * 2 + 0.5f, 0.5f));
            Block("South boundary", parent, new Vector2(0, -half.y), new Vector2(half.x * 2 + 0.5f, 0.5f));
            Block("West boundary", parent, new Vector2(-half.x, 0), new Vector2(0.5f, half.y * 2));
            Block("East boundary", parent, new Vector2(half.x, 0), new Vector2(0.5f, half.y * 2));
            var random = new System.Random(41);
            for (int i = 0; i < 110; i++)
            {
                var detail = Visual("Floor detail " + i, Sprite("Tuft"), parent, -10);
                detail.transform.localPosition = new Vector3((float)(random.NextDouble() * 60 - 30), (float)(random.NextDouble() * 36 - 18), 0);
            }
        }
        static void Block(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = Visual(name, white, parent, -5); go.transform.localPosition = position;
            go.transform.localScale = new Vector3(size.x * 32, size.y * 32, 1);
            go.GetComponent<SpriteRenderer>().color = new Color(0.21f, 0.42f, 0.4f);
        }
        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        static Image Box(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var image = Rect(name, parent, position, size).gameObject.AddComponent<Image>();
            image.sprite = white; image.color = color; image.raycastTarget = false; return image;
        }
        static Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var text = Rect(name, parent, position, size).gameObject.AddComponent<Text>();
            text.font = font; text.text = KoreanSetup.Legacy(value); text.fontSize = fontSize; text.color = color; text.alignment = alignment;
            text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
        static Button Button(string name, Transform parent, string label, Vector2 position, Vector2 size)
        {
            var box = Box(name, parent, position, size, PanelColor); box.raycastTarget = true;
            var button = box.gameObject.AddComponent<Button>(); button.targetGraphic = box;
            var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1.3f, 1.6f, 1.7f);
            colors.selectedColor = new Color(1.3f, 1.6f, 1.7f); colors.pressedColor = new Color(0.6f, 0.9f, 0.8f); button.colors = colors;
            if (!string.IsNullOrEmpty(label)) Label("Label", box.transform, label, Vector2.zero, size - new Vector2(16, 8), 28, Mint);
            return button;
        }
        static GameObject Overlay(string name, Transform parent)
        {
            Image panel = Box(name, parent, Vector2.zero, Vector2.zero, Ink); Stretch(panel.rectTransform);
            panel.raycastTarget = true; return panel.gameObject;
        }
        static Image Bar(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            Image background = Box(name, parent, position, size, new Color(0.1f, 0.17f, 0.23f));
            Image fill = Box("Fill", background.transform, Vector2.zero, size, color);
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0;
            return fill;
        }
        static UpgradeButton MakeUpgradeButton()
        {
            var button = Button("UpgradeButton", null, "", Vector2.zero, new Vector2(340, 240));
            Text number = Label("Number", button.transform, "01", new Vector2(0, 74), new Vector2(300, 38), 22, Muted);
            Text title = Label("Title", button.transform, "POWER CORE", new Vector2(0, 20), new Vector2(310, 50), 26, Mint);
            Text description = Label("Description", button.transform, "Damage +20%\n10 > 12", new Vector2(0, -56), new Vector2(310, 80), 23, Color.white);
            var upgrade = button.gameObject.AddComponent<UpgradeButton>(); upgrade.Configure(button, title, description, number);
            return SavePrefab(button.gameObject, "UpgradeButton").GetComponent<UpgradeButton>();
        }
        static void BuildUI(out GameUI gameUI, out LevelUpUI levelUI, out ResultUI resultUI)
        {
            var root = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = 0.5f;
            gameUI = root.AddComponent<GameUI>(); levelUI = root.AddComponent<LevelUpUI>(); resultUI = root.AddComponent<ResultUI>();
            var hud = Rect("GameplayPanel", root.transform, Vector2.zero, Vector2.zero); Stretch(hud);
            var top = Box("Header", hud, Vector2.zero, new Vector2(0, 110), new Color(0.025f, 0.05f, 0.08f, 0.94f));
            top.rectTransform.anchorMin = new Vector2(0, 1); top.rectTransform.anchorMax = new Vector2(1, 1);
            top.rectTransform.pivot = new Vector2(0.5f, 1); top.rectTransform.anchoredPosition = Vector2.zero;
            Text level = Label("Level", top.transform, "LEVEL 01", new Vector2(-655, 22), new Vector2(200, 40), 25, Mint, TextAnchor.MiddleLeft);
            Text hp = Label("HP", top.transform, "HP 100 / 100", new Vector2(-620, -15), new Vector2(270, 28), 18, Color.white, TextAnchor.MiddleLeft);
            Image hpBar = Bar("HP Bar", top.transform, new Vector2(-600, -36), new Vector2(310, 9), new Color(0.95f, 0.37f, 0.45f));
            Label("Timer caption", top.transform, "TIME REMAINING", new Vector2(0, 29), new Vector2(300, 24), 16, Muted);
            Text timer = Label("Timer", top.transform, "00:00:00", new Vector2(0, -5), new Vector2(300, 55), 40, Color.white);
            Text kills = Label("Kills", top.transform, "KILLS 000", new Vector2(650, 8), new Vector2(220, 48), 26, Color.white);
            var bottom = Rect("Footer", hud, Vector2.zero, new Vector2(0, 90));
            bottom.anchorMin = new Vector2(0, 0); bottom.anchorMax = new Vector2(1, 0); bottom.pivot = new Vector2(0.5f, 0);
            Text exp = Label("EXP", bottom, "EXP 0 / 5", new Vector2(0, 8), new Vector2(280, 30), 18, Color.white);
            Image expBar = Bar("EXP Bar", bottom, new Vector2(0, -20), new Vector2(1460, 10), new Color(0.35f, 0.72f, 1));

            GameObject menu = Overlay("MainMenuPanel", root.transform);
            Label("Eyebrow", menu.transform, "INFINITE SURVIVAL", new Vector2(0, 210), new Vector2(900, 40), 22, Muted);
            Label("Title", menu.transform, "PIXEL SURVIVOR", new Vector2(0, 120), new Vector2(1200, 100), 78, Mint);
            Label("Subtitle", menu.transform, "L U M E N   F I E L D", new Vector2(0, 45), new Vector2(900, 45), 28, Color.white);
            Label("Instructions", menu.transform, "WASD or arrow keys to move. Attacks are automatic.\nCollect gems. Build equipment. Survive as long as you can.", new Vector2(0, -55), new Vector2(1000, 110), 24, Muted);
            Button start = Button("StartButton", menu.transform, "START", new Vector2(0, -190), new Vector2(300, 70));
            Label("Menu help", menu.transform, "Click a button, or use arrow keys + Enter", new Vector2(0, -290), new Vector2(1000, 40), 18, Muted);
            gameUI.Configure(menu, hud.gameObject, start, level, timer, hp, exp, kills, hpBar, expBar);
            UnityEngine.Object.FindAnyObjectByType<EventSystem>().firstSelectedGameObject = start.gameObject;

            GameObject upgrade = Overlay("LevelUpPanel", root.transform);
            Label("Title", upgrade.transform, "LEVEL UP", new Vector2(0, 230), new Vector2(900, 90), 64, Mint);
            Label("Help", upgrade.transform, "CHOOSE ONE UPGRADE  /  TIME IS PAUSED", new Vector2(0, 155), new Vector2(1000, 45), 22, Muted);
            UpgradeButton cardPrefab = MakeUpgradeButton(); var cards = new UpgradeButton[3];
            for (int i = 0; i < 3; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab.gameObject, upgrade.transform);
                go.name = "Choice " + (i + 1); go.GetComponent<RectTransform>().anchoredPosition = new Vector2((i - 1) * 370, -20);
                cards[i] = go.GetComponent<UpgradeButton>();
            }
            for (int i = 0; i < 3; i++)
            {
                var nav = new Navigation { mode = Navigation.Mode.Explicit,
                    selectOnLeft = cards[(i + 2) % 3].Button, selectOnRight = cards[(i + 1) % 3].Button };
                cards[i].Button.navigation = nav;
            }
            Label("Help bottom", upgrade.transform, "Click a card, or use Left / Right + Enter", new Vector2(0, -240), new Vector2(900, 40), 20, Muted);
            levelUI.Configure(upgrade, cards);

            GameObject result = Overlay("ResultPanel", root.transform);
            Text resultTitle = Label("Title", result.transform, "GAME OVER", new Vector2(0, 170), new Vector2(1100, 100), 72, Mint);
            Text summary = Label("Summary", result.transform, "", new Vector2(0, 0), new Vector2(1000, 180), 32, Color.white);
            Button restart = Button("RestartButton", result.transform, "RESTART", new Vector2(0, -200), new Vector2(340, 70));
            resultUI.Configure(result, resultTitle, summary, restart);
            hud.gameObject.SetActive(false); upgrade.SetActive(false); result.SetActive(false);
        }

        [MenuItem("Tools/Pixel Survivor/Validate Current Scene", false, 20)]
        public static void ValidateScene()
        {
            GameManager manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (manager == null || !manager.HasRequiredReferences()) throw new InvalidOperationException("Missing gameplay/UI/input/prefab references. Run Setup again.");
            if (!manager.Spawner.HasExpandedRoster)
                throw new InvalidOperationException("Missing enemy variants. Run Apply Monster and Animation Update.");
            PlayerSpriteAnimator animation = manager.Player.GetComponent<PlayerSpriteAnimator>();
            if (animation == null || !animation.IsConfigured)
                throw new InvalidOperationException("Missing player animation frames. Run Apply Monster and Animation Update.");
            var campaign=manager.GetComponent<CampaignController>();
            if(campaign!=null)CampaignSetup.ValidateData(campaign.Data);
            int missing = 0;
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
            foreach (string path in Directory.GetFiles(Root + "/Prefabs", "*.prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\', '/'));
                if (prefab == null) throw new InvalidOperationException("Missing prefab: " + path);
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                    missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                foreach (SpriteRenderer sprite in prefab.GetComponentsInChildren<SpriteRenderer>(true))
                    if (sprite.sprite == null) throw new InvalidOperationException("Missing sprite in " + path);
            }
            if (missing != 0) throw new InvalidOperationException("Missing Script count: " + missing);
            var input = UnityEngine.Object.FindAnyObjectByType<InputSystemUIInputModule>();
            if (input == null || input.point == null || input.leftClick == null || input.move == null || input.submit == null)
                throw new InvalidOperationException("Missing InputSystemUIInputModule action references");
            Debug.Log("Pixel Survivor: required refs, 7 prefabs, 3 enemies, 16 player frames, sprites, UI, Input System and Missing Script check passed. This does not replace a Play test.");
        }
    }
}
