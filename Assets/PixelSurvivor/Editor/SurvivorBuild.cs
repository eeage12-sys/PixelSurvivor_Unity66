using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PixelSurvivor.Editor
{
    public static class SurvivorBuild
    {
        [MenuItem("Tools/Pixel Survivor/Build Windows x64", false, 30)]
        public static void Windows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            { Debug.LogError("Install Windows Build Support for Unity 6000.6.6f1 in Unity Hub first."); return; }
            if (!File.Exists(SurvivorSetup.ScenePath)) { Debug.LogError("Run Pixel Survivor Setup first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(SurvivorSetup.ScenePath);
            SurvivorSetup.ValidateScene();
            Directory.CreateDirectory("Builds/PixelSurvivor");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { SurvivorSetup.ScenePath },
                locationPathName = "Builds/PixelSurvivor/PixelSurvivor.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.InvalidOperationException("Windows build failed. Read the first red Console message.");
            Debug.Log("Windows build ready in Builds/PixelSurvivor. Distribute the whole folder, not only the .exe.");
        }
    }
}
