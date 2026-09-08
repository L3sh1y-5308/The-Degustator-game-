// SceneEventHand.cs
// MonoBehaviour — читает список слотов, спавнит ItemSlot-префабы в Canvas.
// Все слоты — дочерние объекты Canvas, используют Image.
//
// Настройка в Inspector:
//   slotPrefab    → префаб с Image + ItemSlot + (для еды) FoodDragHandler
//   anchorPoints  → RectTransform-точки (пустые UI объекты) в Canvas
//   slotConfigs   → список настроек для каждого слота

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Degustation;

public class SceneEventHand : MonoBehaviour
{
    public enum ItemTag { Food, NPC, Organ }

    // ── Конфиг одного слота ───────────────────────────────────────
    [Serializable]
    public class SlotConfig
    {
        public string  label    = "Slot";
        public ItemTag slotType = ItemTag.Food;

        [Header("Food")]
        public FoodData foodData;

        [Header("NPC")]
        public NPcCharacter npcData;

        [Header("Organ")]
        public SenseType      organSense   = SenseType.Vision;
        public SenseStatsData playerStats;
    }

    // ── Inspector ─────────────────────────────────────────────────
    [Header("Префаб слота (Image + ItemSlot на Canvas)")]
    public GameObject slotPrefab;

    [Header("Точки спавна — RectTransform внутри Canvas")]
    public RectTransform[] anchorPoints;

    [Header("Конфиги слотов (порядок = порядок anchorPoints)")]
    public List<SlotConfig> slotConfigs = new();

    // ── Рантайм ───────────────────────────────────────────────────
    public List<ItemSlot> SpawnedSlots { get; private set; } = new();

    private void Start() => SpawnSlots();

    public void SpawnSlots()
    {
        // Чистим старые
        foreach (var s in SpawnedSlots)
            if (s != null) Destroy(s.gameObject);
        SpawnedSlots.Clear();

        if (slotPrefab == null)
        {
            Debug.LogWarning("[SceneEventHand] slotPrefab не назначен!");
            return;
        }

        int count = Mathf.Min(slotConfigs.Count, anchorPoints.Length);

        for (int i = 0; i < count; i++)
        {
            var cfg   = slotConfigs[i];
            var point = anchorPoints[i];

            // Спавн как дочерний объект той же панели где anchorPoint
            var go   = Instantiate(slotPrefab, point.parent);
            var rect = go.GetComponent<RectTransform>();

            // Ставим в позицию точки
            rect.anchoredPosition = point.anchoredPosition;
            rect.sizeDelta        = point.sizeDelta;

            var slot = go.GetComponent<ItemSlot>();
            if (slot == null)
            {
                Debug.LogError($"[SceneEventHand] Нет ItemSlot на префабе! Слот [{cfg.label}]");
                Destroy(go);
                continue;
            }

            // Применяем конфиг
            slot.name     = $"[Slot] {cfg.label}";
            slot.slotType = (ItemSlot.SlotType)(int)cfg.slotType;

            switch (cfg.slotType)
            {
                case ItemTag.Food:
                    slot.SetFood(cfg.foodData);
                    break;

                case ItemTag.NPC:
                    slot.npcData = cfg.npcData;
                    slot.SetNPCExpression(NPcCharacter.NPCFaceExpression.Neutral);
                    break;

                case ItemTag.Organ:
                    slot.organSense  = cfg.organSense;
                    slot.playerStats = cfg.playerStats;
                    slot.RefreshOrganSprite();
                    break;
            }

            SpawnedSlots.Add(slot);
        }

        Debug.Log($"[SceneEventHand] Заспавнено {SpawnedSlots.Count} слотов.");
    }

    // ── Хелперы ───────────────────────────────────────────────────
    public List<ItemSlot> GetFoodSlots() =>
        SpawnedSlots.FindAll(s => s.slotType == ItemSlot.SlotType.Food);

    public ItemSlot GetNPCSlot(string label) =>
        SpawnedSlots.Find(s => s.slotType == ItemSlot.SlotType.NPC && s.name.Contains(label));

    public ItemSlot GetOrganSlot(SenseType sense) =>
        SpawnedSlots.Find(s => s.slotType == ItemSlot.SlotType.Organ && s.organSense == sense);

    // Сменить эмоцию NPC по метке слота
    public void SetNPCExpression(string slotLabel, NPcCharacter.NPCFaceExpression expression)
    {
        var slot = GetNPCSlot(slotLabel);
        if (slot != null) slot.SetNPCExpression(expression);
    }
}
