// FoodPlacementPoint.cs
// Точка на столе, куда может встать одно блюдо.
// Просто пустой GameObject-ребёнок стола с этим компонентом.
// Заменяет старую структуру PlacementPointData из 2D-версии.

using UnityEngine;

namespace Degustation
{
    [DisallowMultipleComponent]
    public class FoodPlacementPoint : MonoBehaviour
    {
        [Tooltip("Смещение точки спавна относительно объекта (локальные координаты)")]
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;

        [Tooltip("Ставить блюдо по повороту точки (иначе — без поворота)")]
        [SerializeField] private bool alignToPointRotation = true;

        [Header("Gizmo")]
        [SerializeField] private float gizmoRadius = 0.06f;

        /// Блюдо, стоящее в этой точке (null = свободно)
        public FoodItem Current { get; private set; }

        public bool IsFree => Current == null;

        public Vector3    SpawnPosition => transform.TransformPoint(spawnOffset);
        public Quaternion SpawnRotation => alignToPointRotation ? transform.rotation : Quaternion.identity;

        public void Occupy(FoodItem item) => Current = item;
        public void Release()             => Current = null;

        private void OnDrawGizmos()
        {
            Gizmos.color = IsFree ? new Color(0.3f, 1f, 0.4f, 0.9f) : new Color(1f, 0.4f, 0.3f, 0.9f);
            Gizmos.DrawWireSphere(SpawnPosition, gizmoRadius);
            Gizmos.DrawRay(SpawnPosition, transform.up * gizmoRadius * 2f);
        }
    }
}
