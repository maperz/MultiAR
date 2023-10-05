using UnityEditor;
using UnityEngine;

namespace MultiAR.Editor
{
    public class FindMissingScripts : EditorWindow
    {
        static int _goCount, _componentsCount, _missingCount;

        [MenuItem("MultiAR/Tools/FindMissingScripts")]
        public static void ShowWindow()
        {
            GetWindow(typeof(FindMissingScripts));
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Find Missing Scripts in selected GameObjects"))
            {
                FindInSelected();
            }

            if (GUILayout.Button("Find Missing Scripts"))
            {
                FindAll();
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Component Scanned:");
                EditorGUILayout.LabelField("" + (_componentsCount == -1 ? "---" : _componentsCount.ToString()));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Object Scanned:");
                EditorGUILayout.LabelField("" + (_goCount == -1 ? "---" : _goCount.ToString()));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Possible Missing Scripts:");
                EditorGUILayout.LabelField("" + (_missingCount == -1 ? "---" : _missingCount.ToString()));
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void FindAll()
        {
            _componentsCount = 0;
            _goCount = 0;
            _missingCount = 0;

            string[] assetsPaths = AssetDatabase.GetAllAssetPaths();

            foreach (string assetPath in assetsPaths)
            {
                Object[] data = LoadAllAssetsAtPath(assetPath);
                foreach (Object o in data)
                {
                    if (o != null)
                    {
                        if (o is GameObject)
                        {
                            FindInGameObject((GameObject)o);
                        }
                    }
                }
            }

            Debug.Log($"Searched {_goCount} GameObjects, {_componentsCount} components, found {_missingCount} missing");
        }

        private static Object[] LoadAllAssetsAtPath(string assetPath)
        {
            return typeof(SceneAsset) == AssetDatabase.GetMainAssetTypeAtPath(assetPath)
                ?
                // prevent error "Do not use readobjectthreaded on scene objects!"
                new[] {AssetDatabase.LoadMainAssetAtPath(assetPath)}
                : AssetDatabase.LoadAllAssetsAtPath(assetPath);
        }

        private static void FindInSelected()
        {
            GameObject[] go = Selection.gameObjects;
            _goCount = 0;
            _componentsCount = 0;
            _missingCount = 0;
            foreach (GameObject g in go)
            {
                FindInGameObject(g);
            }

            Debug.Log($"Searched {_goCount} GameObjects, {_componentsCount} components, found {_missingCount} missing");
        }

        private static void FindInGameObject(GameObject gameObject)
        {
            _goCount++;
            var components = gameObject.GetComponents<Component>();
            for (var componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                _componentsCount++;
                if (components[componentIndex] == null)
                {
                    _missingCount++;
                    var gameObjectName = gameObject.name;
                    var transform = gameObject.transform;
                    while (transform.parent != null)
                    {
                        var parent = transform.parent;
                        gameObjectName = parent.name + "/" + gameObjectName;
                        transform = parent;
                    }

                    Debug.Log(gameObjectName + " has an empty script attached in position: " + componentIndex,
                        gameObject);
                }
            }

            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in gameObject.transform)
            {
                //Debug.Log("Searching " + childT.name  + " " );
                FindInGameObject(childT.gameObject);
            }
        }
    }
}
