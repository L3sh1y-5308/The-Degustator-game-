// InspectionProcessor.cs
// Запускает проверку всех блюд слева направо.
// Сравнивает выбор игрока из SlidePanel с правильным чувством/действием в FoodData.
// Начисляет очки и наносит урон через InspectionDamageTable.
//
// Вешается на GameManager или отдельный GameObject в сцене.

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    public class InspectionProcessor : MonoBehaviour
    {
        [Header("Зависимости")]
        [SerializeField] private TastedItemSpawner     spawner;
        [SerializeField] private SlidePanel            slidePanel;
        [SerializeField] private SenseStatsData        senseStats;
        [SerializeField] private InspectionDamageTable damageTable;

        // ════════════════════════════════════════════════════════════
        // Результат проверки одного блюда
        // ════════════════════════════════════════════════════════════
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

        // ════════════════════════════════════════════════════════════
        // Главный метод — вызывай из кнопки "Проверить"
        // ════════════════════════════════════════════════════════════
        public List<InspectionResult> ProcessAll()
        {
            var results = new List<InspectionResult>();
            var slots   = spawner.GetActiveSlots(); // отсортированы слева направо

            foreach (var slot in slots)
            {
                FoodData food = slot.GetFood();
                if (food == null) continue;

                var choice = slidePanel.GetChoice(food);
                var result = ProcessSingle(food, choice);
                results.Add(result);

                Debug.Log($"[Inspection] {food.foodName} → {result.grade} | +{result.score} очков");
            }

            return results;
        }

        // ════════════════════════════════════════════════════════════
        // Проверка одного блюда
        // ════════════════════════════════════════════════════════════
        private InspectionResult ProcessSingle(FoodData food, FoodInspectionChoice choice)
        {
            var result = new InspectionResult { food = food };

            // Нет выбора — провал
            if (choice == null || choice.activeSenses.Count == 0)
            {
                result.grade = InspectionGrade.Fail;
                result.score = 0;
                ApplyDamage(food, SenseType.Taste); // худший вариант
                return result;
            }

            bool correctSense  = choice.activeSenses.Contains(food.targetSense);
            bool correctAction = false;
            SubActionData usedAction = null;

            if (correctSense && food.correctAction != null &&
                choice.selectedAction.ContainsKey(food.targetSense))
            {
                int idx             = choice.selectedAction[food.targetSense];
                var unlockedActions = FindObjectOfType<ActionUnlockManager>()
                                      ?.GetUnlockedForSense(food.targetSense)
                                      ?? new List<SubActionData>();

                if (idx < unlockedActions.Count)
                {
                    usedAction    = unlockedActions[idx];
                    correctAction = usedAction == food.correctAction;
                }
            }

            result.usedSense  = food.targetSense;
            result.usedAction = usedAction;

            if (correctSense && correctAction)
            {
                // Супер — нет урона
                result.grade = InspectionGrade.Perfect;
                result.score = 20;
            }
            else if (correctSense)
            {
                // Молодец — правильное чувство, слабый урон
                result.grade = InspectionGrade.Partial;
                result.score = 10;
                ApplyDamage(food, food.targetSense);
            }
            else
            {
                // Провал — макс урон
                result.grade = InspectionGrade.Fail;
                result.score = 0;
                ApplyDamage(food, GetWorstSense(food));
            }

            return result;
        }

        // ════════════════════════════════════════════════════════════
        // Урон
        // ════════════════════════════════════════════════════════════
        private void ApplyDamage(FoodData food, SenseType sense)
        {
            if (damageTable == null || senseStats == null) return;
            RuntimeFood runtime = food.effectProfile != null
                ? food.Roll()
                : new RuntimeFood(food);
            damageTable.ApplyDamage(runtime, sense, senseStats);
        }

        // При провале используем самое опасное чувство для этой еды
        private SenseType GetWorstSense(FoodData food)
        {
            // Вкус — самый опасный по умолчанию (макс урон в таблице)
            return SenseType.Taste;
        }
    }
}
