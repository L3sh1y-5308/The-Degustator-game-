// PlayerInteractor.cs
// Наведение курсора и клик по 3D-объектам (IInteractable).
// Вешается на камеру игрока или на пустышку "Player".
//
// Клики над UI игнорируются, чтобы нажатие по панели чувств
// не «проваливалось» на блюдо за ней.

using UnityEngine;
using UnityEngine.EventSystems;

namespace Degustation
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Луч")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("Максимальная дистанция взаимодействия, метры")]
        [SerializeField] private float maxDistance = 4f;

        [Tooltip("По каким слоям стрелять лучом")]
        [SerializeField] private LayerMask interactableMask = ~0;

        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Подсказка под курсором (опционально)")]
        [SerializeField] private HoverLabel hoverLabel;

        private IInteractable _hovered;
        private Camera Cam => targetCamera != null ? targetCamera : Camera.main;

        private void Update()
        {
            // пока блюдо в руках — стол не кликается
            var inspector = InspectionController.Instance;
            if (inspector != null && (inspector.IsInspecting || inspector.IsBusy))
            {
                SetHovered(null);
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                SetHovered(null);
                return;
            }

            var cam = Cam;
            if (cam == null) return;

            IInteractable found = null;

            Ray ray = cam.ScreenPointToRay(InputCompat.MousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableMask, triggerInteraction))
                found = hit.collider.GetComponentInParent<IInteractable>();

            if (found != null && !found.CanInteract) found = null;

            SetHovered(found);

            if (_hovered != null && InputCompat.LeftClickDown)
                _hovered.OnInteract();
        }

        private void OnDisable() => SetHovered(null);

        private void SetHovered(IInteractable next)
        {
            if (ReferenceEquals(_hovered, next)) return;

            _hovered?.OnHoverExit();
            _hovered = next;
            _hovered?.OnHoverEnter();

            if (hoverLabel != null) hoverLabel.Show(_hovered?.DisplayName);
        }
    }
}
