// ScrollItemData.cs
// ScriptableObject — свиток навыка, который открывает суб-действие

using UnityEngine;

namespace Degustation
{
    [CreateAssetMenu(
        fileName = "ScrollItemData",
        menuName = "Degustation/Scroll Item Data")]
    public class ScrollItemData : ScriptableObject
    {
        [Header("Базовое")]
        public string      scrollId;     // Уникальный ID
        public string      displayName;
        [TextArea]
        public string      description;
        public Sprite      icon;

        [Header("Магазин")]
        public int         price;        // Цена в игровой валюте
        // SenseType определяет, в какую категорию свитков попадёт в магазине
        public SenseType   senseType;

        [Header("Открывает действие")]
        // Прямая ссылка на SubActionData, которое разблокируется после покупки
        public SubActionData unlocksAction;
    }
}
