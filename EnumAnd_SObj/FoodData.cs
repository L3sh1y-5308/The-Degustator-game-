// FoodData.cs
// ScriptableObject — данные одного продукта питания
// Хранит иконки (миниатюра и крупный вид) + базовые параметры

using UnityEngine;

namespace Degustation
{
    [CreateAssetMenu(
        fileName = "NewFood",
        menuName = "Degustation/Food")]
    public class FoodData : ScriptableObject
    {
        [Header("Название")]
        public string foodName = "Food";

        [Header("Иконки")]
        [Tooltip("Маленькая иконка — для инвентаря, списков")]
        public Sprite iconSmall;

        [Tooltip("Большая иконка — для детального просмотра, тултипа")]
        public Sprite iconLarge;

        [Header("Параметры")]
        [Range(0, 100)] public int nutritionValue  = 50;   // Питательность
        [Range(0, 100)] public int stomachImpact   = 10;   // Влияние на желудок (+ восст / - вред)
        [Range(-100, 100)] public int senseModifier = 0;   // Влияние на чувства (общее смещение)
        public SenseType targetSense = SenseType.Vision;   // Какое чувство затрагивает
    }
}
