// Degustator3DPrefabFactory.cs
// EDITOR ONLY — лежит в папке Editor, в билд не попадает.
//
// Меню: Degustation → 3D → ...
//   • Create Food Prefab Template — болванка 3D-блюда (меш кидаешь сам)
//   • Create Placement Points     — раскидать точки на столе в ряд
//   • Setup Inspection Rig        — создать HoldAnchor + контроллеры на сцене

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Degustation.EditorTools
{
    public static class Degustator3DPrefabFactory
    {
        private const string PrefabRoot = "Assets/Prefabs/Degustation3D";

        // ────────────────────────────────────────────────────────
        [MenuItem("Degustation/3D/Create Food Prefab Template")]
        private static void CreateFoodPrefab()
        {
            var root = new GameObject("Food3D_Template");

            // временный визуал — замени на свою модель из Blender
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Mesh";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * 0.15f;

            root.AddComponent<FoodItem>();
            root.AddComponent<FoodHighlight>();
            root.AddComponent<Manipulation>();

            SavePrefab(root, PrefabRoot, "Food3D_Template");
        }

        // ────────────────────────────────────────────────────────
        [MenuItem("Degustation/3D/Create Placement Points (5 in a row)")]
        private static void CreatePlacementPoints()
        {
            var parent = Selection.activeGameObject;
            var root   = new GameObject("PlacementPoints");

            if (parent != null) root.transform.SetParent(parent.transform, false);
            Undo.RegisterCreatedObjectUndo(root, "Create Placement Points");

            const int   count   = 5;
            const float spacing = 0.35f;

            for (int i = 0; i < count; i++)
            {
                var p = new GameObject($"Point_{i + 1}");
                p.transform.SetParent(root.transform, false);
                p.transform.localPosition = new Vector3((i - (count - 1) * 0.5f) * spacing, 0f, 0f);
                p.AddComponent<FoodPlacementPoint>();
            }

            Selection.activeGameObject = root;
            Debug.Log("[Degustation] Точки созданы. Двигай их по столу — гизмо покажет позицию блюда.");
        }

        // ────────────────────────────────────────────────────────
        [MenuItem("Degustation/3D/Setup Inspection Rig")]
        private static void SetupRig()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[Degustation] На сцене нет камеры с тегом MainCamera.");
                return;
            }

            var anchor = cam.transform.Find("HoldAnchor");
            if (anchor == null)
            {
                var go = new GameObject("HoldAnchor");
                Undo.RegisterCreatedObjectUndo(go, "Create HoldAnchor");
                go.transform.SetParent(cam.transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, 0.55f);
                anchor = go.transform;
            }

            var controller = Object.FindObjectOfType<InspectionController>();
            if (controller == null)
            {
                var go = new GameObject("InspectionController");
                Undo.RegisterCreatedObjectUndo(go, "Create InspectionController");
                controller = go.AddComponent<InspectionController>();
                go.AddComponent<PlayerInteractor>();
            }

            Selection.activeObject = controller;
            Debug.Log("[Degustation] Риг готов. Назначь в инспекторе: targetCamera, holdAnchor, slidePanel, disableWhileInspecting.");
        }

        // ────────────────────────────────────────────────────────
        private static void SavePrefab(GameObject go, string folder, string fileName)
        {
            EnsureFolder(folder);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}.prefab");

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            AssetDatabase.Refresh();
            Debug.Log($"[Degustation] Префаб создан: {path}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
