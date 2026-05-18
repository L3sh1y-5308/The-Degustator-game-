// FoodData.cs

using UnityEngine;

namespace Degustation
{
    [CreateAssetMenu(fileName = "NewFood", menuName = "Degustation/Food")]
    public class FoodData : ScriptableObject
    {
        [Header("Название")]
        public string foodName = "Food";

        [Header("Иконка для магазина")]
        [Tooltip("Используется только в UI магазина")]
        public Sprite shopIcon;

        [Header("Целевой сенс")]
        [Tooltip("Основное чувство которым проверяется это блюдо")]
        public SenseType targetSense = SenseType.Vision;

        [Header("Профиль эффектов")]
        [Tooltip("FoodEffect SO — яды/болезни блюда")]
        public FoodEffect effectProfile;

        public RuntimeFood Roll()
        {
            var runtime = new RuntimeFood(this);

            if (effectProfile == null)
                return runtime;

            foreach (var poison in effectProfile.poisons)
            {
                runtime.rolledEffects.Add(new EffectEntry
                {
                    effectType  = EffectType.Poison,
                    customLabel = poison.ToString(),
                    strength    = Random.Range(1, 6)
                });
            }

            foreach (var disease in effectProfile.diseases)
            {
                runtime.rolledEffects.Add(new EffectEntry
                {
                    effectType  = EffectType.Disease,
                    customLabel = disease.ToString(),
                    strength    = Random.Range(1, 4)
                });
            }

            return runtime;
        }
    }
}
