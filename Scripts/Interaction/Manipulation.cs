// Manipulation.cs
// Вращение и приближение блюда, пока оно в руках у игрока.
//
// ЧТО ИЗМЕНИЛОСЬ ПРОТИВ СТАРОЙ ВЕРСИИ:
//   • убрана проверка зоны (zoneCenter/zoneRadius) — теперь компонент
//     включает и выключает InspectionController, лишних Update нет;
//   • вращение стало относительно камеры, а не локальных осей объекта:
//     тянешь мышь вправо — блюдо крутится вправо на экране, всегда;
//   • зум больше не двигает объект по прямой к камере (объект мог
//     улететь сквозь неё), а меняет дистанцию от точки удержания
//     с жёстким клампом.
//
// Инпут читается из существующей карты GameInput → ControllOf3dObj
// (TwistHold / TwistDelta / Scroll) — новых экшенов заводить не надо.

using UnityEngine;

namespace Degustation
{
    [DisallowMultipleComponent]
    public sealed class Manipulation : MonoBehaviour
    {
        [Header("Вращение")]
        [SerializeField] private float rotateSensitivity = 0.25f;
        [SerializeField] private bool  invertY = false;

        [Header("Приближение")]
        [SerializeField] private float zoomSpeed = 0.35f;

        [Tooltip("Насколько близко можно поднести блюдо к камере (метры)")]
        [SerializeField] private float minDistance = 0.25f;

        [Tooltip("Насколько далеко можно отодвинуть (метры)")]
        [SerializeField] private float maxDistance = 1.2f;

        [Header("Плавность")]
        [SerializeField, Range(0f, 30f)] private float zoomSmoothing = 12f;

        private GameInput  _controls;
        private Transform  _anchor;      // точка удержания перед камерой
        private Camera     _camera;
        private Vector3    _baseLocalPos;
        private float      _targetDistance;
        private float      _currentDistance;

        private void Awake()
        {
            _controls = new GameInput();
            enabled = false; // включает только InspectionController
        }

        private void OnDestroy() => _controls?.Dispose();

        // ════════════════════════════════════════════════════════
        // Вызывает InspectionController когда блюдо доехало до рук
        // ════════════════════════════════════════════════════════
        public void BeginInspect(Transform holdAnchor, Camera cam)
        {
            _anchor = holdAnchor;
            _camera = cam != null ? cam : Camera.main;

            _baseLocalPos    = transform.localPosition;
            _currentDistance = Mathf.Clamp(
                Vector3.Distance(transform.position, _camera.transform.position),
                minDistance, maxDistance);
            _targetDistance  = _currentDistance;

            _controls.ControllOf3dObj.Enable();
            enabled = true;
        }

        public void EndInspect()
        {
            _controls.ControllOf3dObj.Disable();
            enabled = false;
            _anchor = null;
        }

        private void OnDisable()
        {
            // на случай выключения объекта из инспектора/сцены
            if (_controls != null) _controls.ControllOf3dObj.Disable();
        }

        private void Update()
        {
            if (_anchor == null || _camera == null) return;

            ProcessRotation();
            ProcessZoom();
        }

        // ════════════════════════════════════════════════════════
        // Вращение вокруг осей КАМЕРЫ (Space.World с векторами камеры),
        // поэтому направление мыши всегда совпадает с движением на экране
        // ════════════════════════════════════════════════════════
        private void ProcessRotation()
        {
            if (!_controls.ControllOf3dObj.TwistHold.IsPressed()) return;

            Vector2 delta = _controls.ControllOf3dObj.TwistDelta.ReadValue<Vector2>();
            if (delta.sqrMagnitude < 0.0001f) return;

            var camT = _camera.transform;
            float yaw   = -delta.x * rotateSensitivity;
            float pitch = (invertY ? -delta.y : delta.y) * rotateSensitivity;

            transform.Rotate(camT.up,    yaw,   Space.World);
            transform.Rotate(camT.right, pitch, Space.World);
        }

        // ════════════════════════════════════════════════════════
        // Зум = дистанция от камеры вдоль луча "камера → якорь".
        // Объект физически не может пролететь сквозь камеру.
        // ════════════════════════════════════════════════════════
        private void ProcessZoom()
        {
            Vector2 scroll = _controls.ControllOf3dObj.Scroll.ReadValue<Vector2>();

            if (Mathf.Abs(scroll.y) > 0.01f)
            {
                // колесо даёт ±120 на "щелчок" — нормализуем
                float step = Mathf.Sign(scroll.y) * zoomSpeed;
                _targetDistance = Mathf.Clamp(_targetDistance - step, minDistance, maxDistance);
            }

            if (Mathf.Abs(_currentDistance - _targetDistance) < 0.0005f) return;

            _currentDistance = zoomSmoothing > 0f
                ? Mathf.Lerp(_currentDistance, _targetDistance, Time.unscaledDeltaTime * zoomSmoothing)
                : _targetDistance;

            Vector3 camPos = _camera.transform.position;
            Vector3 dir    = (_anchor.TransformPoint(_baseLocalPos) - camPos).normalized;
            transform.position = camPos + dir * _currentDistance;
        }
    }
}
