using UnityEditor;
using UnityEngine;

namespace PixelSurvivor.Editor
{
    public sealed class RedesignImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Resources/Redesign/") ||
                assetPath.EndsWith("/BoneCrawler.png") || assetPath.EndsWith("/EmberWraith.png")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
        }
    }
}
