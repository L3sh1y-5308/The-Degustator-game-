// SlidePanel.cs
// Сайдпанель инспекции еды.
// Показывает иконку еды, 5 строк чувств (Toggle + Dropdown).
// Сохраняет выбор per-food. Поинты общие на сессию.
// В 3D-режиме открывается из InspectionController: Open(food) / Close().

using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Degustation;

public class SlidePanel : MonoBehaviour
{
    [Header("Анимация")]
    [SerializeField] private GameObject slidePanelObject;
    [SerializeField] private RectTransform panelPosition;
    [SerializeField] private float leftX, middlePosX;
    [SerializeField] private float tweenDuration;

    [Header("UI")]
    [SerializeField] private Image foodIcon;
    [SerializeField] private SenseRow[] senseRows; // 5 строк

    [Header("Данные")]
    [SerializeField] private InspectionPoints inspectionPoints;
    [SerializeField] private ActionUnlockManager unlockManager;

    private bool _isAnimating = false;
    private bool _isOpen      = false;
    private FoodData _currentFood;

    // Сохранённые выборы per-food
    private Dictionary<FoodData, FoodInspectionChoice> _choices = new();

    // ════════════════════════════════════════════════════════════
    void Start()
    {
        slidePanelObject.SetActive(true);
        panelPosition.anchoredPosition = new Vector2(leftX, panelPosition.anchoredPosition.y);
    }

    void Update()
    {
        // InputCompat вместо Input.GetKeyDown — старый Input Manager
        // выбрасывает исключение, если в проекте включён только новый Input System
        if (InputCompat.KeyDown(KeyCode.F) && !_isAnimating)
            ToggleVoid();
    }

    // ════════════════════════════════════════════════════════════
    // ЯВНОЕ открытие/закрытие — для 3D-режима осмотра.
    // ToggleWithFood оставлен ради старого 2D-кода, но он именно
    // переключает: второй клик по другому блюду закрывал бы панель.
    // ════════════════════════════════════════════════════════════
    public void Open(FoodData food)
    {
        if (food == null) return;

        SaveCurrentChoice();
        ApplyFood(food);

        if (!_isOpen) _ = SlideIn();
    }

    public void Close()
    {
        SaveCurrentChoice();
        if (_isOpen) _ = SlideOut();
    }

    // ════════════════════════════════════════════════════════════
    // Открыть панель с конкретной едой
    // ════════════════════════════════════════════════════════════
    public void ToggleWithFood(FoodData food)
    {
        SaveCurrentChoice();
        ApplyFood(food);
        _ = Toggle();
    }

    // Заполнить панель данными блюда (без анимации)
    private void ApplyFood(FoodData food)
    {
        _currentFood = food;

        if (foodIcon != null)
        {
            foodIcon.sprite = food.shopIcon;
            foodIcon.color  = food.shopIcon != null ? Color.white : new Color(1, 1, 1, 0.25f);
        }

        var choice = _choices.ContainsKey(food)
            ? _choices[food]
            : new FoodInspectionChoice();

        foreach (var row in senseRows)
        {
            bool isOn       = choice.activeSenses.Contains(row.senseType);
            int  savedIndex = choice.selectedAction.ContainsKey(row.senseType)
                ? choice.selectedAction[row.senseType] : 0;
            row.Setup(row.senseType, isOn, savedIndex, inspectionPoints, unlockManager);
        }
    }

    // Для клавиши F — без еды
    public void ToggleVoid() => _ = Toggle();

    // ════════════════════════════════════════════════════════════
    // Сохранить текущий выбор (вызывать перед сменой еды или закрытием)
    // ════════════════════════════════════════════════════════════
    public void SaveCurrentChoice()
    {
        if (_currentFood == null) return;
        var choice = new FoodInspectionChoice();
        foreach (var row in senseRows)
        {
            if (row.IsActive())
            {
                choice.activeSenses.Add(row.senseType);
                choice.selectedAction[row.senseType] = row.GetActionIndex();
            }
        }
        _choices[_currentFood] = choice;
    }

    // Получить выбор для конкретной еды (для GameManager)
    public FoodInspectionChoice GetChoice(FoodData food)
    {
        SaveCurrentChoice();
        return _choices.ContainsKey(food) ? _choices[food] : null;
    }

    // ════════════════════════════════════════════════════════════
    // Анимация
    // ════════════════════════════════════════════════════════════
    public async Task Toggle()
    {
        if (_isAnimating) return;
        if (_isOpen) await SlideOut();
        else         await SlideIn();
    }

    async Task SlideIn()
    {
        _isAnimating = true;
        _isOpen      = true;
        await panelPosition.DOAnchorPosX(middlePosX, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
        _isAnimating = false;
    }

    async Task SlideOut()
    {
        _isAnimating = true;
        SaveCurrentChoice();
        await panelPosition.DOAnchorPosX(leftX, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
        _isOpen      = false;
        _isAnimating = false;
    }
}

// ════════════════════════════════════════════════════════════
// Выбор игрока для одной еды
// ════════════════════════════════════════════════════════════
public class FoodInspectionChoice
{
    public HashSet<SenseType>          activeSenses   = new();
    public Dictionary<SenseType, int>  selectedAction = new();
}
