// ShopItemView.cs
// UI-компонент на префабе товара в магазине.
// Image + клик через IPointerClickHandler (Canvas).

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class ShopItemView : MonoBehaviour, IPointerClickHandler
{
    public ShopInventory.ShopItem data;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.preserveAspect = true;
    }

    public void Init(ShopInventory.ShopItem item)
    {
        data          = item;
        _image.sprite = item.icon;
        name          = $"[Shop] {item.displayName}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // ShopManager.Instance.TryBuy(data.id);
        Debug.Log($"[ShopItemView] Клик: {data.displayName} ({data.price}g)");
    }
}
