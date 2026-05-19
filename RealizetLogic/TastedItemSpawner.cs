// TastedItemSpawner.cs
// Заполняет UI ItemSlot-ы едой из FoodData шаблонов.
// Не спавнит 3D объекты — работает только с Canvas слотами.

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    public class TastedItemSpawner : MonoBehaviour
    {
        [Header("Слоты на столе (UI ItemSlot)")]
        [Tooltip("Все Food-слоты на сцене — перетащи из иерархии")]
        public List<ItemSlot> foodSlots = new();

        [Header("Еда")]
        [Tooltip("Шаблоны FoodData — из них роллится RuntimeFood")]
        public List<FoodData> foodTemplates = new();

        [Range(1, 10)] public int minSpawnCount = 2;
        [Range(1, 10)] public int maxSpawnCount = 5;

        // Активные слоты текущего раунда (те что заполнены)
        private List<ItemSlot> _activeSlots = new();

        // ════════════════════════════════════════════════════════════
        // Спавн
        // ════════════════════════════════════════════════════════════
        public List<ItemSlot> SpawnRandomItems()
        {
            ClearItems();

            if (foodSlots.Count == 0)
            {
                Debug.LogWarning("[TastedItemSpawner] Нет слотов!");
                return _activeSlots;
            }

            if (foodTemplates.Count == 0)
            {
                Debug.LogWarning("[TastedItemSpawner] Нет FoodData-шаблонов!");
                return _activeSlots;
            }

            int count = Random.Range(minSpawnCount, Mathf.Min(maxSpawnCount, foodSlots.Count) + 1);

            // Перемешиваем слоты и шаблоны
            List<ItemSlot> shuffledSlots = new(foodSlots);
            List<FoodData> shuffledFood  = new(foodTemplates);
            Shuffle(shuffledSlots);
            Shuffle(shuffledFood);

            // Сначала очищаем все слоты
            foreach (var slot in foodSlots)
                slot.ClearSlot();

            for (int i = 0; i < count; i++)
            {
                FoodData food = shuffledFood[i % shuffledFood.Count];
                shuffledSlots[i].SetFood(food);
                _activeSlots.Add(shuffledSlots[i]);
            }

            // Сортируем активные слоты слева направо по X
            _activeSlots.Sort((a, b) =>
            {
                var ra = a.GetComponent<RectTransform>();
                var rb = b.GetComponent<RectTransform>();
                return ra.anchoredPosition.x.CompareTo(rb.anchoredPosition.x);
            });

            Debug.Log($"[TastedItemSpawner] Заполнено {_activeSlots.Count} слотов.");
            return _activeSlots;
        }

        // ════════════════════════════════════════════════════════════
        // Очистка
        // ════════════════════════════════════════════════════════════
        public void ClearItems()
        {
            foreach (var slot in foodSlots)
                if (slot != null) slot.ClearSlot();
            _activeSlots.Clear();
        }

        // Активные слоты отсортированы слева направо
        public List<ItemSlot> GetActiveSlots() => new(_activeSlots);

        // ════════════════════════════════════════════════════════════
        // Fisher-Yates shuffle
        // ════════════════════════════════════════════════════════════
        void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
