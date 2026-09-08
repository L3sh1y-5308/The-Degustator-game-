// InspectionProcessor.cs
// Прогоняет проверку всех блюд на столе слева направо.
// Сравнивает выбор игрока из SlidePanel с правильным чувством/действием в FoodData,
// начисляет очки и наносит урон органам через InspectionDamageTable.
//
// ЧТО ИЗМЕНИЛОСЬ ПРОТИВ 2D-ВЕРСИИ:
//   • работает с FoodSpawner3D / FoodItem вместо Canvas-слотов ItemSlot;
//   • урон считается по УЖЕ отроленному RuntimeFood конкретного блюда
//     (раньше Roll() вызывался повторно в момент урона — эффекты
//      получались другие, чем те, что были у предмета на столе);
//   • ActionUnlockManager резолвится один раз, а не FindObjectOfType в цикле.

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    public class InspectionProcessor : MonoBehaviour
    {
        [Header("Зависимости")]
        [SerializeField] private FoodSpawner3D        spawner;
        [SerializeField] private SlidePanel           slidePanel;
        [SerializeField] private SenseStatsData       senseStats;
        [SerializeField] private InspectionDamageTable damageTable;
        [SerializeField] private ActionUnlockManager  unlockManager;

        // ════════════════════════════════════════════════════════
        public enum InspectionGrade
        {
            Fail,       // неправильное чувство → макс урон
            Partial,    // правильное чувство, неправильное действие → мало урона
            Perfect     // и чувство и действие правильные → нет урона
        }

        public struct InspectionResult
        {
            public FoodData        food;
            public InspectionGrade grade;
            public int             score;
            public SenseType       usedSense;
            public SubActionData   usedAction;
        }

        private void Awake()
        {
            if (spawner       == null) spawner       = FindComponent<FoodSpawner3D>();
            if (slidePanel    == null) slidePanel    = FindComponent<SlidePanel>();
            if (unlockManager == null) unlockManager = FindComponent<ActionUnlockManager>();
        }

        // ════════════════════════════════════════════════════════
        // Главный метод — вызывай из кнопки "Проверить"
        // ════════════════════════════════════════════════════════
        public List<InspectionResult> ProcessAll()
        {
            var results = new List<InspectionResult>();

            if (spawner == null)
            {
                Debug.LogWarning("[InspectionProcessor] Нет FoodSpawner3D!");
                return results;
            }

            foreach (var item in spawner.ActiveItems)   // уже слева направо
            {
                if (item == null || item.Food == null) continue;

                var choice = slidePanel != null ? slidePanel.GetChoice(item.Food) : null;
                var result = ProcessSingle(item, choice);
                results.Add(result);

                Debug.Log($"[Inspection] {item.Food.foodName} → {result.grade} | +{result.score} очков");
            }

            return results;
        }

        // ════════════════════════════════════════════════════════
        // Проверка одного блюда
        // ════════════════════════════════════════════════════════
        private InspectionResult ProcessSingle(FoodItem item, FoodInspectionChoice choice)
        {
            FoodData food = item.Food;
            var result = new InspectionResult { food = food, usedSense = food.targetSense };

            // Игрок вообще не выбрал чувство — провал
            if (choice == null || choice.activeSenses.Count == 0)
            {
                result.grade = InspectionGrade.Fail;
                result.score = 0;
                ApplyDamage(item, GetWorstSense(food));
                return result;
            }

            bool correctSense = choice.activeSenses.Contains(food.targetSense);
            bool correctAction = false;
            SubActionData usedAction = null;

            if (correctSense && food.correctAction != null &&
                choice.selectedAction.TryGetValue(food.targetSense, out int idx))
            {
                var unlocked = unlockManager != null
                    ? unlockManager.GetUnlockedForSense(food.targetSense)
                    : new List<SubActionData>();

                if (idx >= 0 && idx < unlocked.Count)
                {
                    usedAction    = unlocked[idx];
                    correctAction = usedAction == food.correctAction;
                }
            }

            result.usedAction = usedAction;

            if (correctSense && correctAction)
            {
                result.grade = InspectionGrade.Perfect;
                result.score = 20;
                // урона нет — игрок сделал всё правильно
            }
            else if (correctSense)
            {
                result.grade = InspectionGrade.Partial;
                result.score = 10;
                ApplyDamage(item, food.targetSense);
            }
            else
            {
                result.grade = InspectionGrade.Fail;
                result.score = 0;
                ApplyDamage(item, GetWorstSense(food));
            }

            return result;
        }

        // ════════════════════════════════════════════════════════
        // Урон — по РЕАЛЬНОМУ роллу этого экземпляра блюда
        // ════════════════════════════════════════════════════════
        private void ApplyDamage(FoodItem item, SenseType sense)
        {
            if (damageTable == null || senseStats == null) return;

            RuntimeFood runtime = item.Runtime ?? new RuntimeFood(item.Food);
            damageTable.ApplyDamage(runtime, sense, senseStats);
        }

        // При провале считаем, что игрок полез самым опасным способом
        private SenseType GetWorstSense(FoodData food) => SenseType.Taste;

        private static T FindComponent<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }
    }
}
