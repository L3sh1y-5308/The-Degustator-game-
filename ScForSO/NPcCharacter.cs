using UnityEngine;
using Degustation;

[CreateAssetMenu(fileName = "NPcCharacter", menuName = "SO/NPcCharacter")]
public class NPcCharacter : ScriptableObject
{
    public enum NPCFaceExpression
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Surprised
    }

    // ── Привязка спрайта к enum ──────────────────────────────────
    // Один struct вместо 5 отдельных полей.
    // В инспекторе видишь список: Expression + Sprite рядом.
    [System.Serializable]
    public struct ExpressionSprite
    {
        public NPCFaceExpression expression;
        public Sprite sprite;
    }

    [Header("NPC Sprites")]
    public ExpressionSprite[] sprites;

    // Получить спрайт по enum — вызывай в UI/диалоговой системе
    public Sprite GetSprite(NPCFaceExpression expression)
    {
        foreach (var entry in sprites)
            if (entry.expression == expression)
                return entry.sprite;
        return null;
    }

    // ── Реплики привязанные к эмоции ────────────────────────────
    // Каждая реплика несёт свою эмоцию → спрайт меняется автоматически
    [System.Serializable]
    public struct DialogueLine
    {
        [TextArea(1, 3)]
        public string text;
        public NPCFaceExpression expression; // какой спрайт показать при этой реплике
    }

    [Header("NPC Dialogue")]
    public DialogueLine[] dialogueLines;

    // ── Еда ─────────────────────────────────────────────────────
    // Список блюд вместо одного — NPC может реагировать на разные
    [Header("NPC Special Food")]
    public FoodData[] specialFoods;
}