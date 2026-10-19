using UnityEngine;
/*
[RequireComponent(typeof(LineRenderer))]
public class TableLine1 : MonoBehaviour
{
    public Transform startPoint;   // перетащите сюда Start из иерархии
    public Transform endPoint;     // и End

    public float cellSize = 1f;
    public float sphereRadius = 0.2f;
    public Color cellColor = Color.yellow;
    public Transform BoxColliderTransform;

    // Берём точки из трансформов, если они назначены, иначе из полей
    public Vector3 Start => startPoint ? startPoint.position : start;
    public Vector3 End => endPoint ? endPoint.position : end;

    [SerializeField] Vector3 start;
    [SerializeField] Vector3 end;

    public float Length => Vector3.Distance(start, end);
    public Vector3 Direction => (end - start).normalized;
    public int CellCount => Mathf.FloorToInt(Length / cellSize);

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(start, end);
        DrawCells();
    }

    void DrawCells()
    {
        Gizmos.color = cellColor;
        for (int i = 0; i < CellCount; i++)
        {
            Vector3 center = start + Direction * ((i + 0.5f) * cellSize);
            Gizmos.DrawSphere(center, sphereRadius);
        }
    }

    void Update()
    {
        CreateCellColliders();
    }

    void CreateCellColliders()
    {
        for (int i = 0; i < CellCount; i++)
        {
            Vector3 center = start + Direction * ((i + 0.5f) * cellSize);

            GameObject cell = new GameObject("CellCollider_" + i);
            BoxCollider box = cell.AddComponent<BoxCollider>();
            box.size = Vector3.one * sphereRadius * 2f;

            if (BoxColliderTransform != null)
            {
                // Сначала назначаем родителя без сохранения мировой позиции,
                // затем переводим мировую точку в локальные координаты родителя
                cell.transform.SetParent(BoxColliderTransform, false);
                cell.transform.localPosition = BoxColliderTransform.InverseTransformPoint(center);
            }
            else
            {
                cell.transform.position = center;
            }
        }
    }
}*/