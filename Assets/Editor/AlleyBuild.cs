using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TrashRac.Editor
{
    public static class AlleyBuild
    {
        private const string ScenePath = "Assets/Scenes/BackAlley.unity";

        public static void BuildWeb()
        {
            EnsureScene();
            PlayerSettings.productName = "Trash Rac";
            PlayerSettings.companyName = "Hambone Software";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.BuildTimes;
            PlayerSettings.WebGL.template = "PROJECT:TrashRac";
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            AssetDatabase.SaveAssets();

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Web",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Web build ended with " + report.summary.result);
            File.WriteAllText("Builds/Web/.nojekyll", string.Empty);
            Debug.Log($"Trash Rac Web build complete ({report.summary.totalSize:N0} bytes).");
        }

        private static void EnsureScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory("Assets/Scenes");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("Back Alley Board").AddComponent<AlleyGame>();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
