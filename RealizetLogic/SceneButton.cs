// SceneButton.cs
// Вспомогательный компонент — вешается прямо на GameObject с Button.
// Позволяет задать целевую сцену в инспекторе без написания кода.
//
// КАК ИСПОЛЬЗОВАТЬ:
//   1. Добавь компонент SceneButton на объект с кнопкой
//   2. Выбери targetScene из дропдауна (Game / Shop / Menu / Custom)
//   3. Если Custom — впиши имя сцены в customSceneName
//   4. Опционально задай delay
//   5. Кнопка сама подключится при старте — ничего больше не нужно

using UnityEngine;
using UnityEngine.UI;

namespace Degustation
{
    [RequireComponent(typeof(Button))]
    public class SceneButton : MonoBehaviour
    {
        public enum SceneTarget { Game, Shop, Menu, Reload, Custom }

        [Header("Куда переходить")]
        public SceneTarget targetScene = SceneTarget.Game;

        [Tooltip("Только если targetScene = Custom")]
        public string customSceneName = "";

        [Header("Задержка перед переходом (сек)")]
        [Range(0f, 5f)]
        public float delay = 0f;

        void Start()
        {
            var btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            if (SceneSwitcher.Instance == null)
            {
                Debug.LogWarning("[SceneButton] SceneSwitcher не найден на сцене!");
                return;
            }

            if (delay > 0f)
            {
                // С задержкой
                switch (targetScene)
                {
                    case SceneTarget.Game:   SceneSwitcher.Instance.GoToGameDelayed(delay); break;
                    case SceneTarget.Shop:   SceneSwitcher.Instance.GoToShopDelayed(delay); break;
                    case SceneTarget.Menu:   SceneSwitcher.Instance.GoToSceneDelayed(SceneSwitcher.Instance.mainMenuSceneName, delay); break;
                    case SceneTarget.Reload: SceneSwitcher.Instance.GoToSceneDelayed(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, delay); break;
                    case SceneTarget.Custom: SceneSwitcher.Instance.GoToSceneDelayed(customSceneName, delay); break;
                }
            }
            else
            {
                // Мгновенно
                switch (targetScene)
                {
                    case SceneTarget.Game:   SceneSwitcher.Instance.GoToGame(); break;
                    case SceneTarget.Shop:   SceneSwitcher.Instance.GoToShop(); break;
                    case SceneTarget.Menu:   SceneSwitcher.Instance.GoToMenu(); break;
                    case SceneTarget.Reload: SceneSwitcher.Instance.Reload();   break;
                    case SceneTarget.Custom: SceneSwitcher.Instance.GoToScene(customSceneName); break;
                }
            }
        }
    }
}
