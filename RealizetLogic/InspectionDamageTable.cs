// InspectionDamageTable.cs
// ScriptableObject — таблица урона по органам в зависимости от метода проверки.
//
// ПРИНЦИП:
//   Каждый эффект (EffectType) имеет список правил InspectionDamageRule.
//   Правило описывает: каким чувством проверяли → какой орган страдает → сколько урона.
//
// ПРИМЕР — осколки стекла (EffectType.Custom, label="glass"):
//   Vision  → eyeHp    = 0   (увидел — не пострадал)
//   Touch   → touchHp  = 5   (пощупал — порезал пальцы немного)
//   Taste   → mouthHp  = 20  (положил в рот — максимальный урон)
//
// КАК ИСПОЛЬЗОВАТЬ:
//   var table = Resources.Load<InspectionDamageTable>("YourAssetPath");
//   int dmg = table.GetDamage(EffectType.Custom, "glass", SenseType.Taste);
//   senseStats.TakeDamage(SenseType.Taste, dmg);
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    // ──────────────────────────────────────────────
    // Одно правило: метод → орган → базовый урон
    // ──────────────────────────────────────────────
    [Serializable]
    public class InspectionDamageRule
    {
        [Tooltip("Чем проверял игрок")]
        public SenseType inspectionMethod;

        [Tooltip("Какой орган получает урон")]
        public SenseType damagedOrgan;

        [Tooltip("Базовый урон. 0 = безопасно")]
        [Range(0, 100)]
        public int baseDamage;

        [Tooltip("Множитель от силы эффекта (strength из EffectEntry). " +
                 "Итоговый урон = baseDamage + effectStrength * strengthMultiplier")]
        [Range(0f, 5f)]
        public float strengthMultiplier = 0f;

        /// Рассчитать итоговый урон с учётом силы эффекта
        public int Calculate(int effectStrength)
            => baseDamage + Mathf.RoundToInt(effectStrength * strengthMultiplier);
    }

    // ──────────────────────────────────────────────
    // Группа правил для одного эффекта
    // ──────────────────────────────────────────────
    [Serializable]
    public class EffectDamageGroup
    {
        [Tooltip("Тип эффекта. Для Custom — используй customLabel")]
        public EffectType effectType = EffectType.None;

        [Tooltip("Только для EffectType.Custom: совпадает с EffectEntry.customLabel")]
        public string customLabel = "";

        [Tooltip("Список правил для этого эффекта")]
        public List<InspectionDamageRule> rules = new();

        // Найти правило по методу проверки
        public InspectionDamageRule FindRule(SenseType method)
            => rules.Find(r => r.inspectionMethod == method);
    }

    // ──────────────────────────────────────────────
    // ScriptableObject — вся таблица урона
    // ──────────────────────────────────────────────
    [CreateAssetMenu(
        fileName = "InspectionDamageTable",
        menuName  = "Degustation/Inspection Damage Table")]
    public class InspectionDamageTable : ScriptableObject
    {
        [Header("Группы правил по типам эффектов")]
        public List<EffectDamageGroup> groups = new()
        {
            // ── Пресет: осколки стекла (Custom / "glass") ─────────
            new EffectDamageGroup
            {
                effectType  = EffectType.Custom,
                customLabel = "glass",
                rules = new List<InspectionDamageRule>
                {
                    new() { inspectionMethod = SenseType.Vision,  damagedOrgan = SenseType.Vision,  baseDamage = 0,  strengthMultiplier = 0f },
                    new() { inspectionMethod = SenseType.Touch,   damagedOrgan = SenseType.Touch,   baseDamage = 5,  strengthMultiplier = 0.5f },
                    new() { inspectionMethod = SenseType.Smell,   damagedOrgan = SenseType.Smell,   baseDamage = 2,  strengthMultiplier = 0.2f },
                    new() { inspectionMethod = SenseType.Taste,   damagedOrgan = SenseType.Taste,   baseDamage = 20, strengthMultiplier = 1.5f },
                }
            },
            // ── Пресет: яд (Poison) ───────────────────────────────
            new EffectDamageGroup
            {
                effectType  = EffectType.Poison,
                customLabel = "",
                rules = new List<InspectionDamageRule>
                {
                    new() { inspectionMethod = SenseType.Vision,  damagedOrgan = SenseType.Vision,  baseDamage = 0,  strengthMultiplier = 0f },
                    new() { inspectionMethod = SenseType.Smell,   damagedOrgan = SenseType.Smell,   baseDamage = 3,  strengthMultiplier = 0.3f },
                    new() { inspectionMethod = SenseType.Touch,   damagedOrgan = SenseType.Touch,   baseDamage = 5,  strengthMultiplier = 0.5f },
                    new() { inspectionMethod = SenseType.Taste,   damagedOrgan = SenseType.Taste,   baseDamage = 15, strengthMultiplier = 2f   },
                }
            },
            // ── Пресет: огонь / жжение (Fire) ────────────────────
            new EffectDamageGroup
            {
                effectType  = EffectType.Fire,
                customLabel = "",
                rules = new List<InspectionDamageRule>
                {
                    new() { inspectionMethod = SenseType.Vision,  damagedOrgan = SenseType.Vision,  baseDamage = 0,  strengthMultiplier = 0f   },
                    new() { inspectionMethod = SenseType.Touch,   damagedOrgan = SenseType.Touch,   baseDamage = 10, strengthMultiplier = 1f   },
                    new() { inspectionMethod = SenseType.Smell,   damagedOrgan = SenseType.Smell,   baseDamage = 5,  strengthMultiplier = 0.5f },
                    new() { inspectionMethod = SenseType.Taste,   damagedOrgan = SenseType.Taste,   baseDamage = 20, strengthMultiplier = 2f   },
                }
            },
        };

        // ════════════════════════════════════════════════════════
        // Публичный API
        // ════════════════════════════════════════════════════════

        /// <summary>
        /// Найти группу правил для EffectEntry.
        /// Для Custom сравнивает customLabel; для остальных — effectType.
        /// </summary>
        public EffectDamageGroup FindGroup(EffectEntry effect)
        {
            foreach (var g in groups)
            {
                if (effect.effectType == EffectType.Custom)
                {
                    if (g.effectType == EffectType.Custom &&
                        string.Equals(g.customLabel, effect.customLabel, StringComparison.OrdinalIgnoreCase))
                        return g;
                }
                else if (g.effectType == effect.effectType)
                {
                    return g;
                }
            }
            return null;
        }

        /// <summary>
        /// Рассчитать урон для одного эффекта при конкретном методе проверки.
        /// Возвращает 0 если правило не найдено (безопасно).
        /// </summary>
        public int GetDamage(EffectEntry effect, SenseType inspectionMethod)
        {
            var group = FindGroup(effect);
            if (group == null) return 0;

            var rule = group.FindRule(inspectionMethod);
            if (rule == null) return 0;

            return rule.Calculate(effect.strength);
        }

        /// <summary>
        /// Применить урон от всех эффектов RuntimeFood при выбранном методе проверки.
        /// Вызывай после того, как игрок выбрал действие (Vision, Touch, Taste и т.д.)
        /// </summary>
        /// <param name="food">Рантайм-экземпляр блюда</param>
        /// <param name="inspectionMethod">Чем проверял игрок</param>
        /// <param name="stats">SO статов персонажа</param>
        public void ApplyDamage(RuntimeFood food, SenseType inspectionMethod, SenseStatsData stats)
        {
            if (food == null || stats == null) return;

            foreach (var effect in food.rolledEffects)
            {
                if (!effect.IsActive) continue;

                var group = FindGroup(effect);
                if (group == null) continue;

                var rule = group.FindRule(inspectionMethod);
                if (rule == null || rule.baseDamage == 0 && rule.strengthMultiplier == 0f) continue;

                int dmg = rule.Calculate(effect.strength);
                if (dmg > 0)
                {
                    stats.TakeDamage(rule.damagedOrgan, dmg);
                    Debug.Log($"[DamageTable] {inspectionMethod} + {effect} → -{dmg} HP ({rule.damagedOrgan})");
                }
            }
        }
    }
}