using Degustation;
using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{ 
public class RoundPlan
{
    public string locationName;
    public List<FoodData> dishes = new List<FoodData>();
    public List<FoodEffect> modifiers = new List<FoodEffect>();
}
}
