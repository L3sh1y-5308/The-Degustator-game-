// FoodDragHandler.cs
// Drag & Drop еды между Food-слотами на Canvas.
// Вешается на префаб Food-слота вместе с ItemSlot.
//
// Требования на сцене:
//   - Canvas с GraphicRaycaster
//   - EventSystem
//   - Image.raycastTarget = true на слотах

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Image), typeof(ItemSlot))]
public class FoodDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Корневой Canvas (для размещения ghost поверх всего)")]
    [SerializeField] private Canvas rootCanvas;

    private Image    _image;
    private ItemSlot _slot;
    private GameObject    _ghost;
    private RectTransform _ghostRect;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _slot  = GetComponent<ItemSlot>();
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_slot.GetFood() == null) return;

        // Создаём ghost — летит за курсором поверх всего
        _ghost = new GameObject("DragGhost");
        _ghost.transform.SetParent(rootCanvas.transform, false);
        _ghost.transform.SetAsLastSibling();

        var ghostImg           = _ghost.AddComponent<Image>();
        ghostImg.sprite        = _image.sprite;
        ghostImg.preserveAspect = true;
        ghostImg.raycastTarget = false; // не мешает raycast на слоты

        _ghostRect           = _ghost.GetComponent<RectTransform>();
        _ghostRect.sizeDelta = ((RectTransform)transform).sizeDelta;

        // Полупрозрачный оригинал пока тащим
        _image.color = new Color(1, 1, 1, 0.3f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghostRect == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventData.pressEventCamera,
            out Vector2 localPos);
        _ghostRect.localPosition = localPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_ghost != null) Destroy(_ghost);

        var target = GetSlotUnderPointer(eventData);

        if (target != null && target != _slot && target.slotType == ItemSlot.SlotType.Food)
        {
            // Swap
            var myFood     = _slot.GetFood();
            var targetFood = target.GetFood();
            _slot.SetFood(targetFood);
            target.SetFood(myFood);
        }

        _image.color = Color.white;
    }

    private ItemSlot GetSlotUnderPointer(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            var slot = hit.gameObject.GetComponent<ItemSlot>();
            if (slot != null) return slot;
        }
        return null;
    }
}
