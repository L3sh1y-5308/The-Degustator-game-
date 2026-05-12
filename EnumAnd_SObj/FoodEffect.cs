// FoodEffect.cs
// Определяет типы эффектов и SO-список эффектов для продукта питания

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    // ──────────────────────────────────────────────
    // Тип эффекта — расширяй по вкусу
    // ──────────────────────────────────────────────
    public enum EffectType
    {
        None,
        Poison,         // Яд
        Fire,           // Огонь / жжение
        Frost,          // Заморозка
        Hallucination,  // Галлюцинация
        Blind,          // Слепота
        Regen,          // Регенерация
        Stun,           // Оглушение
        Blessed,        // Благословение
        Cursed,         // Проклятие
        Custom          // Произвольный — описание в customLabel
    }

    // ──────────────────────────────────────────────
    // Один эффект: тип + сила + опциональный лейбл
    // ──────────────────────────────────────────────
    [Serializable]
    public class EffectEntry
    {
        [Tooltip("Тип эффекта")]
        public EffectType effectType = EffectType.None;

        [Tooltip("Произвольное название (только для Custom или для отображения)")]
        public string customLabel = "";

        [Tooltip("Сила эффекта — абсолютное значение, трактуй в логике игры")]
        [Range(0, 100)]
        public int strength = 0;

        [Tooltip("Длительность в секундах (0 = мгновенный)")]
        public float duration = 0f;

        // ── Вычисляемые булины ──────────────────────
        // Не храни булины отдельно — читай через свойства.
        // Это исключает рассинхрон между флагами и данными.

        public bool IsActive      => effectType != EffectType.None && strength > 0;
        public bool IsPoisonous   => effectType == EffectType.Poison   && strength > 0;
        public bool IsOnFire      => effectType == EffectType.Fire     && strength > 0;
        public bool IsHallucino   => effectType == EffectType.Hallucination && strength > 0;
        public bool IsHarmful     => effectType is EffectType.Poison
                                                 or EffectType.Fire
                                                 or EffectType.Frost
                                                 or EffectType.Blind
                                                 or EffectType.Stun
                                                 or EffectType.Cursed;
        public bool IsBeneficial  => effectType is EffectType.Regen or EffectType.Blessed;

        public override string ToString() =>
            customLabel.Length > 0
                ? $"{customLabel} x{strength}"
                : $"{effectType} x{strength}";
    }

    // ──────────────────────────────────────────────
    // SO — список эффектов (attach к FoodData или отдельно)
    // ──────────────────────────────────────────────
    [CreateAssetMenu(
        fileName = "NewFoodEffect",
        menuName = "Degustation/FoodEffect")]
    public class FoodEffect : ScriptableObject
    {
        [Header("Эффекты продукта")]
        public List<EffectEntry> effects = new();

        // Быстрые запросы ─────────────────────────

        /// Есть ли хоть один активный вредный эффект
        public bool HasHarmfulEffect
        {
            get
            {
                foreach (var e in effects)
                    if (e.IsHarmful && e.IsActive) return true;
                return false;
            }
        }

        /// Суммарная сила эффектов конкретного типа
        public int TotalStrengthOf(EffectType type)
        {
            int total = 0;
            foreach (var e in effects)
                if (e.effectType == type) total += e.strength;
            return total;
        }

        /// Все активные эффекты определённого типа
        public List<EffectEntry> GetEffects(EffectType type)
        {
            var result = new List<EffectEntry>();
            foreach (var e in effects)
                if (e.effectType == type && e.IsActive)
                    result.Add(e);
            return result;
        }
    }
}
