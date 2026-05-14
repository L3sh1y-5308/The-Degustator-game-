// RuntimeFood.cs
// Рантайм-экземпляр блюда — создаётся через FoodData.Roll() при спавне.
// Хранит ссылку на шаблон + случайно выбранные эффекты.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    // ──────────────────────────────────────────────
    // Типы эффектов блюда
    // ──────────────────────────────────────────────
    public enum EffectType
    {
        None,
        Poison,
        Disease,
        Fire,
        Custom
    }

    // ──────────────────────────────────────────────
    // Один эффект с силой и опциональным label
    // ──────────────────────────────────────────────
    [Serializable]
    public class EffectEntry
    {
        public EffectType effectType;
        public string     customLabel = "";

        [Range(0, 10)]
        public int strength = 1;

        // Активен ли эффект (можно отключать в рантайме)
        public bool IsActive = true;

        public override string ToString() =>
            effectType == EffectType.Custom
                ? $"Custom({customLabel}, str={strength})"
                : $"{effectType}(str={strength})";
    }

    // ──────────────────────────────────────────────
    // Рантайм-экземпляр блюда
    // ──────────────────────────────────────────────
    public class RuntimeFood
    {
        // Шаблон, из которого был создан экземпляр
        public FoodData source;

        // Случайно выбранные активные эффекты
        public List<EffectEntry> rolledEffects = new();

        public RuntimeFood(FoodData src)
        {
            source = src;
        }
    }
}
