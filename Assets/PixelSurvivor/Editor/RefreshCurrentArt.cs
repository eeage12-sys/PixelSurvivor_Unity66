using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PixelSurvivor.Editor
{
    // Keeps serialized sprites and prefab previews in sync with the sheets used in Play Mode.
    public static class RefreshCurrentArt
    {
        const string Root = "Assets/PixelSurvivor/";
        static readonly string[] Directions = { "Down", "Up", "Left", "Right" };

        [MenuItem("Tools/Pixel Survivor/Refresh Current Character And Monster Art")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before refreshing art.");

            var characters = new Texture2D[5];
            for (int i = 0; i < characters.Length; i++)
                characters[i] = Load("Resources/Redesign/Character" + i + ".png");
            var monsters = Load("Resources/Redesign/Monsters.png");
            var imports = new Dictionary<string, float>();

            for (int character = 0; character < characters.Length; character++)
            {
                var sheet = characters[character];
                for (int row = 0; row < 4; row++)
                for (int column = 0; column < 4; column++)
                {
                    int left = column * sheet.width / 4;
                    int right = (column + 1) * sheet.width / 4;
                    int bottom = (3 - row) * sheet.height / 4;
                    int top = (4 - row) * sheet.height / 4;
                    string frame = "Art/Character" + character + "_" + Directions[row] + "_" + column + ".png";
                    WriteCrop(sheet, left, bottom, right - left, top - bottom, frame, 280, imports);
                    if (row == 0 && column == 0)
                    {
                        WriteCrop(sheet, left, bottom, right - left, top - bottom,
                            "Art/Portrait" + character + ".png", 280, imports);
                        if (character == 0)
                            WriteCrop(sheet, left, bottom, right - left, top - bottom,
                                "Art/Player.png", 280, imports);
                    }
                    if (character == 0)
                        WriteCrop(sheet, left, bottom, right - left, top - bottom,
                            "Art/Player_" + Directions[row] + "_" + column + ".png", 280, imports);
                }
            }

            var rects = new[]
            {
                new Rect(0, 760, 510, 390),
                new Rect(510, 760, 759, 480),
                new Rect(0, 0, 510, 760)
            };
            var names = new[] { "Enemy", "EnemyMoth", "EnemyBeetle" };
            var pixelsPerUnit = new[] { 380f, 700f, 600f };
            float scaleX = monsters.width / 1269f, scaleY = monsters.height / 1240f;
            for (int i = 0; i < names.Length; i++)
            {
                var area = rects[i];
                WriteCrop(monsters, Mathf.RoundToInt(area.x * scaleX), Mathf.RoundToInt(area.y * scaleY),
                    Mathf.RoundToInt(area.width * scaleX), Mathf.RoundToInt(area.height * scaleY),
                    "Art/" + names[i] + ".png", pixelsPerUnit[i] * scaleX, imports);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var pair in imports)
                {
                    var importer = (TextureImporter)AssetImporter.GetAtPath(pair.Key);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = pair.Value;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }

            for (int i = 0; i < characters.Length; i++)
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterData>(Root + "Data/Character_" + i + ".asset");
                if (data == null) continue;
                data.sprite = Sprite("Character" + i + "_Down_0");
                data.portrait = Sprite("Portrait" + i);
                data.downFrames = Frames(i, 0);
                data.upFrames = Frames(i, 1);
                data.leftFrames = Frames(i, 2);
                data.rightFrames = Frames(i, 3);
                EditorUtility.SetDirty(data);
            }

            RefreshPrefab("Player", "Player");
            foreach (string name in names) RefreshPrefab(name, name);
            AssetDatabase.SaveAssets();
            Debug.Log("Current art refreshed: " + imports.Count + " sprites, 4 prefab previews.");
        }

        static Texture2D Load(string relative)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + relative);
            if (texture == null || !texture.isReadable)
                throw new InvalidOperationException("Missing or unreadable current sheet: " + relative);
            return texture;
        }

        static void WriteCrop(Texture2D sheet, int x, int y, int width, int height,
            string relative, float pixelsPerUnit, Dictionary<string, float> imports)
        {
            if (width <= 0 || height <= 0 || x < 0 || y < 0 || x + width > sheet.width || y + height > sheet.height)
                throw new InvalidOperationException("Invalid art crop: " + relative);
            var crop = new Texture2D(width, height, TextureFormat.RGBA32, false);
            crop.SetPixels(sheet.GetPixels(x, y, width, height));
            crop.Apply();
            string path = Root + relative;
            File.WriteAllBytes(path, crop.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(crop);
            imports[path] = pixelsPerUnit;
        }

        static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/" + name + ".png");
        static Sprite[] Frames(int character, int direction)
        {
            var frames = new Sprite[4];
            for (int frame = 0; frame < 4; frame++)
                frames[frame] = Sprite("Character" + character + "_" + Directions[direction] + "_" + frame);
            return frames;
        }

        static void RefreshPrefab(string prefabName, string spriteName)
        {
            string path = Root + "Prefabs/" + prefabName + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var renderer = root.GetComponent<SpriteRenderer>();
                renderer.sprite = Sprite(spriteName);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
