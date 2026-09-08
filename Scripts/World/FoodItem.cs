// FoodItem.cs
// Компонент на 3D-префабе блюда (FoodData.prefab3D).
// Заменяет связку ItemSlot + TastedItem из 2D-версии.
//
// Держит: ссылку на FoodData, отроленный RuntimeFood, свою точку на столе.
// Реализует IInteractable — игрок кликает по нему и оно уезжает в руки.
//
// Ничего вручную вешать не надо: FoodSpawner3D добавляет компонент сам,
// коллайдер и подсветка тоже добавляются автоматически, если их нет.

using UnityEngine;

namespace Degustation
{
    [DisallowMultipleComponent]
    public class FoodItem : MonoBehaviour, IInteractable
    {
        public enum State
        {
            OnTable,    // стоит на столе, можно взять
            Moving,     // летит к камере или обратно
            Inspected   // в руках, работает Manipulation
        }

        [Header("Данные (заполняет FoodSpawner3D)")]
        [SerializeField] private FoodData foodData;

        [Header("Как блюдо лежит в руках")]
        [Tooltip("Локальное смещение относительно точки удержания перед камерой")]
        public Vector3 inspectPositionOffset = Vector3.zero;

        [Tooltip("Локальный поворот при осмотре — разверни блюдо «лицом» к игроку")]
        public Vector3 inspectRotationEuler = Vector3.zero;

        [Tooltip("Множитель масштаба в руках. 1 = как на столе")]
        [Range(0.1f, 5f)] public float inspectScale = 1f;

        // ── Публичное состояние ──────────────────────────────────
        public FoodData           Food      => foodData;
        public RuntimeFood        Runtime   { get; private set; }
        public FoodPlacementPoint HomePoint { get; private set; }
        public Manipulation       Manipulation => _manipulation;
        public Vector3            TableScale   => _tableScale;

        public State CurrentState { get; internal set; } = State.OnTable;

        // ── IInteractable ────────────────────────────────────────
        public string DisplayName => foodData != null ? foodData.foodName : name;
        public bool   CanInteract => CurrentState == State.OnTable;

        private FoodHighlight _highlight;
        private Manipulation  _manipulation;
        private Vector3       _tableScale;

        private void Awake()
        {
            _tableScale = transform.localScale;

            _highlight = GetComponent<FoodHighlight>();
            if (_highlight == null) _highlight = gameObject.AddComponent<FoodHighlight>();

            _manipulation = GetComponent<Manipulation>();
            if (_manipulation == null) _manipulation = gameObject.AddComponent<Manipulation>();
            _manipulation.enabled = false; // включится только в руках

            EnsureCollider();
        }

        /// Вызывает FoodSpawner3D сразу после Instantiate
        public void Init(FoodData data, RuntimeFood runtime, FoodPlacementPoint home)
        {
            foodData  = data;
            Runtime   = runtime;
            HomePoint = home;
            name      = $"[Food] {DisplayName}";
        }

        public void OnHoverEnter() => _highlight.SetHighlighted(CanInteract);
        public void OnHoverExit()  => _highlight.SetHighlighted(false);

        public void OnInteract()
        {
            if (!CanInteract) return;
            _highlight.SetHighlighted(false);

            if (InspectionController.Instance == null)
            {
                Debug.LogWarning("[FoodItem] Нет InspectionController на сцене!");
                return;
            }
            InspectionController.Instance.BeginInspect(this);
        }

        // ════════════════════════════════════════════════════════
        // Коллайдер обязателен для raycast-клика.
        // Если художник отдал модель без коллайдера — строим box
        // по границам мешей.
        // ════════════════════════════════════════════════════════
        private void EnsureCollider()
        {
            if (GetComponentInChildren<Collider>() != null) return;

            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[FoodItem] {name}: нет ни коллайдера, ни рендереров — кликнуть не получится.");
                return;
            }

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            var box  = gameObject.AddComponent<BoxCollider>();
            box.center = transform.InverseTransformPoint(b.center);

            Vector3 localSize = transform.InverseTransformVector(b.size);
            box.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        }
    }
}
