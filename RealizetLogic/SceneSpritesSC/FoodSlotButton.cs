// FoodSlotButton.cs
// Вешается на тот же GameObject что и ItemSlot.
// Требует Button компонент — Unity сам обрабатывает клик.

using UnityEngine;
using UnityEngine.UI;
using Degustation;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(ItemSlot))]
public class FoodSlotButton : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private SlidePanel slidePanel;

    private Button _button;
    private ItemSlot _slot;
    private bool _isDragging = false;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _slot = GetComponent<ItemSlot>();
    }

    public void OnBeginDrag(PointerEventData eventData) => _isDragging = true;

    public void OnEndDrag(PointerEventData eventData) => _isDragging = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging) return;
        FoodData food = _slot.GetFood();
        if (food == null) return;

        slidePanel.ToggleWithFood(food);
    }
}