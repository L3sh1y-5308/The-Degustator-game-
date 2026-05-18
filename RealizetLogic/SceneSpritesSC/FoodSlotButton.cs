// FoodSlotButton.cs
// Вешается на тот же GameObject что и ItemSlot.
// Требует Button компонент — Unity сам обрабатывает клик.

using UnityEngine;
using UnityEngine.UI;
using Degustation;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(ItemSlot))]
public class FoodSlotButton : MonoBehaviour
{
    [SerializeField] private SlidePanel slidePanel;

    private Button _button;
    private ItemSlot _slot;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _slot = GetComponent<ItemSlot>();

        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        FoodData food = _slot.GetFood();
        if (food == null) return;

        slidePanel.ToggleWithFood(food);
    }
}