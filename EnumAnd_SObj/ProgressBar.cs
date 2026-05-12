using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MyGame.UI
{
    [ExecuteInEditMode()]
    public class ProgressBar : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("GameObject/UI/ProgressBar")]
        public static void AddProgressBar()
        {
            GameObject prefab = Resources.Load<GameObject>("UI/ProgressBar");
            GameObject obj;

            if (prefab != null)
            {
                obj = Instantiate(prefab);
            }
            else
            {
                obj = new GameObject("ProgressBar", typeof(RectTransform), typeof(ProgressBar));

                GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bg.transform.SetParent(obj.transform, false);
                bg.GetComponent<Image>().color = Color.gray;

                GameObject maskObj = new GameObject("Mask", typeof(RectTransform), typeof(Image));
                maskObj.transform.SetParent(obj.transform, false);
                Image maskImg = maskObj.GetComponent<Image>();
                maskImg.type = Image.Type.Filled;
                maskImg.fillMethod = Image.FillMethod.Horizontal;
                maskImg.fillOrigin = 0;

                ProgressBar pb = obj.GetComponent<ProgressBar>();
                pb.mask = maskImg;

                // Создаём текстовую метку под баром
                GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(obj.transform, false);
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, -0.6f);
                labelRect.anchorMax = new Vector2(1,  0f);
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 14;
                pb.label = tmp;

                Debug.LogWarning("Префаб не найден. Создан стандартный ProgressBar с лейблом.");
            }

            GameObject parent = Selection.activeGameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    canvas = canvasObj.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
                parent = canvas.gameObject;
            }

            obj.transform.SetParent(parent.transform, false);
            Undo.RegisterCreatedObjectUndo(obj, "Create ProgressBar");
            Selection.activeGameObject = obj;
        }
#endif

        public int minimum = 0;
        public int maximum = 100;
        public Image mask;
        public int current = 0;

        // ── Текстовая метка ──────────────────────────────────────────
        [Header("Текст под баром")]
        [Tooltip("TextMeshProUGUI объект, куда выводится подпись")]
        public TextMeshProUGUI label;

        [Tooltip("Формат строки. Поддерживаемые плейсхолдеры:\n{current} — текущее значение\n{maximum} — максимум\n{percent} — процент 0-100\n{level} — уровень (если привязан PlayerScore)\n{maxLevel} — макс. уровень")]
        public string labelFormat = "Уровень {level}  •  {current} / {maximum} XP  ({percent}%)";

        // ── Привязка к PlayerScore (опционально) ─────────────────────
        [Header("Привязка к PlayerScore (опционально)")]
        [Tooltip("Если задан, бар автоматически отображает прогресс XP текущего уровня")]
        public PlayerScore playerScore;

        void OnEnable()
        {
            if (playerScore != null)
                SubscribeToScore();
        }

        void OnDisable()
        {
            if (playerScore != null)
                UnsubscribeFromScore();
        }

        private void SubscribeToScore()
        {
            playerScore.OnXPChanged       += _ => RefreshFromScore();
            playerScore.OnLevelUp         += _ => RefreshFromScore();
        }

        private void UnsubscribeFromScore()
        {
            playerScore.OnXPChanged       -= _ => RefreshFromScore();
            playerScore.OnLevelUp         -= _ => RefreshFromScore();
        }

        // Синхронизировать minimum/maximum/current из PlayerScore
        private void RefreshFromScore()
        {
            if (playerScore == null) return;
            minimum = 0;
            maximum = playerScore.XPToNextLevel;   // XP нужно для след. уровня
            current = playerScore.XPCurrentLevel;  // XP набрано в текущем уровне
            UpdateVisuals();
        }

        void Update()
        {
            // В режиме редактора или если нет привязки — обновляем вручную
            if (playerScore == null)
                UpdateVisuals();

#if UNITY_EDITOR
            if (!Application.isPlaying && playerScore != null)
                RefreshFromScore();
#endif
        }

        private void UpdateVisuals()
        {
            UpdateFill();
            UpdateLabel();
        }

        private void UpdateFill()
        {
            if (mask == null) return;

            float currentOffset = current - minimum;
            float maximumOffset = maximum - minimum;
            if (maximumOffset <= 0) { mask.fillAmount = 0; return; }

            mask.fillAmount = Mathf.Clamp01(currentOffset / maximumOffset);
        }

        private void UpdateLabel()
        {
            if (label == null || string.IsNullOrEmpty(labelFormat)) return;

            float currentOffset = current - minimum;
            float maximumOffset = maximum - minimum;
            int percent = maximumOffset > 0 ? Mathf.RoundToInt(currentOffset / maximumOffset * 100f) : 0;

            int level    = playerScore != null ? playerScore.Level    : 0;
            int maxLevel = playerScore != null ? playerScore.MaxLevel : 0;

            string text = labelFormat
                .Replace("{current}",  current.ToString())
                .Replace("{maximum}",  maximum.ToString())
                .Replace("{percent}",  percent.ToString())
                .Replace("{level}",    level.ToString())
                .Replace("{maxLevel}", maxLevel.ToString());

            label.text = text;
        }
    }
}
