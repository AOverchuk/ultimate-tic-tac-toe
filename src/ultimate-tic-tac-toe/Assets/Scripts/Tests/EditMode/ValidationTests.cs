using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using FluentAssertions;

namespace Tests.EditMode
{
    public class ValidationTests
    {
        [TestCaseSource(nameof(AllScenesPaths))]
        public void AllGameObjectsShouldNoHaveMissingScriptsInScenes(string scenePath)
        {
            var openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            var gameObjectsWithMissingScripts = new List<string>();
            
            foreach (var gameObject in GetAllGameObjects(openedScene))
            {
                if (HasMissingComponents(gameObject)) 
                    gameObjectsWithMissingScripts.Add(gameObject.name);
            }
            
            EditorSceneManager.CloseScene(openedScene, true);
            gameObjectsWithMissingScripts.Should().BeEmpty();
        }

        /*[Test]
        public void AllGameObjectsShouldNoHaveMissingScriptsInPrefabs() => FindMissingComponentsInPrefabs();*/

        private static void FindMissingComponentsInPrefabs()
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
        
        private static bool HasMissingComponents(GameObject gameObject) => 
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject) > 0;

        private static IEnumerable<string> AllScenesPaths() =>
            AssetDatabase
                .FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath);

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

                //if (missingScriptsCount > 0) 
                    //Logger.LogError("Validator", $"Prefab '{prefabPath}' has {missingScriptsCount} missing script(s) on GameObject '{obj.name}'.");
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
