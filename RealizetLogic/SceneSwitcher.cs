// SceneSwitcher.cs
// Универсальный переключатель сцен.
//
// СПОСОБЫ ИСПОЛЬЗОВАНИЯ:
//
//   1. Кнопка в инспекторе:
//      Button.OnClick → SceneSwitcher.GoToGame / GoToShop / GoToScene
//
//   2. Из кода любого скрипта:
//      SceneSwitcher.Instance.GoToScene("ShopScene");
//      SceneSwitcher.Instance.GoToGame();
//      SceneSwitcher.Instance.GoToShop();
//      SceneSwitcher.Instance.Reload();
//
//   3. С задержкой:
//      SceneSwitcher.Instance.GoToSceneDelayed("ShopScene", 1.5f);
//
// НАСТРОЙКА:
//   Вешается на любой GameObject (лучше на тот же что GameManager или отдельный).
//   Имена сцен задаются в инспекторе. Все сцены должны быть в Build Settings.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Degustation
{
    public class SceneSwitcher : MonoBehaviour
    {
        public static SceneSwitcher Instance { get; private set; }

        [Header("Имена сцен")]
        public string gameSceneName = "SampleScene";
        public string shopSceneName = "ShopScene";
        public string mainMenuSceneName = "MainMenu";

        [Header("Опционально: экран загрузки")]
        [Tooltip("Если назначен — показывается перед переходом")]
        public GameObject loadingScreen;

        // ── Singleton ─────────────────────────────────────────────
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ════════════════════════════════════════════════════════════
        // Публичные методы — вызывай из кода или вешай на Button.OnClick
        // ════════════════════════════════════════════════════════════

        /// Перейти в игровую сцену
        public void GoToGame()   => LoadScene(gameSceneName);

        /// Перейти в магазин
        public void GoToShop()   => LoadScene(shopSceneName);

        /// Перейти в главное меню
        public void GoToMenu()   => LoadScene(mainMenuSceneName);

        /// Перезагрузить текущую сцену
        public void Reload()     => LoadScene(SceneManager.GetActiveScene().name);

        /// Перейти в сцену по имени (для UnityEvent / Button.OnClick с параметром)
        public void GoToScene(string sceneName) => LoadScene(sceneName);

        /// Перейти с задержкой (например после анимации)
        public void GoToGameDelayed(float delay)         => StartCoroutine(LoadDelayed(gameSceneName, delay));
        public void GoToShopDelayed(float delay)         => StartCoroutine(LoadDelayed(shopSceneName, delay));
        public void GoToSceneDelayed(string name, float delay) => StartCoroutine(LoadDelayed(name, delay));

        // ════════════════════════════════════════════════════════════
        // Внутренняя логика
        // ════════════════════════════════════════════════════════════

        void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneSwitcher] Имя сцены пустое!");
                return;
            }

            if (loadingScreen != null)
                loadingScreen.SetActive(true);

            Debug.Log($"[SceneSwitcher] Переход → {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        IEnumerator LoadDelayed(string sceneName, float delay)
        {
            yield return new WaitForSeconds(delay);
            LoadScene(sceneName);
        }
    }
}
