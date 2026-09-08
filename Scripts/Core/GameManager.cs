// GameManager.cs
// Центральный менеджер игровой сессии.
// Переживает смену сцен через DontDestroyOnLoad, после загрузки сцены
// заново находит спавнер, процессор проверки и UI.
//
// ЧТО ИЗМЕНИЛОСЬ ПРОТИВ 2D-ВЕРСИИ:
//   • спавнер теперь FoodSpawner3D (3D-модели на столе), а не Canvas-слоты;
//   • результаты раунда берутся из InspectionProcessor.ProcessAll(),
//     а не из заглушки, где playerAnswer = correctAnswer и score = 10;
//   • убран ReceiveInspectionResults — он чинил заглушку задним числом.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Degustation
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Очки инспекции")]
        public InspectionPoints inspectionPoints;

        [Header("ScriptableObjects")]
        public SenseStatsData senseStats;
        public PlayerScore    playerScore;

        [Header("Настройки раунда")]
        [Range(1, 10)] public int totalWaves = 3;

        [Header("Сцены")]
        public string gameSceneName = "SampleScene";
        public string shopSceneName = "ShopScene";

        // ── Рантайм-состояние ────────────────────────────────────
        private int  _currentWave   = 0;
        private bool _sessionActive = false;

        // Объекты текущей сцены — переназначаются после загрузки
        private FoodSpawner3D       _spawner;
        private InspectionProcessor _processor;
        private EndOfRoundWindow    _roundWindow;
        private GameObject          _endSessionWidget;
        private Button              _confirmButton;
        private Button              _continueButton;
        private Button              _shopButton;

        private readonly List<RoundResult> _currentResults = new();

        // ════════════════════════════════════════════════════════
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindSceneObjects();
            if (scene.name == gameSceneName)
                StartSession();
        }

        void BindSceneObjects()
        {
            _spawner   = Find<FoodSpawner3D>();
            _processor = Find<InspectionProcessor>();

            if (_spawner   == null) Debug.LogWarning("[GameManager] FoodSpawner3D не найден!");
            if (_processor == null) Debug.LogWarning("[GameManager] InspectionProcessor не найден!");

            _confirmButton  = FindButton("ConfirmButton");
            _continueButton = FindButton("ContinueButton");
            _shopButton     = FindButton("ShopButton");

            _roundWindow = FindIncludingInactive<EndOfRoundWindow>();
            if (_roundWindow != null)
            {
                _roundWindow.OnNextPressed -= AfterRoundWindowClosed;
                _roundWindow.OnNextPressed += AfterRoundWindowClosed;
                _roundWindow.Hide();
            }
            else Debug.LogWarning("[GameManager] EndOfRoundWindow не найден!");

            _endSessionWidget = GameObject.Find("EndSessionWidget");
            _endSessionWidget?.SetActive(false);

            BindButton(_confirmButton,  OnConfirmPressed);
            BindButton(_continueButton, OnContinuePressed);
            BindButton(_shopButton,     OnShopPressed);
        }

        // ════════════════════════════════════════════════════════
        // Сессия
        // ════════════════════════════════════════════════════════
        public void StartSession()
        {
            _currentWave   = 0;
            _sessionActive = true;
            inspectionPoints?.StartSession();
            StartWave();
        }

        void StartWave()
        {
            _currentWave++;
            _currentResults.Clear();
            Debug.Log($"[GameManager] Волна {_currentWave}/{totalWaves}");

            if (_spawner != null) _spawner.SpawnRandomItems();
            else Debug.LogWarning("[GameManager] Нет спавнера!");

            SetConfirmInteractable(true);
        }

        // ════════════════════════════════════════════════════════
        // Кнопка "Проверить" — игрок закончил осмотр
        // ════════════════════════════════════════════════════════
        void OnConfirmPressed()
        {
            if (!_sessionActive) return;
            SetConfirmInteractable(false);

            CollectResults();

            int totalScore = 0;
            foreach (var r in _currentResults) totalScore += r.score;

            playerScore?.AddXP(totalScore);
            if (totalScore > 0) playerScore?.AddReputation(1);
            playerScore?.Save();

            Debug.Log($"[GameManager] Волна {_currentWave} завершена. Очки: {totalScore}");

            if (_roundWindow != null) _roundWindow.Show(_currentResults, _currentWave);
            else                      AfterRoundWindowClosed();
        }

        // ════════════════════════════════════════════════════════
        // Реальные результаты из InspectionProcessor
        // ════════════════════════════════════════════════════════
        void CollectResults()
        {
            _currentResults.Clear();
            if (_processor == null) return;

            foreach (var r in _processor.ProcessAll())
            {
                if (r.food == null) continue;

                _currentResults.Add(new RoundResult
                {
                    dishName      = r.food.foodName,
                    correctAnswer = r.food.correctAction != null
                                    ? r.food.correctAction.displayName
                                    : r.food.targetSense.ToString(),
                    playerAnswer  = r.usedAction != null
                                    ? r.usedAction.displayName
                                    : r.grade.ToString(),
                    score         = r.score,
                    usedSense     = r.usedSense
                });
            }
        }

        void AfterRoundWindowClosed()
        {
            if (_currentWave < totalWaves) StartWave();
            else                           EndSession();
        }

        void EndSession()
        {
            _sessionActive = false;
            _spawner?.ClearItems();
            playerScore?.Save();
            _endSessionWidget?.SetActive(true);
            Debug.Log("[GameManager] Сессия завершена.");
        }

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

        // ════════════════════════════════════════════════════════
        // Утилиты
        // ════════════════════════════════════════════════════════
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

        static T Find<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        static T FindIncludingInactive<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
            return Object.FindObjectOfType<T>(true);
#endif
        }
    }
}
