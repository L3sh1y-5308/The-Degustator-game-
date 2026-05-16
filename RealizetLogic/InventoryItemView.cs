// InventoryItemView.cs
// UI-компонент на префабе предмета инвентаря (свитки, зелья, инструменты).
// Image + клик через IPointerClickHandler (Canvas).

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Degustation;

[RequireComponent(typeof(Image))]
public class InventoryItemView : MonoBehaviour, IPointerClickHandler
{
    public ScrollItemData data;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.preserveAspect = true;
    }

    public void Init(ScrollItemData scroll)
    {
        data          = scroll;
        _image.sprite = scroll.icon;
        name          = $"[Inv] {scroll.displayName}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // InventoryManager.Instance.Use(data);
        Debug.Log($"[InventoryItemView] Использован: {data.displayName}");
    }
}
