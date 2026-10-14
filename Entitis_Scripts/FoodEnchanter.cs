using System.Collections;
using System.Collections.Generic;
using Degustation;
using UnityEngine;

public class FoodEnchanter : MonoBehaviour
{
    // Получает план и решает, как каждый модификатор ложится на блюда
    public List<FoodData> Apply(RoundPlan plan)
    {
        foreach (var effect in plan.modifiers)
        {
            // Выбрать блюдо и применить effect, например через FoodInstance
        }
        return plan.dishes;
    }
}

public class FoodTable : MonoBehaviour
{
    public IEnumerator Serve(List<FoodData> dishes, string location)
    {
        foreach (var dish in dishes)
        {
            // Spawn dish.prefab3D на столе
            yield return null;
        }
    }
}