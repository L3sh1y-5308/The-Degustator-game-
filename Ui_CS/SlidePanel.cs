using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Degustation;

public class SlidePanel : MonoBehaviour
{
    [SerializeField] GameObject slidePanelObject;
    [SerializeField] RectTransform panelPosition;
    [SerializeField] float leftX, middlePosX;
    [SerializeField] float tweenDuration;
    [SerializeField] Image foodIcon;
    [SerializeField] private SenseRow[] senseRows;
    [SerializeField] private InspectionPoints inspectionPoints;

    private bool isAnimating = false;
    private bool isOpen = false;
    private Dictionary<FoodData, FoodInspectionChoice> _choices = new Dictionary<FoodData, FoodInspectionChoice>();
    private FoodData _currentFood;

    void Start()
    {
        slidePanelObject.SetActive(true);
        panelPosition.anchoredPosition = new Vector2(leftX, panelPosition.anchoredPosition.y);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isAnimating)
            ToggleVoid();
    }

    public void ToggleWithFood(FoodData food)
    {
        // сохраняем выбор предыдущей еды перед переключением
        SaveCurrentChoice();

        _currentFood = food;
        foodIcon.sprite = food.shopIcon;
        foodIcon.color = Color.white;

        var choice = _choices.ContainsKey(food)
            ? _choices[food]
            : new FoodInspectionChoice();

        foreach (var row in senseRows)
        {
            bool isOn = choice.activeSenses.Contains(row.senseType);
            int savedAction = choice.selectedAction.ContainsKey(row.senseType)
                ? choice.selectedAction[row.senseType] : 0;
            row.Setup(row.senseType, isOn, savedAction, inspectionPoints);
        }

        _ = Toggle();
    }

    public void ToggleVoid()
    {
        _ = Toggle();
    }

    public void SaveCurrentChoice()
    {
        if (_currentFood == null) return;
        var choice = new FoodInspectionChoice();
        foreach (var row in senseRows)
        {
            if (row.IsActive())
            {
                choice.activeSenses.Add(row.senseType);
                choice.selectedAction[row.senseType] = row.GetAction();
            }
        }
        _choices[_currentFood] = choice;
    }

    public async Task Toggle()
    {
        if (isAnimating) return;
        if (isOpen) await SlideOut();
        else await SlideIn();
    }

    async Task SlideIn()
    {
        isAnimating = true;
        isOpen = true;
        await panelPosition.DOAnchorPosX(middlePosX, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
        isAnimating = false;
    }

    async Task SlideOut()
    {
        isAnimating = true;
        await panelPosition.DOAnchorPosX(leftX, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
        isOpen = false;
        isAnimating = false;
    }
}

public class FoodInspectionChoice
{
    public HashSet<SenseType> activeSenses = new HashSet<SenseType>();
    public Dictionary<SenseType, int> selectedAction = new Dictionary<SenseType, int>();
}
