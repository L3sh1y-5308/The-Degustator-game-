// TastedItemSpawner.cs
// Отвечает только за создание и очистку предметов на сцене.
// После Instantiate вызывает TastedItem.Init() с рандомизированным RuntimeFood.

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    public class TastedItemSpawner : MonoBehaviour
    {
        [Header("Спавн")]
        [Tooltip("Шаблоны FoodData — из них роллится RuntimeFood")]
        public List<FoodData> foodTemplates = new();

        [Tooltip("Префабы с компонентом TastedItem")]
        public List<GameObject> spawnablePrefabs = new();

        [Tooltip("Точки спавна на сцене")]
        public List<Transform> spawnPoints = new();

        [Range(1, 10)] public int minSpawnCount = 2;
        [Range(1, 10)] public int maxSpawnCount = 5;

        [Header("SO-системы (для инициализации TastedItem)")]
        public SenseStatsData        senseStats;
        public InspectionDamageTable damageTable;

        // Активные предметы текущего раунда
        private List<TastedItem> _activeItems = new();

        // ════════════════════════════════════════════════════════════
        // Спавн
        // ════════════════════════════════════════════════════════════
        public List<TastedItem> SpawnRandomItems()
        {
            ClearItems();

            if (spawnablePrefabs.Count == 0)
            {
                Debug.LogWarning("[TastedItemSpawner] Нет префабов!");
                return _activeItems;
            }

            if (foodTemplates.Count == 0)
            {
                Debug.LogWarning("[TastedItemSpawner] Нет FoodData-шаблонов!");
                return _activeItems;
            }

            int count = Random.Range(minSpawnCount, maxSpawnCount + 1);

            List<Transform> shuffled = new(spawnPoints);
            Shuffle(shuffled);

            for (int i = 0; i < count; i++)
            {
                // Рандомный префаб
                GameObject prefab = spawnablePrefabs[Random.Range(0, spawnablePrefabs.Count)];

                // Рандомный шаблон еды → роллим RuntimeFood
                // Каждый экземпляр получает уникальные рандомные статы
                FoodData    template    = foodTemplates[Random.Range(0, foodTemplates.Count)];
                RuntimeFood runtimeFood = template.Roll();

                // Позиция
                Vector3 pos = (i < shuffled.Count) ? shuffled[i].position : Vector3.zero;

                GameObject obj  = Instantiate(prefab, pos, Quaternion.identity);
                TastedItem item = obj.GetComponent<TastedItem>();

                if (item != null)
                {
                    item.Init(runtimeFood, senseStats, damageTable);
                    _activeItems.Add(item);
                }
                else
                {
                    Debug.LogWarning($"[TastedItemSpawner] У '{prefab.name}' нет TastedItem!");
                    Destroy(obj);
                }
            }

            Debug.Log($"[TastedItemSpawner] Заспавнено {_activeItems.Count} предметов.");
            return _activeItems;
        }

        // ════════════════════════════════════════════════════════════
        // Очистка
        // ════════════════════════════════════════════════════════════
        public void ClearItems()
        {
            foreach (var item in _activeItems)
                if (item != null) Destroy(item.gameObject);
            _activeItems.Clear();
        }

        public List<TastedItem> GetActiveItems() => new(_activeItems);

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
