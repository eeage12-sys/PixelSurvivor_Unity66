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

        [MenuItem("Tools/Pixel Survivor/Build Portfolio Windows x64", false, 31)]
        public static void PortfolioWindows()
        { BuildPortfolio(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64,"Builds/PixelSurvivorPortfolioWindows/PixelSurvivor.exe"); }

        [MenuItem("Tools/Pixel Survivor/Build Portfolio WebGL", false, 32)]
        public static void PortfolioWeb()
        { BuildPortfolio(BuildTargetGroup.WebGL,BuildTarget.WebGL,"Builds/PixelSurvivorPortfolioWeb"); }

        [MenuItem("Tools/Pixel Survivor/Build Portfolio Android APK", false, 33)]
        public static void PortfolioAndroid()
        {
            UIOrientation previous=PlayerSettings.defaultInterfaceOrientation;
            try
            {
                PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
                BuildPortfolio(BuildTargetGroup.Android,BuildTarget.Android,"Builds/PixelSurvivorPortfolioAndroid/PixelSurvivor.apk");
            }
            finally { PlayerSettings.defaultInterfaceOrientation=previous; }
        }

        static void BuildPortfolio(BuildTargetGroup group,BuildTarget target,string location)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new System.InvalidOperationException("Exit Play Mode before building.");
            if(!BuildPipeline.IsBuildTargetSupported(group,target)) throw new System.InvalidOperationException("Install build support for "+target+" in Unity Hub first.");
            if(!File.Exists(SurvivorSetup.ScenePath)) throw new System.IO.FileNotFoundException("Run Pixel Survivor Setup first.",SurvivorSetup.ScenePath);
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(SurvivorSetup.ScenePath);
            SurvivorSetup.ValidateScene();
            Directory.CreateDirectory(Path.GetDirectoryName(location));
            BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]{SurvivorSetup.ScenePath},
                locationPathName=location,
                target=target,
                options=BuildOptions.None,
                extraScriptingDefines=new[]{"PIXELSURVIVOR_PORTFOLIO"}
            });
            if(report.summary.result!=BuildResult.Succeeded)
                throw new System.InvalidOperationException("Portfolio build failed for "+target+". Read the first red Console message.");
            Debug.Log("Portfolio build ready at "+location+"; all stages unlocked in this build.");
        }
    }
}
