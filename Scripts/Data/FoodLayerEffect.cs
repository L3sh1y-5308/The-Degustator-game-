// FoodLayerEffect.cs
// Вешается на любой дочерний спрайт внутри префаба еды.
// Хранит: какой эффект несёт этот слой + каким чувством лучше всего проверять.
//
// КАК ИСПОЛЬЗОВАТЬ:
//   1. Собери префаб еды сам (пустой родитель + дочерние SpriteRenderer-ы)
//   2. На отравленный спрайт-слой добавь этот компонент
//   3. Назначь effectProfile (FoodEffect SO) и bestSense
//   4. Система инспекции читает его через GetComponent<FoodLayerEffect>()

using UnityEngine;

namespace Degustation
{
    public class FoodLayerEffect : MonoBehaviour
    {
        [Header("Эффект этого слоя")]
        [Tooltip("FoodEffect SO — яды/болезни которые несёт этот спрайт")]
        public FoodEffect effectProfile;

        [Header("Лучшее чувство для обнаружения")]
        [Tooltip("Каким SenseType эффективнее всего проверять этот слой")]
        public SenseType bestSense = SenseType.Vision;

        [Header("Состояние (рантайм)")]
        [HideInInspector] public bool isRevealed = false;

        // Есть ли вообще эффект на этом слое
        public bool HasEffect => effectProfile != null;

        // Проверить слой нужным чувством
        // Возвращает true если чувство подходит и эффект найден
        public bool Inspect(SenseType usedSense)
        {
            if (!HasEffect) return false;
            isRevealed = true;
            return usedSense == bestSense;
        }
    }
}
