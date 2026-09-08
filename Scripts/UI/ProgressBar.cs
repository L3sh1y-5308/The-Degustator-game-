// ProgressBar.cs
// UI ProgressBar для Canvas. Исправлена утечка подписок (lambda → именованные методы).

using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MyGame.UI
{
    [ExecuteInEditMode]
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

                var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bg.transform.SetParent(obj.transform, false);
                bg.GetComponent<Image>().color = Color.gray;

                var maskObj            = new GameObject("Mask", typeof(RectTransform), typeof(Image));
                maskObj.transform.SetParent(obj.transform, false);
                Image maskImg          = maskObj.GetComponent<Image>();
                maskImg.type           = Image.Type.Filled;
                maskImg.fillMethod     = Image.FillMethod.Horizontal;
                maskImg.fillOrigin     = 0;

                ProgressBar pb = obj.GetComponent<ProgressBar>();
                pb.mask        = maskImg;

                var labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(obj.transform, false);
                var labelRect       = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, -0.6f);
                labelRect.anchorMax = new Vector2(1,  0f);
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                var tmp             = labelObj.GetComponent<TextMeshProUGUI>();
                tmp.alignment       = TextAlignmentOptions.Center;
                tmp.fontSize        = 14;
                pb.label            = tmp;
            }

            GameObject parent = Selection.activeGameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    var canvasObj = new GameObject("Canvas",
                        typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        public int   minimum = 0;
        public int   maximum = 100;
        public Image mask;
        public int   current = 0;

        [Header("Текст под баром")]
        public TextMeshProUGUI label;
        public string labelFormat = "Уровень {level}  •  {current} / {maximum} XP  ({percent}%)";

        [Header("Привязка к PlayerScore (опционально)")]
        public PlayerScore playerScore;

        // FIX: именованные методы — отписка работает корректно
        private void OnXPChanged(int _)  => RefreshFromScore();
        private void OnLevelUp(int _)    => RefreshFromScore();

        void OnEnable()
        {
            if (playerScore != null)
            {
                playerScore.OnXPChanged += OnXPChanged;
                playerScore.OnLevelUp   += OnLevelUp;
                RefreshFromScore();
            }
        }

        void OnDisable()
        {
            if (playerScore != null)
            {
                playerScore.OnXPChanged -= OnXPChanged;
                playerScore.OnLevelUp   -= OnLevelUp;
            }
        }

        private void RefreshFromScore()
        {
            if (playerScore == null) return;
            minimum = 0;
            maximum = playerScore.XPToNextLevel;
            current = playerScore.XPCurrentLevel;
            UpdateVisuals();
        }

        void Update()
        {
            if (playerScore == null) UpdateVisuals();
#if UNITY_EDITOR
            if (!Application.isPlaying && playerScore != null) RefreshFromScore();
#endif
        }

        private void UpdateVisuals() { UpdateFill(); UpdateLabel(); }

        private void UpdateFill()
        {
            if (mask == null) return;
            float range = maximum - minimum;
            mask.fillAmount = range <= 0 ? 0 : Mathf.Clamp01((current - minimum) / range);
        }

        private void UpdateLabel()
        {
            if (label == null || string.IsNullOrEmpty(labelFormat)) return;
            float range   = maximum - minimum;
            int   percent = range > 0 ? Mathf.RoundToInt((current - minimum) / range * 100f) : 0;
            int   level   = playerScore != null ? playerScore.Level    : 0;
            int   maxLvl  = playerScore != null ? playerScore.MaxLevel : 0;
            label.text = labelFormat
                .Replace("{current}",  current.ToString())
                .Replace("{maximum}",  maximum.ToString())
                .Replace("{percent}",  percent.ToString())
                .Replace("{level}",    level.ToString())
                .Replace("{maxLevel}", maxLvl.ToString());
        }
    }
}
