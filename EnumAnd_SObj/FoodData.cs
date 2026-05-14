// FoodData.cs
// Только: название, иконки, targetSense + ссылка на FoodEffectProfile SO

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    [CreateAssetMenu(fileName = "NewFood", menuName = "Degustation/Food")]
    public class FoodData : ScriptableObject
    {
        [Header("Название")]
        public string foodName = "Food";

        [Header("Иконки")]
        public Sprite iconSmall;
        public Sprite iconLarge;

        [Header("Целевой сенс")]
        public SenseType targetSense = SenseType.Vision;

        // Перетаскиваешь сюда FoodEffectProfile из Assets
        // Создать: ПКМ в Project → Degustation → Food Effect Profile
        [Header("Профиль эффектов")]
        [Tooltip("SO с enum-списками: яды, болезни, сенсы. Менеджер читает отсюда для проверки совместимости")]
       public FoodEffect effectProfile;

        // ════════════════════════════════════════════════════════════
        // Создать рантайм-экземпляр блюда с рандомными эффектами
        // ════════════════════════════════════════════════════════════
        public RuntimeFood Roll()
        {
            var runtime = new RuntimeFood(this);

            if (effectProfile == null)
                return runtime;

            // Конвертируем яды из FoodEffect в EffectEntry
            foreach (var poison in effectProfile.poisons)
            {
                runtime.rolledEffects.Add(new EffectEntry
                {
                    effectType  = EffectType.Poison,
                    customLabel = poison.ToString(),
                    strength    = UnityEngine.Random.Range(1, 6)
                });
            }

            // Конвертируем болезни
            foreach (var disease in effectProfile.diseases)
            {
                runtime.rolledEffects.Add(new EffectEntry
                {
                    effectType  = EffectType.Disease,
                    customLabel = disease.ToString(),
                    strength    = UnityEngine.Random.Range(1, 4)
                });
            }

            return runtime;
        }
    }
}