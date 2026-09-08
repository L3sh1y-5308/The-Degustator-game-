using UnityEngine;
using UnityEngine.Events;
using BayatGames.SaveGameFree;

// SB // Score — Счёт игрока: деньги, репутация, опыт + система уровней
[CreateAssetMenu(fileName = "PlayerScore", menuName = "SB/PlayerScore")]
public class PlayerScore : ScriptableObject
{
    [Header("Баланс")]
    [SerializeField] private float _money = 100f;
    [SerializeField] private int   _reputation = 0;   // -100..+100
    [SerializeField] private int   _xp = 0;
    [SerializeField] private int   _level = 1;

    // ── Система уровней ──────────────────────────────────────────
    [Header("Пороги уровней (XP для перехода на следующий)")]
    [Tooltip("Элемент [0] = XP для перехода с 1→2, [1] = с 2→3, и т.д.")]
    [SerializeField] private int[] _levelThresholds = { 100, 500, 1200, 2500, 5000 };

    // Публичные readonly свойства
    public float Money      => _money;
    public int   Reputation => _reputation;
    public int   XP         => _xp;
    public int   Level      => _level;

    // XP в пределах текущего уровня (от 0 до порога)
    public int   XPCurrentLevel => _xp - XPForLevel(_level);
    // Сколько XP нужно набрать для следующего уровня (0 если макс. уровень)
    public int   XPToNextLevel  => XPThresholdForLevel(_level);

    // Прогресс от 0.0 до 1.0 внутри текущего уровня
    public float LevelProgress
    {
        get
        {
            int needed = XPThresholdForLevel(_level);
            if (needed <= 0) return 1f; // максимальный уровень
            return Mathf.Clamp01((float)XPCurrentLevel / needed);
        }
    }

    public int MaxLevel => _levelThresholds.Length + 1;

    // События
    public UnityAction<float> OnMoneyChanged;
    public UnityAction<int>   OnReputationChanged;
    public UnityAction<int>   OnXPChanged;
    public UnityAction<int>   OnLevelUp;

    // ── Сохранение / Загрузка ──────────────────────────
    public void Save()
    {
        SaveGame.Save("money", _money);
        SaveGame.Save("reputation", _reputation);
        SaveGame.Save("xp", _xp);
        SaveGame.Save("level", _level);
    }

    public void Load()
    {
        float money = SaveGame.Load("money", 100f);
        int rep = SaveGame.Load("reputation", 0);
        int xp = SaveGame.Load("xp", 0);

        _money = money;
        _reputation = rep;
        _xp = xp;
        
        // Пересчет уровня
        _level = 1;
        CheckLevelUp();

        // Оповещаем подписчиков
        OnMoneyChanged?.Invoke(_money);
        OnReputationChanged?.Invoke(_reputation);
        OnXPChanged?.Invoke(_xp);
    }

    // ── Деньги ──────────────────────────────────────────
    public bool SpendMoney(float amount)
    {
        if (_money < amount) return false;
        _money -= amount;
        OnMoneyChanged?.Invoke(_money);
        return true;
    }

    public void AddMoney(float amount)
    {
        _money += amount;
        OnMoneyChanged?.Invoke(_money);
    }

    // ── Репутация ────────────────────────────────────────
    public void AddReputation(int delta)
    {
        _reputation = Mathf.Clamp(_reputation + delta, -100, 100);
        OnReputationChanged?.Invoke(_reputation);
    }

    // ── Опыт + Level Up ──────────────────────────────────
    public void AddXP(int amount)
    {
        _xp += amount;
        OnXPChanged?.Invoke(_xp);
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (_level < MaxLevel)
        {
            int xpNeededTotal = XPForLevel(_level + 1);
            if (_xp >= xpNeededTotal)
            {
                _level++;
                OnLevelUp?.Invoke(_level);
            }
            else break;
        }
    }

    public int XPForLevel(int level)
    {
        int total = 0;
        for (int i = 0; i < level - 1 && i < _levelThresholds.Length; i++)
            total += _levelThresholds[i];
        return total;
    }

    public int XPThresholdForLevel(int level)
    {
        int index = level - 1;
        if (index < 0 || index >= _levelThresholds.Length) return 0;
        return _levelThresholds[index];
    }

    public void Reset(float startMoney = 100f)
    {
        _money = startMoney;
        _reputation = 0;
        _xp = 0;
        _level = 1;
    }
}
