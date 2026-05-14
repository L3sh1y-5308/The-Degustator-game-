// EndOfRoundWindow.cs
// Окно итогов раунда — заполняет таблицу по результатам проверки блюд.
//
// НАСТРОЙКА В UNITY:
//   1. Повесь этот скрипт на объект EndOfRoundWindow
//   2. Создай префаб ResultRow (структура описана в документации)
//   3. Назначь ссылки в инспекторе
//   4. Вызывай Show(results) из GameManager после завершения волны

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Degustation
{
    // ──────────────────────────────────────────────
    // Данные одной строки результата
    // Заполняется в GameManager при завершении волны
    // ──────────────────────────────────────────────
    public class RoundResult
    {
        public string dishName;       // название блюда
        public string playerAnswer;   // что ответил игрок
        public string correctAnswer;  // правильный ответ
        public int    score;          // очки (+ или -)
        public SenseType usedSense;   // каким чувством проверял
    }

    // ──────────────────────────────────────────────
    // Компонент окна итогов
    // ──────────────────────────────────────────────
    public class EndOfRoundWindow : MonoBehaviour
    {
        [Header("Ссылки на UI")]
        [Tooltip("Текст заголовка окна")]
        public TMP_Text titleText;

        [Tooltip("Transform контейнера строк (Content внутри ScrollView)")]
        public Transform rowContainer;

        [Tooltip("Префаб одной строки результата")]
        public GameObject rowPrefab;

        [Tooltip("Кнопка 'Далее' — переход к следующей волне или EndSessionWidget")]
        public Button nextButton;

        [Tooltip("Текст суммарных очков за раунд")]
        public TMP_Text totalScoreText;

        // Колбек — вызывается когда игрок нажал Далее
        // GameManager подписывается: window.OnNextPressed += StartWave / EndSession
        public event System.Action OnNextPressed;

        // Активные строки — чистим перед каждым показом
        private readonly List<GameObject> _rows = new();

        // ════════════════════════════════════════════════════════════
        // Показать окно с результатами
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Вызывай из GameManager: window.Show(results, waveNumber)
        /// </summary>
        public void Show(List<RoundResult> results, int waveNumber = 0)
        {
            gameObject.SetActive(true);

            // Заголовок
            if (titleText != null)
                titleText.text = waveNumber > 0
                    ? $"Итоги — Волна {waveNumber}"
                    : "Итоги раунда";

            // Очищаем старые строки
            ClearRows();

            // Заполняем строки
            int totalScore = 0;
            foreach (var result in results)
            {
                totalScore += result.score;
                SpawnRow(result);
            }

            // Суммарные очки
            if (totalScoreText != null)
            {
                totalScoreText.text = totalScore >= 0
                    ? $"Итого: +{totalScore}"
                    : $"Итого: {totalScore}";
                totalScoreText.color = totalScore >= 0 ? Color.green : Color.red;
            }

            // Привязываем кнопку Далее
            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(OnNextClicked);
            }
        }

        public void Hide() => gameObject.SetActive(false);

        // ════════════════════════════════════════════════════════════
        // Создать строку результата
        // ════════════════════════════════════════════════════════════
        void SpawnRow(RoundResult result)
        {
            if (rowPrefab == null || rowContainer == null)
            {
                Debug.LogWarning("[EndOfRoundWindow] rowPrefab или rowContainer не назначен!");
                return;
            }

            GameObject row = Instantiate(rowPrefab, rowContainer);
            _rows.Add(row);

            // Заполняем поля строки через вспомогательный компонент
            var rowView = row.GetComponent<ResultRowView>();
            if (rowView != null)
            {
                rowView.Fill(result);
            }
            else
            {
                // Fallback: ищем TMP_Text по именам дочерних объектов
                FillRowFallback(row, result);
            }
        }

        // Fallback — заполняет строку по именам дочерних объектов
        // Используй если не хочешь делать отдельный компонент ResultRowView
        void FillRowFallback(GameObject row, RoundResult result)
        {
            SetChildText(row, "DishName",      result.dishName);
            SetChildText(row, "PlayerAnswer",  result.playerAnswer);
            SetChildText(row, "CorrectAnswer", result.correctAnswer);

            bool correct = result.score > 0;

            // Очки — цвет зелёный/красный
            var scoreGO = row.transform.Find("ScoreText");
            if (scoreGO != null)
            {
                var tmp = scoreGO.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    tmp.text  = correct ? $"+{result.score}" : $"{result.score}";
                    tmp.color = correct ? Color.green : Color.red;
                }
            }

            // Иконка результата (Image) — меняем цвет: зелёный/красный
            var iconGO = row.transform.Find("ResultIcon");
            if (iconGO != null)
            {
                var img = iconGO.GetComponent<Image>();
                if (img != null)
                    img.color = correct ? Color.green : Color.red;
            }
        }

        static void SetChildText(GameObject parent, string childName, string text)
        {
            var child = parent.transform.Find(childName);
            if (child == null) return;
            var tmp = child.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = text;
        }

        // ════════════════════════════════════════════════════════════
        // Очистка
        // ════════════════════════════════════════════════════════════
        void ClearRows()
        {
            foreach (var row in _rows)
                if (row != null) Destroy(row);
            _rows.Clear();
        }

        void OnNextClicked()
        {
            Hide();
            OnNextPressed?.Invoke();
        }
    }

    // ──────────────────────────────────────────────
    // Компонент на префабе ResultRow
    // Альтернатива FallbackFill — явные ссылки в инспекторе
    // ──────────────────────────────────────────────
    public class ResultRowView : MonoBehaviour
    {
        public TMP_Text dishNameText;
        public TMP_Text playerAnswerText;
        public TMP_Text correctAnswerText;
        public TMP_Text scoreText;
        public Image    resultIcon;

        [Header("Иконки результата")]
        public Sprite correctSprite;
        public Sprite wrongSprite;

        public void Fill(RoundResult result)
        {
            bool correct = result.score > 0;

            if (dishNameText     != null) dishNameText.text     = result.dishName;
            if (playerAnswerText != null) playerAnswerText.text = result.playerAnswer;
            if (correctAnswerText!= null) correctAnswerText.text= result.correctAnswer;

            if (scoreText != null)
            {
                scoreText.text  = correct ? $"+{result.score}" : $"{result.score}";
                scoreText.color = correct ? Color.green : Color.red;
            }

            if (resultIcon != null)
            {
                resultIcon.sprite = correct ? correctSprite : wrongSprite;
                // Если спрайты не назначены — меняем хотя бы цвет
                if (correctSprite == null && wrongSprite == null)
                    resultIcon.color = correct ? Color.green : Color.red;
            }
        }
    }
}
