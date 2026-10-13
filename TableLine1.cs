using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class TableLine1 : MonoBehaviour
{
    public Vector3 start;
    public Vector3 end;

    public float cellSize = 1f;
    public float sphereRadius = 0.2f;
    public Color cellColor = Color.yellow;
    public Transform BoxColliderTransform;

    public float Length => Vector3.Distance(start, end);
    public Vector3 Direction => (end - start).normalized;
    public int CellCount => Mathf.FloorToInt(Length / cellSize);

    void OnDrawGizmos()
    {
     DrawCells();
    }
    // Draw only in the editor Scene view (never in build/in-game)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(start, end);
    }

    void DrawCells()
    {
        int count = CellCount;
        for (int i = 0; i < count; i++)
        {
            Vector3 center = start + Direction * ((i + 0.5f) * cellSize);
            
            Gizmos.DrawSphere(center, sphereRadius);
        }
    }

    void Start()
    {
        CreateCellColliders();
    }

    void CreateCellColliders()
    {
        int count = CellCount;
        for (int i = 0; i < count; i++)
        {
            // Calculate the center position for the cell
            Vector3 center = start + Direction * ((i + 0.5f) * cellSize);

            // Create a new GameObject for the cell collider
            GameObject cellColliderObject = new GameObject("CellCollider_" + i);
            cellColliderObject.transform.position = center;

            // Add BoxCollider component
            BoxCollider boxCollider = cellColliderObject.AddComponent<BoxCollider>();

            // Set the size of the BoxCollider (based on sphereRadius)
            boxCollider.size = new Vector3(sphereRadius * 2, sphereRadius * 2, sphereRadius * 2);

            // Optional: Set material or color if needed
            // Material material = new Material(Shader.Find("Standard"));
            // material.color = cellColor;
            // boxCollider.material = material;

            // Optional: Set parent if needed
            if (BoxColliderTransform != null)
            {
                cellColliderObject.transform.SetParent(BoxColliderTransform);
                cellColliderObject.transform.localPosition = Vector3.zero;
            }
        }
    }

}
