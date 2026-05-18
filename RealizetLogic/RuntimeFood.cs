// RuntimeFood.cs
// Рантайм-экземпляр блюда — создаётся через FoodData.Roll() при спавне.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    public enum EffectType { None, Poison, Disease, Fire, Custom }

    [Serializable]
    public class EffectEntry
    {
        public EffectType effectType;
        public string     customLabel = "";

        [Range(0, 10)]
        public int strength = 1;

        public bool IsActive = true;

        public override string ToString() =>
            effectType == EffectType.Custom
                ? $"Custom({customLabel}, str={strength})"
                : $"{effectType}(str={strength})";
    }

    public class RuntimeFood
    {
        public FoodData          source;
        public List<EffectEntry> rolledEffects = new();

        public RuntimeFood(FoodData src) { source = src; }
    }
}
