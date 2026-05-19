// ShopItemView.cs
// UI-компонент на префабе товара в магазине.
// Image + клик через IPointerClickHandler (Canvas).

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Degustation;

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
        data = item;
        _image.sprite = item.icon;
        _image.color = item.stock == 0 ? new Color(1, 1, 1, 0.3f) : Color.white; // серый если нет в наличии
        name = $"[Shop] {item.displayName}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (data.stock == 0) return; // нет в наличии — игнорируем клик
        // ShopManager.Instance.TryBuy(data.id);
        Debug.Log($"[ShopItemView] Клик: {data.displayName} ({data.price}g)");
    }
}
