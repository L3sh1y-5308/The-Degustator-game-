using System.Collections.Generic;
using UnityEngine;

public class TableLine : MonoBehaviour
{
    public int cellCount = 5;
    public Vector3 cellSize = new Vector3(1f, 1f, 1f);
    public float spacing = 0.2f;          
    public Color gizmoColor = Color.green;
    public bool autoRebuild = true;       

    public List<CellSlot> cells = new List<CellSlot>();

    void OnValidate()
    {
#if UNITY_EDITOR
        if (autoRebuild)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) Rebuild();
            };
        }
#endif
    }

    [ContextMenu("Rebuild Cells")]
    public void Rebuild()
    {
        ClearCells();

        float step = cellSize.x + spacing;

        for (int i = 0; i < cellCount; i++)
        {
            GameObject go = new GameObject("Cell_" + i);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(i * step, 0f, 0f);

            BoxCollider box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = cellSize;

            CellSlot slot = go.AddComponent<CellSlot>();
            slot.index = i;
            cells.Add(slot);
        }
    }

    void ClearCells()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
        cells.Clear();
    }

    public CellSlot GetFreeCell()
    {
        foreach (var cell in cells)
        {
            if (!cell.occupied) return cell;
        }
        return null;
    }

    public bool PlaceFood(GameObject food)
    {
        CellSlot cell = GetFreeCell();
        if (cell == null) return false;

        cell.occupied = true;
        cell.food = food;
        food.transform.position = cell.transform.position;
        return true;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        if (cells.Count == 0) return;

        Vector3 first = cells[0].transform.position;
        Vector3 last = cells[cells.Count - 1].transform.position;
        Gizmos.DrawLine(first, last);

        foreach (var cell in cells)
        {
            Gizmos.DrawWireCube(cell.transform.position, cellSize);
        }
    }
}

public class CellSlot : MonoBehaviour
{
    
    public int index;
    public bool occupied;
    public GameObject food;
}