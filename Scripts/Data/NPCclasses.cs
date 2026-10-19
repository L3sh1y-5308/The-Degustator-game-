using System;
using System.Collections.Generic;
using System.Linq;
using Degustation;
using Degustator;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCclasses
{
    [Serializable]
    public abstract class NpcSettings 
    {
     public string npcName;
    }

    //The NPC's Tools
    [Serializable]
    public static class RandomPicker
    {
        // T — everything that can be picked from a list, e.g. FoodData, ShopItemSO, etc.
        public static List<T> PickUnique<T>(IReadOnlyList<T> source, int min, int max)
        {
            var result = new List<T>();
            if (source == null || source.Count == 0) return result;

            int count = Mathf.Min(Random.Range(min, max + 1), source.Count);
            var pool = new List<T>(source);   // copy of source to shuffle

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(i, pool.Count);
                (pool[i], pool[index]) = (pool[index], pool[i]);
                result.Add(pool[i]);
            }
            return result;
        }
    }
    [Serializable]
    public static class FoodFilter
    {
        public static List<FoodData> ForLevel(IEnumerable<FoodData> all, FoodBiom.CastleLevel level)
            => all.Where(f => f.Level.Contains(level)).ToList();
    }

    //Th NPC's
    [Serializable]
    public class BanquetNPC : NpcSettings
    {
        [SerializeField] List<FoodData> foodOptions;
        [SerializeField] int minCount = 1;
        [SerializeField] int maxCount = 5;

        readonly List<FoodData> _chosen = new();

        void Awake()
        {
            _chosen.AddRange(RandomPicker.PickUnique(foodOptions, minCount, maxCount));
        }

        public IReadOnlyList<FoodData> Chosen => _chosen;

    }


    [Serializable]
    public class FoodEnchanter : NpcSettings
    {
        [SerializeField] BanquetNPC banquet;
        [SerializeField] int minFoods = 1;
        [SerializeField] int maxFoods = 5;
        [SerializeField] int minEnchantments = 1;
        [SerializeField] int maxEnchantments = 3;
        [SerializeField] List<FoodEffect> enchantments;

        readonly List<RuntimeFood> _enchanted = new();

        void Awake()
        {
            var foods = RandomPicker.PickUnique(banquet.Chosen, minFoods, maxFoods);

            foreach (var food in foods)
            {
                var runtime = food.Roll();
                int count = Random.Range(minEnchantments, maxEnchantments + 1);

                for (int i = 0; i < count && enchantments.Count > 0; i++)
                {
                    FoodEffect enchantment = enchantments[Random.Range(0, enchantments.Count)];
                    foreach (var poison in enchantment.poisons)
                    {
                        runtime.rolledEffects.Add(new EffectEntry
                        {
                            effectType  = EffectType.Poison,
                            customLabel = poison.ToString(),
                            strength    = Random.Range(1, 6)
                        });
                    }

                    foreach (var disease in enchantment.diseases)
                    {
                        runtime.rolledEffects.Add(new EffectEntry
                        {
                            effectType  = EffectType.Disease,
                            customLabel = disease.ToString(),
                            strength    = Random.Range(1, 4)
                        });
                    }
                }

                _enchanted.Add(runtime);
            }
        }

        public IReadOnlyList<RuntimeFood> Enchanted => _enchanted;
    }

    [Serializable]
    public class ShopOwner : NpcSettings
    {
        [SerializeField] List<ShopItemSO> shopItems; // find defn
        [SerializeField] int minCount = 1, maxCount = 5;

        readonly List<ShopItemSO> _chosen = new(); // find defn

        void Awake()
        {
            _chosen.AddRange(RandomPicker.PickUnique(shopItems, minCount, maxCount));
        }
    }
}
