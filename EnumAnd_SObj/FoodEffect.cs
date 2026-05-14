// FoodEffect.cs
// ScriptableObject — список enum-значений конкретной еды
// Менеджер читает отсюда и проверяет совместимости
//
// КАК РАСШИРЯТЬ:
//   Добавил новый яд в PoisonType → он появится в дропдауне автоматически
//   Добавил новый сенс в SenseType → аналогично
//   Нужен новый тип данных (например ItemType) → добавь List<ItemType> сюда

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    [CreateAssetMenu(fileName = "NewFoodEffect", menuName = "Degustation/Food Effect")]
    public class FoodEffect : ScriptableObject
    {
        [Header("Яды")]
        [Tooltip("Какие яды содержит продукт. Добавляй значения в PoisonType enum → появятся здесь")]
        public List<PoisonosActions.PoisonType> poisons = new();

        [Header("Болезни")]
        [Tooltip("Какие болезни может передать продукт")]
        public List<PoisonosActions.DiseaseType> diseases = new();

        [Header("Затрагиваемые чувства")]
        [Tooltip("Какие органы чувств затрагивает (кроме основного targetSense в FoodData)")]
        public List<SenseType> affectedSenses = new();

        [Header("Расходники / экипировка (для совместимости)")]
        [Tooltip("Какие расходники взаимодействуют с этим продуктом")]
        public List<Degustator.SensoryConsumable> relatedConsumables = new();

        [Tooltip("Какая экипировка защищает от этого продукта")]
        public List<Degustator.SensoryEquipment> blockedByEquipment = new();
    }
}