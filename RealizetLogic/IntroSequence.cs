// IntroSequence.cs
// Управляет вступлением: фон зала → NPC диалог → переход к столу.
// Вешается на любой GameObject в сцене (например IntroManager).
//
// Структура Canvas:
//   IntroView (этот RectTransform уезжает вверх при переходе)
//     ├── BackgroundImage  ← Image с фото зала
//     └── NpcImage         ← Image NPC (появляется снизу)
//   DialoguePanel          ← окно с текстом
//     ├── DialogueText (TMP)
//     └── HintText (TMP)   ← "[ Пробел ]"
//   GameVw                 ← стол (стартует ниже экрана)

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Degustation;

public class IntroSequence : MonoBehaviour
{
    [Header("Intro View — уезжает вверх при переходе")]
    [SerializeField] private RectTransform introView;     // родитель фона и NPC
    [SerializeField] private Image         background;    // Image зала
    [SerializeField] private Image         npcImage;      // Image NPC

    [Header("Диалог")]
    [SerializeField] private GameObject    dialoguePanel;
    [SerializeField] private TMP_Text      dialogueText;
    [SerializeField] private TMP_Text      hintText;      // "[ Пробел ]"

    [Header("Стол — въезжает снизу")]
    [SerializeField] private RectTransform gameView;      // GameVw

    [Header("NPC данные")]
    [SerializeField] private NPcCharacter  firstNpc;      // вельможа/гость
    [SerializeField] private NPcCharacter  servantNpc;    // слуга

    [Header("Тайминги")]
    [SerializeField] private float npcSlideDuration  = 0.6f;  // NPC въезжает
    [SerializeField] private float transitionDuration = 0.8f; // переход к столу
    [SerializeField] private float talkInterval      = 0.25f; // интервал анимации рта
    [SerializeField] private float npcHiddenY        = -400f; // стартовая позиция NPC
    [SerializeField] private float npcVisibleY       = -80f;  // видимая позиция NPC
    [SerializeField] private float screenHeight      = 1080f; // высота Canvas

    private bool _waitingForInput = false;
    private bool _spacePressed    = false;
    private bool _talkingActive   = false;

