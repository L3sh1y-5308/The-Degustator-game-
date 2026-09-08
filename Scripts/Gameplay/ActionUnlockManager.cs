// ActionUnlockManager.cs
// MonoBehaviour — хранит, какие суб-действия разблокированы у игрока
// Читает стартовые из SO, обновляет при покупке свитка

using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;

namespace Degustation
{
    public class ActionUnlockManager : MonoBehaviour
    {
        [Header("Все возможные суб-действия в игре")]
        [SerializeField] private List<SubActionData> allActions;

        // Runtime-словарь: SubActionData → разблокировано
        private Dictionary<SubActionData, bool> _unlocked = new();

        private void Awake()
        {
            foreach (var action in allActions)
            {
                // Стартовые разблокированы сразу, свитковые — нет
                _unlocked[action] = action.unlockType == UnlockType.Starter;
            }
        }

        // Вызвать после покупки свитка в магазине
        public void UnlockViaScroll(ScrollItemData scroll)
        {
            if (scroll.unlocksAction != null && _unlocked.ContainsKey(scroll.unlocksAction))
                _unlocked[scroll.unlocksAction] = true;
        }

        public bool IsUnlocked(SubActionData action)
            => _unlocked.TryGetValue(action, out bool val) && val;

        // Получить все разблокированные действия для конкретного чувства
        public List<SubActionData> GetUnlockedForSense(SenseType sense)
        {
            var result = new List<SubActionData>();
            foreach (var kvp in _unlocked)
            {
                if (kvp.Value && kvp.Key.senseType == sense)
                    result.Add(kvp.Key);
            }
            return result;
        }

        // Сохраняем список разоблокированных actionId
        public void Save()
        {
            var unlockedIds = new List<string>();
            foreach (var kvp in _unlocked)
            {
                if (kvp.Value) 
                    unlockedIds.Add(kvp.Key.actionId);
            }

            SaveGame.Save("unlocked_actions", unlockedIds.ToArray());
        }

        // Загружаем список разблокированных actionId
        public void Load()
        {
            var ids = SaveGame.Load<string[]>("unlocked_actions", new string[0]);
            var idSet = new HashSet<string>(ids);

            foreach (var action in allActions)
            {
                _unlocked[action] = idSet.Contains(action.actionId) 
                                    || action.unlockType == UnlockType.Starter;
            }
        }
    }
}
