using System.Collections.Generic;
using UnityEngine;
/*
namespace Degustation
{
    public class FoodServant : MonoBehaviour
    {
        [Header("Настройки Меню")]
        [SerializeField] private FoodMenu foodMenu; // SO со списком всех блюд в игре

        [Header("Настройки Стола и Спавна")]
        [SerializeField] private Transform spawnPoint; // 
        [SerializeField] private Transform targetTable; // К какому столу нести
        [SerializeField] private Animator tableAnimator; // Ссылка на аниматор стола

        // Наш текущий список блюд для раунда (уже сгенерированный)
        private List<RuntimeFood> currentRoundDishes = new List<RuntimeFood>();

        void Start()
        {
            // Симуляция игрового цикла:
            GenerateRandomRoundList(3); // 1. Выбираем 3 случайных блюда
            EnchantAllDishes();         // 2. Накладываем и проверяем чары
            ServeDishes();              // 3. Спавним и запускаем логику движения
        }

        // --- [void Random list dishes] ---
        // Выбираем случайные блюда из общего списка SO и превращаем в RuntimeFood
        public void GenerateRandomRoundList(int countToSpawn)
        {
            if (foodMenu == null || foodMenu.availableFood.Count == 0) return;

            currentRoundDishes.Clear();

            for (int i = 0; i < countToSpawn; i++)
            {
                // Берем случайный индекс из базы данных
                int randomIndex = Random.Range(0, foodMenu.availableFood.Count);
                FoodData randomData = foodMenu.availableFood[randomIndex];

                // Роллим базовые эффекты (яды/болезни из вашего метода Roll)
                RuntimeFood runtimeFood = randomData.Roll();
                currentRoundDishes.Add(runtimeFood);
            }

            Debug.Log($"[Servant] Сформирован список из {currentRoundDishes.Count} случайных блюд.");
        }

        // --- [void Enchant Food { get list } ] ---
        // Проходимся по созданному списку и заставляем работать логику "Random enchant"
        private void EnchantAllDishes()
        {
            foreach (RuntimeFood dish in currentRoundDishes)
            {
                ApplyAndValidateEnchant(dish);
            }
        }

        // --- [Random inchant + Re-random logic] ---
        // Накладываем чары и проверяем их. Если некорректно — роллим заново!
        private void ApplyAndValidateEnchant(RuntimeFood dish)
        {
            bool isEnchantValid = false;
            int safetyCounter = 0; // Защита от бесконечного цикла, если ни одни чары не подходят

            EffectType rolledEnchant = EffectType.Poison; // Сюда запишем выпавший энчант

            while (!isEnchantValid && safetyCounter < 100)
            {
                safetyCounter++;

                // 1. Рандомим чары (например, выбираем случайный тип эффекта)
                // (Замените этот Random на вашу собственную логику или список возможных чар)
                rolledEnchant = (EffectType)Random.Range(0, System.Enum.GetValues(typeof(EffectType)).Length);

                // 2. { check if dish can contain that enchant by list }
                // Проверяем, подходит ли этот энчант нашему блюду. 
                // Например, проверяем совместимость через целевое чувство блюда (targetSense)
                isEnchantValid = CheckEnchantCompatibility(dish.sourceData, rolledEnchant);

                if (!isEnchantValid)
                {
                    // [Enchant was incorect (Re-random that dish enhant and check it agen)]
                    Debug.LogWarning($"[Re-Random] Энчант {rolledEnchant} не подошел для {dish.sourceData.foodName}. Перебрасываем...");
                }
            }

            // Если чары прошли проверку, добавляем их в список эффектов блюда
            dish.rolledEffects.Add(new EffectEntry
            {
                effectType = rolledEnchant,
                customLabel = "Enchanted " + rolledEnchant.ToString(),
                strength = Random.Range(3, 6) // Сильные чары
            });

            Debug.Log($"[Servant] Успешно наложен чар {rolledEnchant} на блюдо {dish.sourceData.foodName}");
        }

        // Ваша кастомная функция проверки совместимости
        private bool CheckEnchantCompatibility(FoodData data, EffectType enchant)
        {
            // Пример правила: На еду, проверяемую Зрением (Vision), нельзя накладывать определенный тип магии
            if (data.targetSense == SenseType.Vision && enchant == EffectType.Disease)
                return false; // Несовместимы, вернет false и запустит Re-random

            return true; // Всё отлично
        }

        // --- [void ServeDishes] ---
        public void ServeDishes()
        {
            foreach (RuntimeFood dish in currentRoundDishes)
            {
                FoodData data = dish.sourceData;

                // 1. { Spawn.Food {curent List} }
                if (data.prefab3D != null && spawnPoint != null)
                {
                    Vector3 spawnPos = spawnPoint.position + data.placementOffset;
                    GameObject spawnedFood = Instantiate(data.prefab3D, spawnPos, spawnPoint.rotation);

                    // 2. (Transform animation logic of movement)
                    // Здесь запускается логика перемещения еды от слуги к столу.
                    // Для плавного перемещения физического объекта лучше использовать DOTween 
                    // или плавный MoveTowards в отдельном скрипте на самой еде.
                    StartCoroutine(MoveFoodToTable(spawnedFood.transform, targetTable.position));
                }
            }

            // 3. { FindTheTabel.Tabel.PlayAnimation }
            if (tableAnimator != null)
            {
                tableAnimator.SetTrigger("OnFoodServed"); // Запуск анимации стола (например, тряска или подсвечивание)
            }
        }

        // Корутина для плавного перемещения блюда к столу
        private System.Collections.IEnumerator MoveFoodToTable(Transform foodTransform, Vector3 targetPosition)
        {
            float speed = 5f;
            while (foodTransform != null && Vector3.Distance(foodTransform.position, targetPosition) > 0.05f)
            {
                foodTransform.position = Vector3.MoveTowards(foodTransform.position, targetPosition, speed * Time.deltaTime);
                yield return null; // Ждем следующий кадр
            }

            if (foodTransform != null) foodTransform.position = targetPosition;
            Debug.Log("[Servant] Блюдо доставлено на стол!");
        }
    }
}
*/