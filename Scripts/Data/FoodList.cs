using System.Collections.Generic;
using Degustation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;


public class RoundPlan
{
    public string locationName;
    public List<FoodData> dishes;
    public List<FoodEffect> modifiers = new List<FoodEffect>();
}

public class FoodList : MonoBehaviour
{
    public List<GameObject> foodPrefabs;
    public int menuSize = 3;
    public ScriptableObject levelType;

 
}
