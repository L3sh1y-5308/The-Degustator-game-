// HoverLabel.cs
// Маленькая подсказка с названием объекта под курсором.
// Screen-space Canvas, TMP_Text. Полностью опционально.

using TMPro;
using UnityEngine;

namespace Degustation
{
    public class HoverLabel : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private TMP_Text      label;

        [Tooltip("Смещение от курсора в пикселях")]
        [SerializeField] private Vector2 cursorOffset = new Vector2(18f, -18f);

        [Tooltip("Следовать за курсором. Выключи, если подсказка стоит в фиксированной точке")]
        [SerializeField] private bool followCursor = true;

        private void Awake()
        {
            if (root == null) root = transform as RectTransform;
            Show(null);
        }

        /// null или пустая строка — скрыть
        public void Show(string text)
        {
            bool visible = !string.IsNullOrEmpty(text);
            if (root != null) root.gameObject.SetActive(visible);
            if (visible && label != null) label.text = text;
        }

        private void LateUpdate()
        {
            if (!followCursor || root == null || !root.gameObject.activeSelf) return;
            root.position = (Vector3)InputCompat.MousePosition + (Vector3)cursorOffset;
        }
    }
}
