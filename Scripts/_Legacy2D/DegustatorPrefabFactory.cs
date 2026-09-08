// DegustatorPrefabFactory.cs
// EDITOR SCRIPT — положи в Assets/Editor/
// Создаёт готовые UI-префабы (Image, Canvas) через меню Degustation.
//
// Меню: Degustation → Create Prefab → [NPC / Food Item / Shop Item / Inventory Item]
// Сохраняет в Assets/Prefabs/Degustation/

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Degustation;

public static class DegustatorPrefabFactory
{
    const string ROOT = "Assets/Prefabs/Degustation";

    // ── NPC ───────────────────────────────────────────────────────
    [MenuItem("Degustation/Create Prefab/NPC")]
    static void CreateNPC()
    {
        var root = new GameObject("NPC_Prefab");
        var img  = root.AddComponent<Image>();       // Canvas UI
        img.raycastTarget = true;
        img.preserveAspect = true;
        var slot      = root.AddComponent<ItemSlot>();
        slot.slotType = ItemSlot.SlotType.NPC;
        SavePrefab(root, ROOT + "/NPC", "NPC_Prefab");
    }

    // ── Food Item ─────────────────────────────────────────────────
    [MenuItem("Degustation/Create Prefab/Food Item")]
    static void CreateFoodItem()
    {
        var root = new GameObject("FoodItem_Prefab");
        var img  = root.AddComponent<Image>();
        img.raycastTarget  = true;
        img.preserveAspect = true;
        var slot      = root.AddComponent<ItemSlot>();
        slot.slotType = ItemSlot.SlotType.Food;
        root.AddComponent<FoodDragHandler>();    // drag & drop между слотами
        root.AddComponent<TastedItem>();
        SavePrefab(root, ROOT + "/Food", "FoodItem_Prefab");
    }

    // ── Shop Item ─────────────────────────────────────────────────
    [MenuItem("Degustation/Create Prefab/Shop Item")]
    static void CreateShopItem()
    {
        var root = new GameObject("ShopItem_Prefab");
        var img  = root.AddComponent<Image>();
        img.raycastTarget  = true;
        img.preserveAspect = true;
        root.AddComponent<ShopItemView>();
        SavePrefab(root, ROOT + "/Shop", "ShopItem_Prefab");
    }

    // ── Inventory Item ────────────────────────────────────────────
    [MenuItem("Degustation/Create Prefab/Inventory Item")]
    static void CreateInventoryItem()
    {
        var root = new GameObject("InventoryItem_Prefab");
        var img  = root.AddComponent<Image>();
        img.raycastTarget  = true;
        img.preserveAspect = true;
        root.AddComponent<InventoryItemView>();
        SavePrefab(root, ROOT + "/Inventory", "InventoryItem_Prefab");
    }

    // ── Organ State Icon ──────────────────────────────────────────
    [MenuItem("Degustation/Create Prefab/Organ State Icon")]
    static void CreateOrganIcon()
    {
        var root = new GameObject("OrganIcon_Prefab");
        var img  = root.AddComponent<Image>();
        img.raycastTarget  = false; // иконки органов не кликабельны
        img.preserveAspect = true;
        var slot      = root.AddComponent<ItemSlot>();
        slot.slotType = ItemSlot.SlotType.Organ;
        SavePrefab(root, ROOT + "/Organs", "OrganIcon_Prefab");
    }

    // ── Утилита ───────────────────────────────────────────────────
    static void SavePrefab(GameObject go, string folder, string fileName)
    {
        EnsureFolder("Assets/Prefabs", "Degustation");
        EnsureFolder("Assets/Prefabs/Degustation", GetLeaf(folder));

        string path   = $"{folder}/{fileName}.prefab";
        var    prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"[PrefabFactory] Создан: {path}");
    }

    static void EnsureFolder(string parent, string name)
    {
        string full = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, name);
    }

    static string GetLeaf(string path) { var p = path.Split('/'); return p[p.Length - 1]; }
}
#endif
