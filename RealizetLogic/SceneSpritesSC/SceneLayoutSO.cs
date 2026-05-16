// SceneLayoutSO.cs
// SO-конфиг: описывает ВСЕ слоты на сцене — тип, дефолтные данные.
// Позиции берутся из реальных Transform-точек (пустые объекты на сцене).
// Создать: ПКМ → Degustation → Scene Layout

using System;
using System.Collections.Generic;
using UnityEngine;
using Degustation;

[CreateAssetMenu(fileName = "NewSceneLayout", menuName = "Degustation/Scene Layout")]
public class SceneLayoutSO : ScriptableObject
{
    // ─── Что может быть в слоте ───────────────────────────────────
    public enum SlotType
    {
        Food,    // FoodData SO → спрайт + drag & drop
        NPC,     // NPcCharacter SO → Expression-спрайты
        Organ    // SenseStatsData → спрайт органа по HP
    }

    // ─── Один слот ────────────────────────────────────────────────
    [Serializable]
    public class SlotConfig
    {
        [Tooltip("Как называть слот (для дебага)")]
        public string label = "Slot";

        public SlotType slotType = SlotType.Food;

        // Только один из трёх будет заполнен в зависимости от slotType
        [Header("Food")]
        public FoodData foodData;

        [Header("NPC")]
        public NPcCharacter npcData;

        [Header("Organ — какой орган отображать")]
        public SenseType organSense = SenseType.Vision;

        [Tooltip("Ссылка на SO игрока с HP органов")]
        public SenseStatsData playerStats;
    }

    // ─── Список слотов ────────────────────────────────────────────
    [Header("Слоты сцены")]
    public List<SlotConfig> slots = new();
}
