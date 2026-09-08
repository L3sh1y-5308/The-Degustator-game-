using UnityEngine;
using System;

namespace Degustation
{
    [CreateAssetMenu(fileName = "InspectionPoints", menuName = "Degustation/Inspection Points")]
    public class InspectionPoints : ScriptableObject
    {
        [SerializeField] private int basePoints = 5;

        public int MaxPoints { get; private set; }
        public int UsedPoints { get; private set; }
        public int Free => MaxPoints - UsedPoints;

        public event Action OnChanged;

        public void StartSession()
        {
            MaxPoints = basePoints;
            UsedPoints = 0;
            OnChanged?.Invoke();
        }

        private void OnEnable()
        {
            StartSession(); // SO сбрасывается при каждой загрузке
        }

        // возвращает true если удалось потратить
        public bool Spend()
        {
            if (Free <= 0) return false;
            UsedPoints++;
            OnChanged?.Invoke();
            return true;
        }

        public void Refund()
        {
            if (UsedPoints <= 0) return;
            UsedPoints--;
            OnChanged?.Invoke();
        }

        // вызывается из предметов
        public void AddBonus(int amount)
        {
            MaxPoints += amount;
            OnChanged?.Invoke();
        }
    }
}