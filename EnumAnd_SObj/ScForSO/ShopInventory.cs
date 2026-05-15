using System.Collections.Generic;
using UnityEngine;

// SB // Цены Магазина — инвентарь магазина
// ────────────────────────────────────────────────────────────────
// Использование:
//   1. Создай ScriptableObject: Assets → Create → SB → ShopInventory
//   2. В инспекторе заполни Items (или они уже есть по умолчанию)
//   3. Вызывай TryBuy(itemId, playerScore) из UI-кнопок
// ────────────────────────────────────────────────────────────────

[CreateAssetMenu(fileName = "ShopInventory", menuName = "SB/ShopInventory")]
public class ShopInventory : ScriptableObject
{
    // ── Категории ─────────────────────────────────────────────────
    public enum ItemCategory
    {
        Scroll,     // Свитки знаний
        Potion,     // Зелья
        Charm,      // Обереки
        Tool        // Объекты для анализа
    }

    // ── Товар ─────────────────────────────────────────────────────
    [System.Serializable]
    public class ShopItem
    {
        [Tooltip("Уникальный ID (например: scroll_skill_fire)")]
        public string id;
        public string displayName;
        public ItemCategory category;
        public float price;
        [Tooltip("Описание эффекта для UI")]
        [TextArea(1, 3)] public string description;
        [Tooltip("Количество в магазине (-1 = бесконечно)")]
        public int stock = -1;
    }

    // ── Каталог товаров ───────────────────────────────────────────
    [Header("Каталог товаров")]
    public List<ShopItem> Items = new List<ShopItem>
    {
        // ── Свитки знаний ─────────────────────────────────────────
        new ShopItem { id="scroll_class_skill",    displayName="Свиток: Скил класса",
                       category=ItemCategory.Scroll, price=80f,
                       description="Открывает базовый скил вашего класса.", stock=-1 },

        new ShopItem { id="scroll_combo_skill",    displayName="Свиток: Комбо-скил",
                       category=ItemCategory.Scroll, price=150f,
                       description="Открывает комбо-атаку с союзником.", stock=5 },

        new ShopItem { id="scroll_economy_skill",  displayName="Свиток: Экономный скил",
                       category=ItemCategory.Scroll, price=60f,
                       description="Снижает расход маны на 20%.", stock=-1 },

        // ── Зелья ─────────────────────────────────────────────────
        new ShopItem { id="potion_head",           displayName="Зелье: Лечение головы",
                       category=ItemCategory.Potion, price=25f,
                       description="Лечит травмы головы и снимает оглушение.", stock=-1 },

        new ShopItem { id="potion_limb",           displayName="Зелье: Лечение конечностей",
                       category=ItemCategory.Potion, price=20f,
                       description="Восстанавливает руки/ноги.", stock=-1 },

        new ShopItem { id="potion_shield",         displayName="Зелье: Щит от урона",
                       category=ItemCategory.Potion, price=45f,
                       description="Даёт временный барьер, поглощающий урон.", stock=10 },

        // ── Обереки ───────────────────────────────────────────────
        new ShopItem { id="charm_detect_curse",    displayName="Оберег: Детектор порчи",
                       category=ItemCategory.Charm, price=120f,
                       description="Выявляет отдельные виды порчи на предметах.", stock=3 },

        // ── Объекты для анализа ───────────────────────────────────
        new ShopItem { id="tool_glasses",          displayName="Очки анализа",
                       category=ItemCategory.Tool, price=200f,
                       description="Показывают скрытые свойства предметов.", stock=2 },

        new ShopItem { id="tool_magnifier",        displayName="Лупа",
                       category=ItemCategory.Tool, price=90f,
                       description="Детальный осмотр улик и мелких объектов.", stock=-1 },

        new ShopItem { id="tool_inv_expand",       displayName="Расширитель инвентаря",
                       category=ItemCategory.Tool, price=300f,
                       description="Добавляет +10 слотов в инвентарь.", stock=1 },

        new ShopItem { id="tool_discount_card",    displayName="Скидочная карточка (1 покупка)",
                       category=ItemCategory.Tool, price=50f,
                       description="Даёт скидку 30% на следующую покупку.", stock=5 },
    };

    // ── API ───────────────────────────────────────────────────────

    /// Найти товар по ID
    public ShopItem Find(string id) => Items.Find(x => x.id == id);

    /// Попытка купить товар.
    /// discountCard — применить скидку 30% (карточка тратится снаружи)
    /// Возвращает true при успехе.
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
        // ↑ скидочная карточка даёт -30%

        if (!player.SpendMoney(finalPrice))
        {
            Debug.Log($"[Shop] Недостаточно денег. Нужно {finalPrice:F1}");
            return false;
        }

        if (item.stock > 0) item.stock--;
        Debug.Log($"[Shop] Куплено: {item.displayName} за {finalPrice:F1}");
        return true;
    }

    /// Все товары выбранной категории
    public List<ShopItem> GetByCategory(ItemCategory cat) =>
        Items.FindAll(x => x.category == cat);
}
