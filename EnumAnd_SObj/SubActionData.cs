// SubActionData.cs
// ScriptableObject — описание одного суб-действия (создаётся как Asset в редакторе)

using UnityEngine;

namespace Degustation
{
    [CreateAssetMenu(
        fileName = "SubActionData",
        menuName = "Degustation/Sub Action Data")]
    public class SubActionData : ScriptableObject
    {
        [Header("Идентификация")]
        public SenseType   senseType;    // К какому чувству относится
        public string      actionId;     // Уникальный строковый ID (напр. "HoldOnTongue")
        public string      displayName;  // Название для UI

        [Header("Разблокировка")]
        public UnlockType  unlockType;   // Starter или Scroll
        // Если Scroll — сюда кладём ссылку на свиток (можно оставить null для стартовых)
        public ScrollItemData requiredScroll;

        [Header("Описание")]
        [TextArea] public string description; // Что происходит при выборе этого действия

        [Header("Параметры действия")]
        [Range(0f, 1f)]
        public float accuracyBonus;   // Бонус к точности определения вещества
        public float timeCost;        // Время в секундах, которое уходит на действие
    }
}
