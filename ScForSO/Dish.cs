// DishData.cs

using UnityEngine;

namespace Degustation
{
    [CreateAssetMenu(fileName = "NewDish", menuName = "Degustation/Dish")]
    public class DishData : ScriptableObject
    {
        [Header("Название")]
        public string dishName = "Dish";

        [Header("Иконка")]
        public Sprite dishIcon;

        [Header("Кусочки еды")]
        public FoodData[] pieces;
    }
}