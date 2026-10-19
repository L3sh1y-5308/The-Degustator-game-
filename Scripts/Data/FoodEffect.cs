// FoodEffect.cs
// ScriptableObject — a list of enum values ​​for a specific food
// The manager reads from here and checks for compatibility
//
// HOW TO EXTEND:
// Added a new poison to PoisonType → it will appear in the dropdown automatically
// Added a new sense to SenseType → similarly
// Need a new data type (for example, ItemType) → add List<ItemType> here

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Degustation
{
    [CreateAssetMenu(fileName = "NewFoodEffect", menuName = "Degustation/Food Effect")]
    public class FoodEffect : ScriptableObject
    {
        [Header("Poison")]
        [Tooltip("Which poison does the food contain. Add values to PoisonType enum → appear here")]
        [FormerlySerializedAs("poison")]
        public List<PoisonosActions.PoisonType> poisons = new();

        [Header("Diseases")]
        [Tooltip("Which diseases this food may transmit")]
        public List<PoisonosActions.DiseaseType> diseases = new();

        [Header("Affecting Senses")]
        [Tooltip("Which organs/feelings are affected (excluding the main target sense)")]
        public List<SenseType> affectedSenses = new();

        [Header("Consumables related to the product")]
        [Tooltip("Which consumables interact with this food product")]
        public List<Degustator.SensoryConsumable> relatedConsumables = new();

        [Tooltip("Which sensory equipment shields against this food product")]
        public List<Degustator.SensoryEquipment> blockedByEquipment = new();
    }
}