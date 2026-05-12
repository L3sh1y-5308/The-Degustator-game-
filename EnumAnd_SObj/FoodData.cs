// FoodData.cs (Roguelite edition)
// ScriptableObject — шаблон продукта питания с диапазонами для рандомизации
//
// ПАТТЕРН ДЛЯ РОГАЛИКА:
//   FoodData = шаблон (SO, не трогаем)
//   RuntimeFood = живой экземпляр с рандомизированными статами на ран
//   Такой подход стандартен: SO — источник правды, рантайм-класс — инстанс

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    // ──────────────────────────────────────────────
    // Диапазон целочисленного стата для рандомизации
    // ──────────────────────────────────────────────
    [Serializable]
    public class StatRange
    {
        [Tooltip("Минимальное значение (включительно)")]
        public int min = 0;

        [Tooltip("Максимальное значение (включительно)")]
        public int max = 100;

        public int Roll() => UnityEngine.Random.Range(min, max + 1);

        // Фиксированное значение — просто выставь min == max
        public bool IsFixed => min == max;
    }

    // ──────────────────────────────────────────────
    // Живой экземпляр еды — создаётся из FoodData при спавне
    // Хранит рандомизированные значения конкретного предмета
    // ──────────────────────────────────────────────
    [Serializable]
    public class RuntimeFood
    {
        public FoodData source;         // Ссылка на шаблон

        public int nutritionValue;
        public int stomachImpact;
        public int senseModifier;
        public List<EffectEntry> rolledEffects = new();

        // ── Вычисляемые булины ──
        // Проверяй через эти свойства, не храни отдельные bool-поля

        /// Продукт однозначно ядовит (яд >= порога)
        public bool IsPoisoned => TotalEffectStrength(EffectType.Poison) >= 5;

        /// Продукт на огне / обжигает
        public bool IsOnFire => TotalEffectStrength(EffectType.Fire) >= 1;

        /// Хотя бы один вредный эффект активен
        public bool IsHazardous
        {
            get
            {
                foreach (var e in rolledEffects)
                    if (e.IsHarmful && e.IsActive) return true;
                return false;
            }
        }

        /// Полностью безвреден (нет вредных эффектов, желудок не страдает)
        public bool IsSafe => !IsHazardous && stomachImpact >= 0;

        private int TotalEffectStrength(EffectType type)
        {
            int total = 0;
            foreach (var e in rolledEffects)
                if (e.effectType == type) total += e.strength;
            return total;
        }

        public override string ToString() =>
            $"[{source?.foodName}] nutrition={nutritionValue} stomach={stomachImpact} " +
            $"sense={senseModifier} effects={rolledEffects.Count}";
    }

    // ──────────────────────────────────────────────
    // Диапазон для эффекта — тип фиксирован, сила рандомная
    // ──────────────────────────────────────────────
    [Serializable]
    public class EffectEntryRange
    {
        public EffectType effectType = EffectType.None;
        public string customLabel = "";

        [Tooltip("Диапазон силы. min=max → фиксированное значение")]
        public StatRange strengthRange = new() { min = 0, max = 10 };

        [Tooltip("Вероятность появления (0–1). 1 = всегда")]
        [Range(0f, 1f)]
        public float spawnChance = 1f;

        [Tooltip("Длительность в секундах")]
        public float duration = 0f;

        /// Возвращает EffectEntry или null (если не прошёл шанс)
        public EffectEntry TryRoll()
        {
            if (UnityEngine.Random.value > spawnChance) return null;

            return new EffectEntry
            {
                effectType = effectType,
                customLabel = customLabel,
                strength = strengthRange.Roll(),
                duration = duration
            };
        }
    }

    // ──────────────────────────────────────────────
    // ScriptableObject — шаблон продукта
    // ──────────────────────────────────────────────
    [CreateAssetMenu(
        fileName = "NewFood",
        menuName = "Degustation/Food")]
    public class FoodData : ScriptableObject
    {
        [Header("Название")]
        public string foodName = "Food";

        [Header("Иконки")]
        [Tooltip("Маленькая иконка — для инвентаря, списков")]
        public Sprite iconSmall;

        [Tooltip("Большая иконка — для детального просмотра, тултипа")]
        public Sprite iconLarge;

        // ── Базовые параметры (диапазоны для рандомизации) ──
        [Header("Параметры (min–max для рандомайзера)")]
        [Tooltip("Питательность. Поставь min=max для фиксированного значения")]
        public StatRange nutritionRange = new() { min = 40, max = 60 };

        [Tooltip("Влияние на желудок. Отрицательный min = риск вреда")]
        public StatRange stomachRange = new() { min = 5, max = 15 };

        [Tooltip("Смещение чувств")]
        public StatRange senseModRange = new() { min = -10, max = 10 };

        public SenseType targetSense = SenseType.Vision;

        // ── Эффекты ─────────────────────────────────
        [Header("Возможные эффекты")]
        [Tooltip("Каждый эффект имеет свой шанс появления и диапазон силы")]
        public List<EffectEntryRange> possibleEffects = new();

        // ── Редкость / теги ─────────────────────────
        [Header("Рогалик-метаданные")]
        [Tooltip("Редкость влияет на спавн в LootTable")]
        [Range(0f, 1f)]
        public float spawnWeight = 1f;

        [Tooltip("Теги для фильтрации (например: Mushroom, Meat, Alchemical)")]
        public List<string> tags = new();

        // ────────────────────────────────────────────
        // Создать RuntimeFood из этого шаблона
        // Вызывай при спавне предмета или старте рогалик-рана
        // ────────────────────────────────────────────
        public RuntimeFood Roll()
        {
            var instance = new RuntimeFood
            {
                source = this,
                nutritionValue = nutritionRange.Roll(),
                stomachImpact = stomachRange.Roll(),
                senseModifier = senseModRange.Roll()
            };

            foreach (var effectRange in possibleEffects)
            {
                var rolled = effectRange.TryRoll();
                if (rolled != null)
                    instance.rolledEffects.Add(rolled);
            }

            return instance;
        }

        // ── Объединение двух RuntimeFood (крафт / смешивание) ──
        // Паттерн: суммируем статы с весами, объединяем эффекты
        // Используй для крафта или "съел два блюда подряд"
        public static RuntimeFood Combine(RuntimeFood a, RuntimeFood b, float weightA = 0.5f)
        {
            float weightB = 1f - weightA;
            return new RuntimeFood
            {
                source = a.source, // Берём источник первого
                nutritionValue = Mathf.RoundToInt(a.nutritionValue * weightA + b.nutritionValue * weightB),
                stomachImpact = Mathf.RoundToInt(a.stomachImpact * weightA + b.stomachImpact * weightB),
                senseModifier = Mathf.RoundToInt(a.senseModifier * weightA + b.senseModifier * weightB),
                rolledEffects = MergeEffects(a.rolledEffects, b.rolledEffects)
            };
        }

        private static List<EffectEntry> MergeEffects(
            List<EffectEntry> a, List<EffectEntry> b)
        {
            // Суммируем силу эффектов одного типа, остальные добавляем
            var merged = new Dictionary<EffectType, EffectEntry>();

            void AddEffect(EffectEntry e)
            {
                if (merged.TryGetValue(e.effectType, out var existing))
                    existing.strength += e.strength; // Стакаются
                else
                    merged[e.effectType] = new EffectEntry
                    {
                        effectType = e.effectType,
                        customLabel = e.customLabel,
                        strength = e.strength,
                        duration = e.duration
                    };
            }

            foreach (var e in a) AddEffect(e);
            foreach (var e in b) AddEffect(e);

            return new List<EffectEntry>(merged.Values);
        }
    }
}