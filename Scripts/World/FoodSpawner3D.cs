// FoodSpawner3D.cs
// Спавнит 3D-модели блюд (FoodData.prefab3D) по точкам на столе.
// Замена TastedItemSpawner, который работал с Canvas-слотами.
//
// НАСТРОЙКА:
//   1. Повесь на объект "Table" (или пустышку "FoodSpawner")
//   2. Раскидай по столу пустые GameObject с FoodPlacementPoint
//   3. Если спавнер — родитель точек, они подтянутся автоматически
//   4. Заполни foodTemplates списком FoodData

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    public class FoodSpawner3D : MonoBehaviour
    {
        [Header("Точки на столе")]
        [SerializeField] private List<FoodPlacementPoint> placementPoints = new();

        [Tooltip("Если список пуст — собрать точки из детей этого объекта")]
        [SerializeField] private bool autoCollectChildPoints = true;

        [Header("Еда")]
        [SerializeField] private List<FoodData> foodTemplates = new();

        [SerializeField, Range(1, 10)] private int minSpawnCount = 2;
        [SerializeField, Range(1, 10)] private int maxSpawnCount = 5;

        [Tooltip("Разрешить одинаковые блюда в одном раунде")]
        [SerializeField] private bool allowDuplicates = false;

        [Header("Сортировка слева направо")]
        [Tooltip("Камера, относительно которой считается «слева направо». Пусто = Camera.main")]
        [SerializeField] private Camera sortCamera;

        private readonly List<FoodItem> _active = new();

        /// Активные блюда раунда, отсортированы слева направо с точки зрения игрока
        public IReadOnlyList<FoodItem> ActiveItems => _active;
        public List<FoodItem> GetActiveItems() => new(_active);

        private void Awake()
        {
            if (autoCollectChildPoints && placementPoints.Count == 0)
                placementPoints.AddRange(GetComponentsInChildren<FoodPlacementPoint>(true));
        }

        // ════════════════════════════════════════════════════════
        // Спавн раунда
        // ════════════════════════════════════════════════════════
        public List<FoodItem> SpawnRandomItems()
        {
            ClearItems();

            if (placementPoints.Count == 0)
            {
                Debug.LogWarning("[FoodSpawner3D] Нет точек размещения на столе!");
                return _active;
            }
            if (foodTemplates.Count == 0)
            {
                Debug.LogWarning("[FoodSpawner3D] Нет FoodData-шаблонов!");
                return _active;
            }

            int maxByPoints = Mathf.Min(maxSpawnCount, placementPoints.Count);
            if (!allowDuplicates) maxByPoints = Mathf.Min(maxByPoints, foodTemplates.Count);

            int count = Random.Range(Mathf.Min(minSpawnCount, maxByPoints), maxByPoints + 1);

            var points = new List<FoodPlacementPoint>(placementPoints);
            var foods  = new List<FoodData>(foodTemplates);
            Shuffle(points);
            Shuffle(foods);

            for (int i = 0; i < count; i++)
            {
                var point = points[i];
                var food  = foods[i % foods.Count];

                if (food == null || food.prefab3D == null)
                {
                    Debug.LogWarning($"[FoodSpawner3D] У блюда '{(food != null ? food.foodName : "null")}' не назначен prefab3D — пропускаю.");
                    continue;
                }

                var go = Instantiate(
                    food.prefab3D,
                    point.SpawnPosition + food.placementOffset,
                    point.SpawnRotation,
                    point.transform);

                var item = go.GetComponent<FoodItem>();
                if (item == null) item = go.AddComponent<FoodItem>();

                item.Init(food, food.Roll(), point);
                point.Occupy(item);
                _active.Add(item);
            }

            SortLeftToRight();
            Debug.Log($"[FoodSpawner3D] Заспавнено блюд: {_active.Count}");
            return _active;
        }

        // ════════════════════════════════════════════════════════
        // Очистка стола
        // ════════════════════════════════════════════════════════
        public void ClearItems()
        {
            foreach (var item in _active)
            {
                if (item == null) continue;
                if (item.HomePoint != null) item.HomePoint.Release();
                Destroy(item.gameObject);
            }
            _active.Clear();

            // страховка: снимаем занятость со всех точек
            foreach (var p in placementPoints)
                if (p != null) p.Release();
        }

        // ════════════════════════════════════════════════════════
        // Порядок проверки: слева направо ГЛАЗАМИ ИГРОКА,
        // поэтому переводим позицию в локальные координаты камеры.
        // ════════════════════════════════════════════════════════
        private void SortLeftToRight()
        {
            var cam = sortCamera != null ? sortCamera : Camera.main;

            if (cam == null)
            {
                _active.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
                return;
            }

            var camT = cam.transform;
            _active.Sort((a, b) =>
                camT.InverseTransformPoint(a.transform.position).x
                    .CompareTo(camT.InverseTransformPoint(b.transform.position).x));
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
