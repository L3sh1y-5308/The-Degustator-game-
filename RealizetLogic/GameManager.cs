// GameManager.cs
// Центральный менеджер игровой сессии.
// Делегирует спавн TastedItemSpawner, переживает смену сцен через DontDestroyOnLoad,
// после каждой загрузки сцены ищет UI-объекты через FindObjectOfType / Find.

using System.Collections;
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
        [Tooltip("SO статов персонажа — единый источник правды для HP органов")]
        public SenseStatsData senseStats;

        [Tooltip("SO прогресса игрока (деньги, репутация, XP)")]
        public PlayerScore playerScore;

        [Tooltip("SO таблицы урона")]
        public InspectionDamageTable damageTable;

        // ── Настройки раунда ───────────────────────────────────────
        [Header("Round Settings")]
        [Tooltip("Количество раундов (волн) в одной сессии")]
        [Range(1, 10)]
        public int totalWaves = 3;

        // ── Имена сцен ─────────────────────────────────────────────
        [Header("Scenes")]
        public string gameSceneName = "GameScene";
        public string shopSceneName = "ShopScene";

        // ── Рантайм-состояние ──────────────────────────────────────
        private int  _currentWave   = 0;
        private bool _sessionActive = false;

        // Ссылки на компоненты текущей сцены — переназначаются после загрузки
        private TastedItemSpawner _spawner;
        private Button            _confirmButton;
        private GameObject        _resultsPanel;
        private GameObject        _endSessionWidget;
        private Button            _continueButton;
        private Button            _shopButton;

        // ════════════════════════════════════════════════════════════
        // Singleton
        // ════════════════════════════════════════════════════════════
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            // Слушаем загрузку новой сцены, чтобы переназначить UI
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ════════════════════════════════════════════════════════════
        // После загрузки любой сцены — найти и привязать объекты
        // Имена объектов в иерархии должны совпадать с тегами ниже
        // ════════════════════════════════════════════════════════════
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindSceneObjects();

            // Игровая сцена → стартуем сессию
            if (scene.name == gameSceneName)
                StartSession();
        }

        void BindSceneObjects()
        {
            // TastedItemSpawner — ищем в сцене (один экземпляр)
            _spawner = FindObjectOfType<TastedItemSpawner>();
            if (_spawner == null)
                Debug.LogWarning("[GameManager] TastedItemSpawner не найден в сцене!");

            // UI — ищем по имени объекта.
            // ВАЖНО: имена объектов в иерархии должны совпадать:
            //   "ConfirmButton", "ResultsPanel", "EndSessionWidget",
            //   "ContinueButton", "ShopButton"
            _confirmButton    = FindButton("ConfirmButton");
            _continueButton   = FindButton("ContinueButton");
            _shopButton       = FindButton("ShopButton");
            _resultsPanel     = GameObject.Find("ResultsPanel");
            _endSessionWidget = GameObject.Find("EndSessionWidget");

            // Скрываем панели
            _resultsPanel?.SetActive(false);
            _endSessionWidget?.SetActive(false);

            // Привязываем кнопки (RemoveAllListeners — чтобы не дублировались)
            BindButton(_confirmButton,  OnConfirmPressed);
            BindButton(_continueButton, OnContinuePressed);
            BindButton(_shopButton,     OnShopPressed);
        }

        // ════════════════════════════════════════════════════════════
        // Сессия
        // ════════════════════════════════════════════════════════════
        public void StartSession()
        {
            _currentWave  = 0;
            _sessionActive = true;
            StartWave();
        }

        void StartWave()
        {
            _currentWave++;
            Debug.Log($"[GameManager] Волна {_currentWave}/{totalWaves}");

            // Делегируем спавн — GameManager сам не создаёт объекты
            if (_spawner != null)
                _spawner.SpawnRandomItems();
            else
                Debug.LogWarning("[GameManager] Нет спавнера — пропускаем спавн.");

            SetConfirmInteractable(true);
        }

        // ════════════════════════════════════════════════════════════
        // Кнопка подтверждения
        // ════════════════════════════════════════════════════════════
        void OnConfirmPressed()
        {
            if (!_sessionActive) return;
            SetConfirmInteractable(false);

            // TODO: здесь передай результат проверки из UI в CalculateScore
            int score = CalculateScore();

            // Начисляем XP и репутацию через PlayerScore SO
            playerScore?.AddXP(score);
            if (score > 0) playerScore?.AddReputation(1);

            Debug.Log($"[GameManager] Волна {_currentWave} завершена. Очки: {score}");

            playerScore?.Save();
            ShowWaveResults();
        }

        // ════════════════════════════════════════════════════════════
        // Подсчёт очков — подключи свою логику
        // ════════════════════════════════════════════════════════════
        int CalculateScore()
        {
            // TODO: пройти по TastedItem-ам из _spawner.GetActiveItems(),
            // сравнить с ответами игрока (через DropDown/UI) и вернуть сумму
            return Random.Range(0, 100);
        }

        // ════════════════════════════════════════════════════════════
        // Результаты волны
        // ════════════════════════════════════════════════════════════
        void ShowWaveResults()
        {
            _resultsPanel?.SetActive(true);
            StartCoroutine(WaitForResultsDismiss());
        }

        IEnumerator WaitForResultsDismiss()
        {
            // Простой таймер — замени на событие "игрок нажал Далее"
            yield return new WaitForSeconds(2f);
            _resultsPanel?.SetActive(false);

            bool hasMoreWaves = _currentWave < totalWaves;
            if (hasMoreWaves)
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
            // BindSceneObjects + StartSession вызовутся автоматически через OnSceneLoaded
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
