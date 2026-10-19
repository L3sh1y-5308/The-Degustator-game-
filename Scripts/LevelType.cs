using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Degustation
{
    [CreateAssetMenu(fileName = "LevelType", menuName = "Degustation/LevelType")]
    public class LevelType : ScriptableObject
    {
        public string levelName; 
        public List<FoodData> availableFood;
        public List<FoodEffect> possibleModifiers;
    }
}
