using System.Collections.Generic;
using Degustation;
using UnityEngine;


[System.Serializable]
public class FoodGroup
{
    public string groupName = "Group";
    public List<FoodData> foods = new();
}

[CreateAssetMenu(fileName = "ListMenu", menuName = "Degustation/ListMenu")]
public class ListMenu : ScriptableObject
{
    public List<FoodGroup> groups = new();
}
