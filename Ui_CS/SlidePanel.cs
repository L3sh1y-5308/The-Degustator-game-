// SlidePanel.cs
// Сайдпанель инспекции еды.
// Показывает иконку еды, 5 строк чувств (Toggle + Dropdown).
// Сохраняет выбор per-food. Поинты общие на сессию.

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
        if (Input.GetKeyDown(KeyCode.F) && !_isAnimating)
            ToggleVoid();
    }

    // ════════════════════════════════════════════════════════════
    // Открыть панель с конкретной едой
    // ════════════════════════════════════════════════════════════
    public void ToggleWithFood(FoodData food)
    {
        // сохраняем предыдущий выбор
        SaveCurrentChoice();

        _currentFood    = food;
        foodIcon.sprite = food.shopIcon;
        foodIcon.color  = Color.white;

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

        _ = Toggle();
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