    // ════════════════════════════════════════════════════════════
    void Start()
    {
        // Прячем стол за нижним краем экрана
        gameView.anchoredPosition = new Vector2(
            gameView.anchoredPosition.x, -screenHeight);

        // Прячем NPC
        npcImage.rectTransform.anchoredPosition = new Vector2(
            npcImage.rectTransform.anchoredPosition.x, npcHiddenY);
        npcImage.color = new Color(1, 1, 1, 0);

        dialoguePanel.SetActive(false);

        _ = RunIntro();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _waitingForInput)
        {
            _spacePressed = true;
            _waitingForInput = false;
        }
    }

    // ════════════════════════════════════════════════════════════
    // Главная последовательность
    // ════════════════════════════════════════════════════════════
    async Task RunIntro()
    {
        // 1. Показываем фон — fade in
        background.color = new Color(1, 1, 1, 0);
        await background.DOFade(1f, 0.8f).AsyncWaitForCompletion();
        await Task.Delay(600);

        // 2. Первый NPC въезжает снизу
        await ShowNpc(firstNpc);

        // 3. Диалог первого NPC
        await RunDialogue(firstNpc);

        // 4. Первый NPC уезжает
        await HideNpc();
        await Task.Delay(400);

        // 5. Слуга въезжает
        await ShowNpc(servantNpc);

        // 6. Диалог слуги (одна реплика)
        await RunDialogue(servantNpc);

        // 7. Слуга уезжает
        await HideNpc();
        await Task.Delay(300);

        // 8. Переход к столу
        await TransitionToTable();
    }

    // ════════════════════════════════════════════════════════════
    // NPC появляется / исчезает
    // ════════════════════════════════════════════════════════════
    async Task ShowNpc(NPcCharacter npc)
    {
        // Устанавливаем нейтральный спрайт
        npcImage.sprite = npc.GetSprite(NPcCharacter.NPCFaceExpression.Neutral);
        npcImage.color  = new Color(1, 1, 1, 0);
        npcImage.rectTransform.anchoredPosition = new Vector2(
            npcImage.rectTransform.anchoredPosition.x, npcHiddenY);

        // Въезжает снизу + fade in одновременно
        npcImage.rectTransform
            .DOAnchorPosY(npcVisibleY, npcSlideDuration)
            .SetEase(Ease.OutQuart);
        await npcImage
            .DOFade(1f, npcSlideDuration)
            .AsyncWaitForCompletion();
    }

    async Task HideNpc()
    {
        _talkingActive = false;
        npcImage.rectTransform
            .DOAnchorPosY(npcHiddenY, npcSlideDuration)
            .SetEase(Ease.InQuart);
        await npcImage
            .DOFade(0f, npcSlideDuration)
            .AsyncWaitForCompletion();
    }

    // ════════════════════════════════════════════════════════════
    // Диалог — перебираем реплики по Пробелу
    // ════════════════════════════════════════════════════════════
    async Task RunDialogue(NPcCharacter npc)
    {
        if (npc.dialogueLines == null || npc.dialogueLines.Length == 0) return;

        dialoguePanel.SetActive(true);
        if (hintText != null) hintText.text = "[ Пробел ]";

        foreach (var line in npc.dialogueLines)
        {
            // Меняем спрайт по эмоции
            var sprite = npc.GetSprite(line.expression);
            if (sprite != null) npcImage.sprite = sprite;

            // Анимация рта — пока текст "печатается"
            _ = AnimateTalking(npc, line.expression);

            // Печатаем текст посимвольно
            await TypeText(line.text);

            // Останавливаем анимацию рта
            _talkingActive = false;

            // Ждём Пробел
            await WaitForSpace();
        }

        dialoguePanel.SetActive(false);
    }

    // Посимвольный вывод текста
    async Task TypeText(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            await Task.Delay(30);
            // Если нажали пробел во время печати — выводим всё сразу
            if (_spacePressed)
            {
                _spacePressed    = false;
                dialogueText.text = text;
                return;
            }
        }
    }

    // Анимация рта — чередует Neutral и текущую эмоцию
    async Task AnimateTalking(NPcCharacter npc, NPcCharacter.NPCFaceExpression expression)
    {
        _talkingActive = true;
        bool toggle = false;
        while (_talkingActive)
        {
            var expr = toggle
                ? NPcCharacter.NPCFaceExpression.Neutral
                : expression;
            var sprite = npc.GetSprite(expr);
            if (sprite != null) npcImage.sprite = sprite;
            toggle = !toggle;
            await Task.Delay((int)(talkInterval * 1000));
        }
        // Возвращаем основную эмоцию после анимации
        var finalSprite = npc.GetSprite(expression);
        if (finalSprite != null) npcImage.sprite = finalSprite;
    }

    // Ждём нажатия Пробела
    async Task WaitForSpace()
    {
        _spacePressed    = false;
        _waitingForInput = true;
        while (_waitingForInput)
            await Task.Yield();
    }

    // ════════════════════════════════════════════════════════════
    // Переход к столу — IntroView уезжает вверх, GameVw въезжает
    // ════════════════════════════════════════════════════════════
    async Task TransitionToTable()
    {
        // IntroView уезжает вверх (эффект взгляда вниз)
        introView
            .DOAnchorPosY(screenHeight, transitionDuration)
            .SetEase(Ease.InOutQuart);

        // GameVw въезжает снизу одновременно
        await gameView
            .DOAnchorPosY(0f, transitionDuration)
            .SetEase(Ease.InOutQuart)
            .AsyncWaitForCompletion();

        // Отключаем introView — больше не нужен
        introView.gameObject.SetActive(false);

        // Запускаем игровую сессию
        GameManager.Instance?.StartSession();
    }
}
