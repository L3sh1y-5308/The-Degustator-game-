// CheckingPanel.cs
// Куб уже на сцене и выезжает по Y через DOTween.
// Скрипт спавнит SpriteRenderer еды на FoodSpawnPoint,
// показывает каждое блюдо holdDuration секунд,
// применяет Outline (OutlineFx) по статусу и TMP текст который плывёт вверх и рассеивается.
// После всех блюд — запускает InspectionProcessor.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using TMPro;
using OutlineFx;
using Degustation;

public class CheckingPanel : MonoBehaviour
{
    [Header("Куб — анимация по Y")]
    [SerializeField] private Transform cubeTransform;
    [SerializeField] private float hiddenY = 8f;
    [SerializeField] private float visibleY = 0f;
    [SerializeField] private float slideDuration = 0.6f;

    [Header("Спавн спрайта еды")]
    [SerializeField] private Transform foodSpawnPoint;
    [SerializeField] private float spriteScale = 1f;

    [Header("Карусель")]
    [SerializeField] private float holdDuration = 4f;
    [SerializeField] private float scrollDuration = 0.5f;
    [SerializeField] private float scrollOffsetX = 6f;

    [Header("Outline цвета по статусу")]
    [SerializeField] private Color colorPerfect = Color.green;
    [SerializeField] private Color colorPartial = Color.yellow;
    [SerializeField] private Color colorFail = Color.red;

    [Header("Текст статуса")]
    [SerializeField] private GameObject statusTextPrefab;
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float textRiseDuration = 1.5f;
    [SerializeField] private float textFadeDuration = 1f;

    [Header("Зависимости")]
    [SerializeField] private InspectionProcessor processor;
    [SerializeField] private TastedItemSpawner spawner;

    private GameObject _currentSprite;
    private Outline _currentOutline;

    // ── Вызывается кнопкой "Start Checking" ──────────────────────
    public void StartChecking() => _ = RunChecking();

    async Task RunChecking()
    {
        var slots = spawner.GetActiveSlots();
        if (slots.Count == 0) return;

        // Получаем результаты до показа — чтобы знать цвет outline
        var results = processor.ProcessAll();

        // Куб выезжает вниз
        await cubeTransform
            .DOMoveY(visibleY, slideDuration)
            .SetEase(Ease.OutQuart)
            .AsyncWaitForCompletion();

        for (int i = 0; i < slots.Count; i++)
        {
            FoodData food = slots[i].GetFood();
            if (food == null) continue;

            var grade = i < results.Count
                ? results[i].grade
                : InspectionProcessor.InspectionGrade.Fail;

            // Спрайт въезжает справа
            SpawnFoodSprite(food, grade);
            await Task.Delay((int)(holdDuration * 1000));

            // Текст статуса
            string label = grade switch
            {
                InspectionProcessor.InspectionGrade.Perfect => "Супер!",
                InspectionProcessor.InspectionGrade.Partial => "Молодец",
                InspectionProcessor.InspectionGrade.Fail => "Провал",
                _ => ""
            };
            _ = ShowStatusText(label, grade);
            await Task.Delay(800);

            // Уезжает влево — чистим
            if (i < slots.Count - 1)
            {
                await _currentSprite.transform
                    .DOMoveX(cubeTransform.position.x - scrollOffsetX, scrollDuration)
                    .SetEase(Ease.InQuad)
                    .AsyncWaitForCompletion();
                DestroyCurrentSprite();
            }
        }

        await Task.Delay(1000);
        DestroyCurrentSprite();

        // Куб уезжает вверх
        await cubeTransform
            .DOMoveY(hiddenY, slideDuration)
            .SetEase(Ease.InQuart)
            .AsyncWaitForCompletion();

        // GameManager.ReceiveInspectionResults() удалён вместе с заглушкой CollectResults():
        // теперь GameManager сам вызывает InspectionProcessor.ProcessAll() и получает
        // готовые результаты, а не чинит их задним числом. Эта карусель — 2D-легаси
        // и в 3D-сцене не участвует; при переделке под 3D подписывайся на события
        // раунда вместо прямого вызова в GameManager.
        Debug.Log($"[CheckingPanel] Показ результатов завершён: {results.Count} блюд.");
    }

    void SpawnFoodSprite(FoodData food, InspectionProcessor.InspectionGrade grade)
    {
        DestroyCurrentSprite();

        _currentSprite = new GameObject($"FoodDisplay_{food.foodName}");
        var sr = _currentSprite.AddComponent<SpriteRenderer>();
        sr.sprite = food.shopIcon;
        sr.sortingOrder = 1;

        _currentSprite.transform.localScale = Vector3.one * spriteScale;

        // Outline по статусу
        _currentOutline = _currentSprite.AddComponent<Outline>();
        _currentOutline.Color = grade switch
        {
            InspectionProcessor.InspectionGrade.Perfect => colorPerfect,
            InspectionProcessor.InspectionGrade.Partial => colorPartial,
            InspectionProcessor.InspectionGrade.Fail => colorFail,
            _ => Color.white
        };

        // Въезжает справа
        Vector3 startPos = foodSpawnPoint.position + Vector3.right * scrollOffsetX;
        _currentSprite.transform.position = startPos;
        _currentSprite.transform
            .DOMoveX(foodSpawnPoint.position.x, scrollDuration)
            .SetEase(Ease.OutQuad);
    }

    void DestroyCurrentSprite()
    {
        if (_currentSprite == null) return;
        _currentSprite.transform.DOKill();
        Destroy(_currentSprite);
        _currentSprite = null;
        _currentOutline = null;
    }

    async Task ShowStatusText(string message, InspectionProcessor.InspectionGrade grade)
    {
        if (statusTextPrefab == null) return;

        var textObj = Instantiate(
            statusTextPrefab,
            foodSpawnPoint.position + textOffset,
            Quaternion.identity);

        var tmp = textObj.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = message;
            tmp.color = grade switch
            {
                InspectionProcessor.InspectionGrade.Perfect => colorPerfect,
                InspectionProcessor.InspectionGrade.Partial => colorPartial,
                InspectionProcessor.InspectionGrade.Fail => colorFail,
                _ => Color.white
            };

            textObj.transform
                .DOMoveY(textObj.transform.position.y + 1.5f, textRiseDuration)
                .SetEase(Ease.OutQuad);

            await tmp
                .DOFade(0f, textFadeDuration)
                .SetDelay(textRiseDuration - textFadeDuration)
                .AsyncWaitForCompletion();
        }
        else
        {
            await Task.Delay((int)(textRiseDuration * 1000));
        }

        Destroy(textObj);
    }
}