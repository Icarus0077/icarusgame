using IcarusGame.CleanDuel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IcarusGame.EditorTools
{
    public static class CleanDuelSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/CleanDuelScene.unity";
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string SampleSceneBackupPath = "Assets/Scenes/SampleScene_PreZeroRebuild.unity";

        [MenuItem("Icarus/Rebuild Clean Duel Scene")]
        public static void BuildDefaultScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "CleanDuelScene";

            GameObject runtime = new GameObject("Clean Duel Bootstrap");
            runtime.AddComponent<CleanDuelBootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Clean duel scene rebuilt at " + ScenePath);
        }

        [MenuItem("Icarus/Rebuild Sample Scene From Zero")]
        public static void BuildSampleSceneFromZero()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SampleScenePath) != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(SampleSceneBackupPath) == null)
            {
                AssetDatabase.CopyAsset(SampleScenePath, SampleSceneBackupPath);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SampleScene";

            GameObject runtime = new GameObject("Clean Duel Bootstrap");
            runtime.AddComponent<CleanDuelBootstrap>();

            EditorSceneManager.SaveScene(scene, SampleScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SampleScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SampleScene rebuilt from zero at " + SampleScenePath);
        }
    }
}