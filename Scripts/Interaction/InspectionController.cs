// InspectionController.cs
// Режим осмотра блюда «как в Skyrim»:
//   клик по блюду → оно плавно уезжает к камере → крутим/приближаем
//   через Manipulation → ПКМ или Esc → блюдо возвращается на стол.
//
// НАСТРОЙКА СЦЕНЫ:
//   1. Пустой объект "InspectionController" с этим компонентом
//   2. Ребёнком камеры — пустышка "HoldAnchor" примерно на (0, 0, 0.55)
//   3. targetCamera = основная камера, holdAnchor = эта пустышка
//   4. slidePanel = панель чувств (Canvas), можно оставить пустым
//   5. disableWhileInspecting = скрипты движения/поворота камеры игрока
//   6. (опционально) Слой "Inspect" + отдельная near-камера,
//      чтобы блюдо не резалось стенами

using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Degustation
{
    public class InspectionController : MonoBehaviour
    {
        public static InspectionController Instance { get; private set; }

        [Header("Камера и точка удержания")]
        [SerializeField] private Camera    targetCamera;
        [Tooltip("Пустой объект-ребёнок камеры, ~0.5–0.7 м перед ней")]
        [SerializeField] private Transform holdAnchor;

        [Header("Анимация приближения")]
        [SerializeField] private float moveDuration = 0.45f;
        [SerializeField] private Ease  moveEase     = Ease.OutCubic;

        [Header("UI чувств (опционально)")]
        [SerializeField] private SlidePanel slidePanel;

        [Header("Отключить пока игрок рассматривает блюдо")]
        [Tooltip("Скрипты движения игрока, поворота камеры и т.п.")]
        [SerializeField] private MonoBehaviour[] disableWhileInspecting;

        [Header("Слой для предмета в руках")]
        [Tooltip("Пусто = не менять слой. Иначе создай слой с этим именем")]
        [SerializeField] private string inspectLayerName = "";

        [Header("Выход из осмотра")]
        [SerializeField] private bool exitOnRightClick = true;
        [SerializeField] private bool exitOnEscape     = true;

        // Обычный UnityEvent<T> не сериализуется — нужен конкретный наследник,
        // иначе поле просто не появится в инспекторе
        [System.Serializable] public class FoodItemEvent : UnityEvent<FoodItem> { }

        [Header("События")]
        public FoodItemEvent onInspectStarted;
        public FoodItemEvent onInspectEnded;

        // ── Состояние ────────────────────────────────────────────
        public FoodItem Current      { get; private set; }
        public bool     IsInspecting => Current != null;
        public bool     IsBusy       => _isAnimating;

        private bool       _isAnimating;
        private Transform  _homeParent;
        private Vector3    _homePos;
        private Quaternion _homeRot;
        private int        _originalLayer;
        private int        _inspectLayer = -1;

        private Camera Cam => targetCamera != null ? targetCamera : Camera.main;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            if (!string.IsNullOrEmpty(inspectLayerName))
            {
                _inspectLayer = LayerMask.NameToLayer(inspectLayerName);
                if (_inspectLayer < 0)
                    Debug.LogWarning($"[InspectionController] Слой '{inspectLayerName}' не найден — слой меняться не будет.");
            }
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            if (!IsInspecting || _isAnimating) return;

            bool wantExit = (exitOnRightClick && InputCompat.RightClickDown)
                         || (exitOnEscape     && InputCompat.KeyDown(KeyCode.Escape));

            if (wantExit) EndInspect();
        }

        // ════════════════════════════════════════════════════════
        // Взять блюдо в руки
        // ════════════════════════════════════════════════════════
        public void BeginInspect(FoodItem item)
        {
            if (item == null || IsInspecting || _isAnimating) return;

            if (holdAnchor == null)
            {
                Debug.LogError("[InspectionController] Не назначен holdAnchor!");
                return;
            }

            Current = item;
            item.CurrentState = FoodItem.State.Moving;

            // запоминаем куда вернуть
            _homeParent    = item.transform.parent;
            _homePos       = item.transform.position;
            _homeRot       = item.transform.rotation;
            _originalLayer = item.gameObject.layer;

            if (_inspectLayer >= 0) SetLayerRecursive(item.gameObject, _inspectLayer);
            SetPlayerControls(false);

            item.transform.SetParent(holdAnchor, true);

            _isAnimating = true;
            DOTween.Sequence().SetUpdate(true)
                .Join(item.transform.DOLocalMove(item.inspectPositionOffset, moveDuration).SetEase(moveEase))
                .Join(item.transform.DOLocalRotate(item.inspectRotationEuler, moveDuration).SetEase(moveEase))
                .Join(item.transform.DOScale(item.TableScale * item.inspectScale, moveDuration).SetEase(moveEase))
                .OnComplete(() =>
                {
                    _isAnimating = false;
                    item.CurrentState = FoodItem.State.Inspected;

                    item.Manipulation.BeginInspect(holdAnchor, Cam);

                    if (slidePanel != null && item.Food != null)
                        slidePanel.Open(item.Food);

                    onInspectStarted?.Invoke(item);
                });
        }

        // ════════════════════════════════════════════════════════
        // Положить блюдо обратно на стол
        // ════════════════════════════════════════════════════════
        public void EndInspect()
        {
            if (!IsInspecting || _isAnimating) return;

            var item = Current;
            item.Manipulation.EndInspect();

            if (slidePanel != null) slidePanel.Close();

            item.CurrentState = FoodItem.State.Moving;
            item.transform.SetParent(_homeParent, true);

            _isAnimating = true;
            DOTween.Sequence().SetUpdate(true)
                .Join(item.transform.DOMove(_homePos, moveDuration).SetEase(moveEase))
                .Join(item.transform.DORotateQuaternion(_homeRot, moveDuration).SetEase(moveEase))
                .Join(item.transform.DOScale(item.TableScale, moveDuration).SetEase(moveEase))
                .OnComplete(() =>
                {
                    if (_inspectLayer >= 0) SetLayerRecursive(item.gameObject, _originalLayer);

                    item.CurrentState = FoodItem.State.OnTable;
                    _isAnimating = false;
                    Current      = null;

                    SetPlayerControls(true);
                    onInspectEnded?.Invoke(item);
                });
        }

        // ════════════════════════════════════════════════════════
        // Утилиты
        // ════════════════════════════════════════════════════════
        private void SetPlayerControls(bool enabledState)
        {
            if (disableWhileInspecting == null) return;
            foreach (var mb in disableWhileInspecting)
                if (mb != null) mb.enabled = enabledState;
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }
    }
}
