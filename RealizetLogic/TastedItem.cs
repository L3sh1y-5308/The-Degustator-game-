// TastedItem.cs
// Компонент на префабе дегустируемого предмета.
// Хранит RuntimeFood, применяет урон через InspectionDamageTable при проверке.

using UnityEngine;

namespace Degustation
{
    public class TastedItem : MonoBehaviour
    {
        [Header("Данные предмета")]
        [Tooltip("Уникальный ID предмета (для отслеживания ответов)")]
        public string itemId;

        [Tooltip("Название для UI")]
        public string displayName;

        [Tooltip("Визуальная подсказка (опционально)")]
        public Sprite itemSprite;

        [Header("Параметры оценки")]
        [Tooltip("Очки за верное определение")]
        public int baseScore = 10;

        [Tooltip("Штраф за неверный ответ")]
        public int wrongAnswerPenalty = 2;

        // ── Рантайм-данные (заполняет TastedItemSpawner через Init) ───
        public RuntimeFood RuntimeFood { get; private set; }

        // ── Ссылки на SO-системы (назначь в инспекторе или через Init) ──
        [Header("Системы (назначь или подтянутся из GameManager)")]
        public SenseStatsData        senseStats;
        public InspectionDamageTable damageTable;

        // ════════════════════════════════════════════════════════════
        // Инициализация — вызывает TastedItemSpawner после Instantiate
        // ════════════════════════════════════════════════════════════
        public void Init(RuntimeFood food, SenseStatsData stats, InspectionDamageTable table)
        {
            RuntimeFood = food;
            senseStats  = stats;
            damageTable = table;

            // Название из шаблона, если не переопределено вручную
            if (string.IsNullOrEmpty(displayName) && food?.source != null)
                displayName = food.source.foodName;
        }

        // ════════════════════════════════════════════════════════════
        // Главный метод — вызывай когда игрок применил действие к блюду
        //
        //   playerAnswer    — ответ из UI (например "glass", "poison")
        //   usedSense       — каким чувством проверял
        //   correctAnswer   — правильный ответ (из FoodData или заранее)
        //
        // Возвращает очки (положительные = правильно, отрицательные = штраф)
        // Побочный эффект: наносит урон органам через damageTable
        // ════════════════════════════════════════════════════════════
        public int Inspect(string playerAnswer, SenseType usedSense, string correctAnswer)
        {
            // 1. Применяем урон ПЕРЕД проверкой ответа
            //    (игрок уже взаимодействовал с едой — риск уже случился)
            if (damageTable != null && senseStats != null && RuntimeFood != null)
                damageTable.ApplyDamage(RuntimeFood, usedSense, senseStats);

            // 2. Проверяем ответ
            bool isCorrect = string.Equals(
                playerAnswer, correctAnswer,
                System.StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                // Бонус если использовано "правильное" чувство
                // (Vision = безопасно и точно → максимальный бонус)
                int bonus = usedSense == SenseType.Vision ? 5 : 0;
                return baseScore + bonus;
            }

            return -wrongAnswerPenalty;
        }

        // Краткое описание для UI/логов
        public string GetDisplayInfo()
            => $"{displayName} (ID: {itemId})";
    }
}
