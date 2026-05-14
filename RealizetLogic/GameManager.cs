// GameManager.cs
// Центральный менеджер игровой сессии.
// Делегирует спавн TastedItemSpawner, переживает смену сцен через DontDestroyOnLoad,
// после каждой загрузки сцены ищет UI-объекты через FindObjectOfType / Find.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Degustation
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── ScriptableObject-данные (назначь в инспекторе, живут между сценами) ──
        [Header("ScriptableObjects")]
        public SenseStatsData senseStats;
        public PlayerScore playerScore;
       // public InspectionDamageTable damageTable;

        // ── Настройки раунда ───────────────────────────────────────
        [Header("Round Settings")]
        [Range(1, 10)] public int totalWaves = 3;

        // ── Имена сцен ─────────────────────────────────────────────
        [Header("Scenes")]
        public string gameSceneName = "SampleScene";
        public string shopSceneName = "ShopScene";

        // ── Рантайм-состояние ──────────────────────────────────────
        private int _currentWave = 0;
        private bool _sessionActive = false;

        // Ссылки на объекты текущей сцены — переназначаются после загрузки
        private TastedItemSpawner _spawner;
        private Button _confirmButton;
        private EndOfRoundWindow _roundWindow;
        private GameObject _endSessionWidget;
        private Button _continueButton;
        private Button _shopButton;

        // Результаты текущей волны
        private List<RoundResult> _currentResults = new();

        // ════════════════════════════════════════════════════════════
        // Singleton
        // ════════════════════════════════════════════════════════════
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        // ════════════════════════════════════════════════════════════
        // После загрузки сцены — найти UI и стартовать если нужно
        // ════════════════════════════════════════════════════════════
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindSceneObjects();
            if (scene.name == gameSceneName)
                StartSession();
        }

        void BindSceneObjects()
        {
            _spawner = FindObjectOfType<TastedItemSpawner>();
            if (_spawner == null)
                Debug.LogWarning("[GameManager] TastedItemSpawner не найден!");

            // Кнопки — ищем по имени объекта в иерархии
            _confirmButton = FindButton("ConfirmButton");
            _continueButton = FindButton("ContinueButton");
            _shopButton = FindButton("ShopButton");

            // EndOfRoundWindow — ищем компонент (работает даже если объект скрыт)
            _roundWindow = FindObjectOfType<EndOfRoundWindow>(true);
            if (_roundWindow != null)
            {
                _roundWindow.OnNextPressed -= AfterRoundWindowClosed;
                _roundWindow.OnNextPressed += AfterRoundWindowClosed;
                _roundWindow.Hide();
            }
            else Debug.LogWarning("[GameManager] EndOfRoundWindow не найден!");

            // EndSessionWidget — пустой GameObject с двумя кнопками
            _endSessionWidget = GameObject.Find("EndSessionWidget");
            _endSessionWidget?.SetActive(false);

            BindButton(_confirmButton, OnConfirmPressed);
            BindButton(_continueButton, OnContinuePressed);
            BindButton(_shopButton, OnShopPressed);
        }

        // ════════════════════════════════════════════════════════════
        // Сессия
        // ════════════════════════════════════════════════════════════
        public void StartSession()
        {
            _currentWave = 0;
            _sessionActive = true;
            StartWave();
        }

        void StartWave()
        {
            _currentWave++;
            _currentResults.Clear();
            Debug.Log($"[GameManager] Волна {_currentWave}/{totalWaves}");

            if (_spawner != null)
                _spawner.SpawnRandomItems();
            else
                Debug.LogWarning("[GameManager] Нет спавнера!");

            SetConfirmInteractable(true);
        }

        // ════════════════════════════════════════════════════════════
        // Кнопка подтверждения — игрок проверил все блюда
        // ════════════════════════════════════════════════════════════
        void OnConfirmPressed()
        {
            if (!_sessionActive) return;
            SetConfirmInteractable(false);

            // Собираем результаты по активным блюдам
            CollectResults();

            // Суммируем очки
            int totalScore = 0;
            foreach (var r in _currentResults) totalScore += r.score;

            playerScore?.AddXP(totalScore);
            if (totalScore > 0) playerScore?.AddReputation(1);
            playerScore?.Save();

            Debug.Log($"[GameManager] Волна {_currentWave} завершена. Очки: {totalScore}");

            // Показываем окно итогов
            if (_roundWindow != null)
                _roundWindow.Show(_currentResults, _currentWave);
            else
                AfterRoundWindowClosed(); // окна нет — идём дальше сразу
        }

        // ════════════════════════════════════════════════════════════
        // Сбор результатов из TastedItem-ов
        // ════════════════════════════════════════════════════════════
        void CollectResults()
        {
            _currentResults.Clear();
            if (_spawner == null) return;

            foreach (var item in _spawner.GetActiveItems())
            {
                // TODO: получить реальный ответ игрока из UI (DropDown)
                // Сейчас — заглушка, правильный ответ из шаблона еды
                string correctAnswer = item.RuntimeFood?.source?.foodName ?? "?";
                string playerAnswer = correctAnswer; // заглушка — всегда верно
                int score = item.baseScore; // заглушка

                _currentResults.Add(new RoundResult
                {
                    dishName = item.displayName,
                    playerAnswer = playerAnswer,
                    correctAnswer = correctAnswer,
                    score = score,
                    usedSense = SenseType.Vision // заглушка
                });
            }
        }

        // ════════════════════════════════════════════════════════════
        // Вызывается когда игрок закрыл EndOfRoundWindow (нажал Далее)
        // ════════════════════════════════════════════════════════════
        void AfterRoundWindowClosed()
        {
            if (_currentWave < totalWaves)
                StartWave();
            else
                EndSession();
        }

        // ════════════════════════════════════════════════════════════
        // Конец сессии
        // ════════════════════════════════════════════════════════════
        void EndSession()
        {
            _sessionActive = false;
            _spawner?.ClearItems();
            playerScore?.Save();
            _endSessionWidget?.SetActive(true);
            Debug.Log("[GameManager] Сессия завершена.");
        }

        // ════════════════════════════════════════════════════════════
        // Кнопки финального виджета
        // ════════════════════════════════════════════════════════════
        void OnContinuePressed()
        {
            _endSessionWidget?.SetActive(false);
            SceneManager.LoadScene(gameSceneName);
        }

        void OnShopPressed()
        {
            _endSessionWidget?.SetActive(false);
            SceneManager.LoadScene(shopSceneName);
        }

        // ════════════════════════════════════════════════════════════
        // Утилиты
        // ════════════════════════════════════════════════════════════
        void SetConfirmInteractable(bool value)
        {
            if (_confirmButton != null) _confirmButton.interactable = value;
        }

        static Button FindButton(string objectName)
        {
            var go = GameObject.Find(objectName);
            return go != null ? go.GetComponent<Button>() : null;
        }

        static void BindButton(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null) return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }
}
