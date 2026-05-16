// ItemSlot.cs
// Универсальный слот для Canvas UI.
// Использует Image вместо SpriteRenderer.
// Режимы: Food (drag&drop), NPC (смена эмоций), Organ (по HP из SenseStatsData).

using UnityEngine;
using UnityEngine.UI;
using Degustation;

[RequireComponent(typeof(Image))]
public class ItemSlot : MonoBehaviour
{
    // Тип слота — задаётся из SceneEventHand при инициализации
    public enum SlotType { Food, NPC, Organ }

    [Header("Тип слота")]
    public SlotType slotType = SlotType.Food;

    [Header("NPC данные (если SlotType = NPC)")]
    public NPcCharacter npcData;

    [Header("Орган (если SlotType = Organ)")]
    public SenseType organSense = SenseType.Vision;
    public SenseStatsData playerStats;

    // Публичное состояние
    public bool IsSpaceFree { get; private set; } = true;

    // Image вместо SpriteRenderer — работает на Canvas
    private Image    _image;
    private FoodData _currentFood;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.preserveAspect = true;
    }

    private void OnEnable()
    {
        if (slotType == SlotType.Organ && playerStats != null)
            playerStats.OnOrganDamaged += OnOrganDamaged;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnOrganDamaged -= OnOrganDamaged;
    }

    // ── Food API ──────────────────────────────────────────────────

    /// Положить еду в слот (null = очистить)
    public void PlaceSprite(Sprite sprite)
    {
        _image.sprite = sprite;
        _image.color  = sprite != null ? Color.white : new Color(1, 1, 1, 0.25f);
        IsSpaceFree   = sprite == null;
    }

    public void SetFood(FoodData food)
    {
        _currentFood = food;
        PlaceSprite(food != null ? food.iconSmall : null);
    }

    public FoodData GetFood() => _currentFood;

    public void ClearSlot()
    {
        _currentFood  = null;
        _image.sprite = null;
        _image.color  = new Color(1, 1, 1, 0.25f);
        IsSpaceFree   = true;
    }

    // ── NPC API ───────────────────────────────────────────────────

    /// Сменить эмоцию NPC — спрайт меняется, позиция не меняется
    public void SetNPCExpression(NPcCharacter.NPCFaceExpression expression)
    {
        if (npcData == null) return;
        var sprite = npcData.GetSprite(expression);
        _image.sprite = sprite;
        _image.color  = Color.white;
        IsSpaceFree   = false;
    }

    // ── Organ: автообновление по HP ───────────────────────────────

    private void OnOrganDamaged(DamageEvent evt)
    {
        if (evt.Organ != organSense) return;
        RefreshOrganSprite();
    }

    public void RefreshOrganSprite()
    {
        if (playerStats == null) return;
        var sprite    = playerStats.GetCurrentSprite(organSense);
        _image.sprite = sprite;
        _image.color  = Color.white;
        IsSpaceFree   = false;
    }
}
