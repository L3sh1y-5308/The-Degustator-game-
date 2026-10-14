using System.Collections;
using System.Collections.Generic;
using Degustation;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public LevelType levelType;
    public FoodEnchanter enchanter;
    public FoodTable table;

    [Min(1)] public int dishesPerRound = 5;
    [Min(0)] public int modifiersPerRound = 2;

    IEnumerator RunRound(RoundPlan plan)
    {
        List<FoodData> prepared = enchanter.Apply(plan);

        yield return table.Serve(prepared, plan.locationName);

    }

    static List<T> PickRandom<T>(IReadOnlyList<T> source, int count)
    {
        var result = new List<T>();
        if (source == null || source.Count == 0) return result;

        var pool = new List<T>(source);
        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return result;
    }



}
