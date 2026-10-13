using System.Collections.Generic;
using UnityEngine;

public class TableLine : MonoBehaviour
{
    [SerializeField] private Transform start;   
    [SerializeField] private Transform end;     
    [SerializeField] private float height = 0.5f; // высота столешницы над start/end

    // t: 0 = начало, 1 = конец
    public Vector3 GetPoint(float t)
    {
        t = Mathf.Clamp01(t);
        return Vector3.Lerp(start.position, end.position, t) + Vector3.up * height;
    }

    public void Distribute(IList<Dish> dishes, float margin = 0.1f)
    {
        int n = dishes.Count;
        for (int i = 0; i < n; i++)
        {
            float t = n == 1 ? 0.5f : Mathf.Lerp(margin, 1f - margin, i / (float)(n - 1));
            dishes[i].transform.position = GetPoint(t);
        }
    }
}


public class Dish : MonoBehaviour
{
    [SerializeField] private TableLine table;
    [Range(0f, 1f)][SerializeField] private float position;

    private void OnValidate() => Apply(); // обновляется прямо в редакторе

    public void Apply()
    {
        if (table != null)
            transform.position = table.GetPoint(position);
    }

    // Равномерно распределить N блюд по столу с отступом от краёв


}