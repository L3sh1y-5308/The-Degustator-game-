using UnityEngine;
using UnityEngine.UI; // Исправлено: правильный импорт UI
using System.Collections.Generic; // Добавлено: для работы List<>



public class SceneEventHand : MonoBehaviour
{
    public enum ItemTag
    {
        NPC,
        Food
    }

    [SerializeField] public List<ItemSlot> PointScpriteRenderer;
    [SerializeField] public SceneEventHand Scenes;


    public ItemTag FealdMod = ItemTag.Food;

    public bool isDragble = false;
    public bool isPlaceble = false;
    public bool spaceFree = false;

    public void OnMouseDown()
    {
        if (isDragble)
        {
            // Здесь была логика isDragble = true, если он уже true. 
            // Возможно, вы хотели переключать его или активировать перетаскивание.
            isDragble = true;
        }
    }

    public void PlaceForSprite()
    {
        // Исправлено: корректная проверка перечисления
        if (FealdMod == ItemTag.Food)
        {
            isPlaceble = false;
        }


        if (FealdMod == ItemTag.NPC)
        {
            isPlaceble = false;
        }

    }

    void Start()
    {

    }

    void Update()
    {

    }
}
