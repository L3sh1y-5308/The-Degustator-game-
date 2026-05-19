// CheckingPanel.cs
// Панель проверки блюд.
// Выезжает сверху вниз. Внутри карусель — каждая карточка стоит holdDuration секунд,
// затем Container сдвигается влево на ширину карточки (следующая въезжает справа).
// После показа всех блюд — запускает InspectionProcessor и прячется.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Degustation;

public class CheckingPanel : MonoBehaviour
{
    [Header("Панель — анимация по Y")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float hiddenY       = 800f;
    [SerializeField] private float visibleY      = 0f;
    [SerializeField] private float slideDuration = 0.5f;

    [Header("Карусель")]
    [SerializeField] private RectTransform container;   // двигается по X
    [SerializeField] private GameObject    cardPrefab;  // префаб карточки с Image
    [SerializeField] private float         cardWidth    = 400f;  // ширина одной карточки
    [SerializeField] private float         holdDuration = 4f;    // секунд на блюдо
    [SerializeField] private float         scrollDuration = 0.6f; // скорость листания

    [Header("Зависимости")]
    [SerializeField] private InspectionProcessor processor;
    [SerializeField] private TastedItemSpawner   spawner;

    private List<GameObject> _cards = new();

    void Start()
    {
        panelRect.anchoredPosition = new Vector2(
            panelRect.anchoredPosition.x, hiddenY);
    }

    // ── Вызывается кнопкой "Start Checking" ──────────────────────
    public void StartChecking() => _ = RunChecking();

    async Task RunChecking()
    {
        var slots = spawner.GetActiveSlots();
        if (slots.Count == 0) return;

        // 1. Строим карточки
        BuildCards(slots);

        // 2. Панель выезжает сверху вниз
        await panelRect
            .DOAnchorPosY(visibleY, slideDuration)
            .SetEase(Ease.OutQuart)
            .SetUpdate(true)
            .AsyncWaitForCompletion();

        // 3. Показываем каждую карточку
        for (int i = 0; i < slots.Count; i++)
        {
            // Карточка уже на месте — ждём holdDuration
            await Task.Delay((int)(holdDuration * 1000));

            // Листаем влево (если не последняя)
            if (i < slots.Count - 1)
            {
                float targetX = -(i + 1) * cardWidth;
                await container
                    .DOAnchorPosX(targetX, scrollDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetUpdate(true)
                    .AsyncWaitForCompletion();
            }
        }

        // 4. Небольшая пауза перед закрытием
        await Task.Delay(500);

        // 5. Панель уезжает обратно вверх
        await panelRect
            .DOAnchorPosY(hiddenY, slideDuration)
            .SetEase(Ease.InQuart)
            .SetUpdate(true)
            .AsyncWaitForCompletion();

        // 6. Чистим карточки
        ClearCards();

        // 7. Запускаем проверку и передаём результаты
        var results = processor.ProcessAll();
        GameManager.Instance.ReceiveInspectionResults(results);
    }

    // ── Создаём карточки под каждое блюдо ────────────────────────
    void BuildCards(List<ItemSlot> slots)
    {
        ClearCards();

        // Сбрасываем позицию контейнера
        container.anchoredPosition = Vector2.zero;

        for (int i = 0; i < slots.Count; i++)
        {
            FoodData food = slots[i].GetFood();
            if (food == null) continue;

            GameObject card = Instantiate(cardPrefab, container);
            RectTransform rt = card.GetComponent<RectTransform>();

            // Расставляем карточки горизонтально
            rt.anchoredPosition = new Vector2(i * cardWidth, 0f);
            rt.sizeDelta        = new Vector2(cardWidth, rt.sizeDelta.y);

            // Назначаем иконку
            Image img = card.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = food.shopIcon;
                img.color  = Color.white;
            }

            _cards.Add(card);
        }

        // Растягиваем контейнер под все карточки
        container.sizeDelta = new Vector2(slots.Count * cardWidth, container.sizeDelta.y);
    }

    void ClearCards()
    {
        foreach (var card in _cards)
            if (card != null) Destroy(card);
        _cards.Clear();
    }
}
