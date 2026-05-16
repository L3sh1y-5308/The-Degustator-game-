using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    // Тумблер доступен в инспекторе. По умолчанию место свободно (true)
    [SerializeField] private bool spaceFree = true;

    // Ссылка на компонент отображения картинки на этом объекте
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Публичное свойство, чтобы скрипт SceneEventHand мог проверять статус места
    public bool IsSpaceFree
    {
        get { return spaceFree; }
        set { spaceFree = value; }
    }

    // Метод, который мы вызовем из главного скрипта, чтобы занять место и показать иконку
    public void PlaceSprite(Sprite newIcon)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newIcon; // Устанавливаем картинку
            spaceFree = false;               // Тумблер переключается в "занято"
        }
    }

    // Метод, чтобы очистить место
    public void ClearSlot()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null; // Убираем картинку
            spaceFree = true;             // Тумблер переключается в "свободно"
        }
    }
}
