using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools
{
    public static class Validator
    {
        private static ILogger Logger => Debug.unityLogger;
        
        [MenuItem("Tools/Validate/Find Missing Components in Scenes")]
        public static void FindMissingComponentsInScenes()
        {
            foreach (var scene in GetAllProjectScenes())
            {
                foreach (var gameObject in GetAllGameObjects(scene))
                {
                    var hasMissingScripts = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject) > 0;

                    if (hasMissingScripts)
                        Logger.LogError("Validator", $"GameObject '{gameObject.name}' from Scene '{scene.name}' had missing scripts which were removed.");
                }
            }
        }

        [MenuItem("Tools/Validate/Find Missing Components in Prefabs")]
        public static void FindMissingComponentsInPrefabs()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            foreach (var guid in prefabGuids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                    continue;

                CheckPrefabForMissingComponents(prefab, prefabPath);
            }
        }
        
        private static IEnumerable<Scene> GetAllProjectScenes()
        {
            var scenePaths = AssetDatabase
                .FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath);

            foreach (var scenePath in scenePaths)
            {
                var scene = SceneManager.GetSceneByPath(scenePath);

                if (scene.isLoaded)
                    yield return scene;
                else
                {
                    var openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    yield return openedScene;

                    EditorSceneManager.CloseScene(openedScene, true);
                }
            }
        }

        private static IEnumerable<GameObject> GetAllGameObjects(Scene scene)
        {
            var gameObjectsQueue = new Queue<GameObject>(scene.GetRootGameObjects());

            while (gameObjectsQueue.Count > 0)
            {
                var gameObject = gameObjectsQueue.Dequeue();
                yield return gameObject;

                foreach (Transform child in gameObject.transform)
                {
                    gameObjectsQueue.Enqueue(child.gameObject);
                }
            }
        }
        
        private static void CheckPrefabForMissingComponents(GameObject prefab, string prefabPath)
        {
            var allObjects = new List<GameObject> { prefab };
            GetAllChildrenRecursive(prefab.transform, allObjects);

            foreach (var obj in allObjects)
            {
                var missingScriptsCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj);

                if (missingScriptsCount > 0)
                {
                    Logger.LogError("Validator", $"Prefab '{prefabPath}' has {missingScriptsCount} missing script(s) on GameObject '{obj.name}'.");
                }
            }
        }

        private static void GetAllChildrenRecursive(Transform parent, List<GameObject> result)
        {
            foreach (Transform child in parent)
            {
                result.Add(child.gameObject);
                GetAllChildrenRecursive(child, result);
            }
        }
    }
}