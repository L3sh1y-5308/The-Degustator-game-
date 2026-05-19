using System.Collections.Generic;
using UnityEngine;

// ShopInventory.cs
// SO — каталог товаров магазина.
// Свитки ссылаются на ScrollItemData, который открывает SubActionData.
// Покупка через TryBuy → затем вызови ActionUnlockManager.UnlockViaScroll(scroll).

namespace Degustation
{
    [CreateAssetMenu(fileName = "ShopInventory", menuName = "Degustation/Shop Inventory")]
    public class ShopInventory : ScriptableObject
    {
        public enum ItemCategory
        {
            Scroll,   // Свитки навыков
            Potion,   // Зелья (лечение органов)
            Tool      // Инструменты (доп. слоты, бонусы)
        }

        [System.Serializable]
        public class ShopItem
        {
            [Tooltip("Уникальный ID товара")]
            public string id;
            public string displayName;
            public Sprite icon;
            public ItemCategory category;
            public float price;
            [TextArea(1, 3)] public string description;
            public int stock = -1; // -1 = бесконечно

            [Tooltip("Только для Scroll — ссылка на ScrollItemData который разблокируется")]
            public ScrollItemData scrollData;
        }

        [Header("Каталог товаров")]
        public List<ShopItem> Items = new List<ShopItem>
        {
            // ── Свитки навыков — Вкус ─────────────────────────────
            new ShopItem { id="scroll_hold_tongue",
                           displayName="Свиток: Подержать на языке",
                           category=ItemCategory.Scroll, price=80f,
                           description="Открывает навык 'Подержать на языке' (Вкус).", stock=-1 },

            new ShopItem { id="scroll_hold_under_tongue",
                           displayName="Свиток: Подержать под языком",
                           category=ItemCategory.Scroll, price=120f,
                           description="Открывает навык 'Подержать под языком' (Вкус).", stock=-1 },

            // ── Свитки навыков — Зрение ───────────────────────────
            new ShopItem { id="scroll_squint",
                           displayName="Свиток: Прищуриться",
                           category=ItemCategory.Scroll, price=60f,
                           description="Открывает навык 'Прищуриться' (Зрение).", stock=-1 },

            new ShopItem { id="scroll_examine_each",
                           displayName="Свиток: Рассмотреть каждую",
                           category=ItemCategory.Scroll, price=100f,
                           description="Открывает навык 'Рассмотреть каждую отдельно' (Зрение).", stock=-1 },

            new ShopItem { id="scroll_look_sunlight",
                           displayName="Свиток: Посмотреть под солнцем",
                           category=ItemCategory.Scroll, price=150f,
                           description="Открывает навык 'Посмотреть под солнцем' (Зрение).", stock=3 },

            // ── Свитки навыков — Осязание ─────────────────────────
            new ShopItem { id="scroll_measure_temp",
                           displayName="Свиток: Измерить температуру",
                           category=ItemCategory.Scroll, price=80f,
                           description="Открывает навык 'Измерить температуру' (Осязание).", stock=-1 },

            new ShopItem { id="scroll_rub_fingers",
                           displayName="Свиток: Растереть меж пальцами",
                           category=ItemCategory.Scroll, price=110f,
                           description="Открывает навык 'Растереть меж пальцами' (Осязание).", stock=-1 },

            // ── Свитки навыков — Обоняние ─────────────────────────
            new ShopItem { id="scroll_deep_sniff",
                           displayName="Свиток: Занюхнуть",
                           category=ItemCategory.Scroll, price=70f,
                           description="Открывает навык 'Занюхнуть' (Обоняние).", stock=-1 },

            // ── Свитки навыков — Слух ─────────────────────────────
            new ShopItem { id="scroll_listen_vibrations",
                           displayName="Свиток: Прислушаться к вибрациям",
                           category=ItemCategory.Scroll, price=130f,
                           description="Открывает навык 'Прислушаться к вибрациям' (Слух).", stock=-1 },

            // ── Зелья ─────────────────────────────────────────────
            new ShopItem { id="potion_eye",
                           displayName="Зелье: Лечение зрения",
                           category=ItemCategory.Potion, price=30f,
                           description="Восстанавливает HP органа Зрения.", stock=-1 },

            new ShopItem { id="potion_mouth",
                           displayName="Зелье: Лечение вкуса",
                           category=ItemCategory.Potion, price=30f,
                           description="Восстанавливает HP органа Вкуса.", stock=-1 },

            new ShopItem { id="potion_nose",
                           displayName="Зелье: Лечение обоняния",
                           category=ItemCategory.Potion, price=30f,
                           description="Восстанавливает HP органа Обоняния.", stock=-1 },

            new ShopItem { id="potion_ear",
                           displayName="Зелье: Лечение слуха",
                           category=ItemCategory.Potion, price=30f,
                           description="Восстанавливает HP органа Слуха.", stock=-1 },

            new ShopItem { id="potion_touch",
                           displayName="Зелье: Лечение осязания",
                           category=ItemCategory.Potion, price=30f,
                           description="Восстанавливает HP органа Осязания.", stock=-1 },

            // ── Инструменты ───────────────────────────────────────
            new ShopItem { id="tool_extra_points",
                           displayName="Усиленная концентрация",
                           category=ItemCategory.Tool, price=200f,
                           description="Добавляет +2 очка инспекции на сессию.", stock=2 },

            new ShopItem { id="tool_discount_card",
                           displayName="Скидочная карточка",
                           category=ItemCategory.Tool, price=50f,
                           description="Даёт скидку 30% на следующую покупку.", stock=5 },
        };

        // ── API ───────────────────────────────────────────────────

        public ShopItem Find(string id) => Items.Find(x => x.id == id);

        public List<ShopItem> GetByCategory(ItemCategory cat) =>
            Items.FindAll(x => x.category == cat);

        /// Попытка купить товар.
        /// После успешной покупки свитка вызови:
        ///   ActionUnlockManager.Instance.UnlockViaScroll(item.scrollData)
        /// После покупки tool_extra_points вызови:
        ///   inspectionPoints.AddBonus(2)
        public bool TryBuy(string itemId, PlayerScore player, bool discountCard = false)
        {
            var item = Find(itemId);
            if (item == null)
            {
                Debug.LogWarning($"[Shop] Товар не найден: {itemId}");
                return false;
            }

            if (item.stock == 0)
            {
                Debug.Log($"[Shop] Нет в наличии: {item.displayName}");
                return false;
            }

            float finalPrice = discountCard ? item.price * 0.7f : item.price;

            if (!player.SpendMoney(finalPrice))
            {
                Debug.Log($"[Shop] Недостаточно денег. Нужно {finalPrice:F1}");
                return false;
            }

            if (item.stock > 0) item.stock--;
            Debug.Log($"[Shop] Куплено: {item.displayName} за {finalPrice:F1}");
            return true;
        }
    }
}
