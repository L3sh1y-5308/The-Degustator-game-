using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class FoodList : MonoBehaviour
{
    public List<GameObject> foodPrefabs;
    public int menuSize = 3;
    public ScriptableObject levelType;


    public List<GameObject> GenerateList()
    {
        var menu = new List<GameObject>();

        if (foodPrefabs == null || foodPrefabs.Count == 0)
        {
            Debug.LogWarning("The list is empty");
            return menu;
        }

        var pool = new List<GameObject>(foodPrefabs);
        int count = Mathf.Min(menuSize, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            menu.Add(pool[index]);
            pool.RemoveAt(index);

        }
        return menu;
      // To redu

    }

    public void GenerateMenu()
    {
        //List<FoodData> food = currentLevel.avaibleFood;



    }



    public List<string> LocationName;
  /*
    public List<string> LevelType()
    {
        if (LocationName == null || LocationName.Count == 0)
        {
            Debug.LogWarning("The is no level");
            return null;

        }

    }
  */
}
